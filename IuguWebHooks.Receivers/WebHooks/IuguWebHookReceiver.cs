using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace IuguWebHooks.Receivers
{
    public class IuguWebHookReceiver : WebHookReceiver
    {
        public override string Name { get => ReceiverName; }

        internal const string ReceiverName = "iugu";
        internal const string DefaultAction = "none";




        public override async Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Method != HttpMethod.Post)
            {
                return CreateBadMethodResponse(request);
            }

            // Ensure that we use https and have a valid code parameter
            await EnsureValidCode(request, id);

            // Read the request entity body
            var data = await ReadAsFormDataAsync(request);

            //Modifying the original form_data
            var string_json = this.ConvertToJson(data);

            //Converting the string json to dynamic json removing '\r\n'
            var dynamic_json = JsonConvert.DeserializeObject<dynamic>(string_json.Replace(@"\r\n", string.Empty));

            // Call registered handlers
            return await ExecuteWebHookAsync(id, context, request, new[] { (string)(dynamic_json?.@event ?? DefaultAction) }, dynamic_json);
        }

        private string ConvertToJson(NameValueCollection data)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var key in data.AllKeys)
            {
                if (!key.Contains("["))
                    dictionary.Add(key, data[key]);
                else
                    dictionary.Add(key.Replace("[", ".").Replace("]", string.Empty), data[key]);
            }

            return JsonConvert.SerializeObject(dictionary, Formatting.None);
        }
    }
}
