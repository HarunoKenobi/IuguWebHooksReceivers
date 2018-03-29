using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace IuguWebHooks.Receivers
{
    public class IuguWebHookReceiver : WebHookReceiver
    {
        public override string Name { get => ReceiverName; }

        internal const string ReceiverName = "iugu";
        internal const string DefaultAction = "none";

        internal const string ConfigToken = "IubuWebHook_SecretKey";




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
            //await EnsureValidCode(request, id);

            this.EnsureValidBasicAuthenticationToken(request);

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

        /// <summary>
        /// Method based on [Microsoft.AspNet.WebHooks.WebHookReceiver.EnsureValidCode] method
        /// </summary>
        /// <param name="request"></param>
        private void EnsureValidBasicAuthenticationToken(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            this.EnsureSecureConnection(request);

            var secretKeyRequested = request.Headers?.Authorization?.Scheme ?? string.Empty;

            if (string.IsNullOrEmpty(secretKeyRequested))
            {
                var message = string.Format(CultureInfo.CurrentCulture, "The WebHook verification request must contain a '{0}' query parameter.", ConfigToken);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(message);
                var noCode = request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                throw new HttpResponseException(noCode);
            }

            var secretKey = this.GetReceiverConfig(request);

            if (!WebHookReceiver.SecretEqual(secretKeyRequested, secretKey))
            {
                var message = string.Format(CultureInfo.CurrentCulture, "The '{0}' query parameter provided in the HTTP request did not match the expected value.", ConfigToken);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(message);
                var invalidCode = request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                throw new HttpResponseException(invalidCode);
            }
        }

        public string GetReceiverConfig(HttpRequestMessage request)
        {
            var secretKey = ConfigurationManager.AppSettings[ConfigToken];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                if (secretKey.Length < 32 || secretKey.Length > 128)
                {
                    var message = string.Format(CultureInfo.CurrentCulture, "Could not find a valid configuration for WebHook receiver '{0}'. The setting must be set to a value between {1} and {2} characters long.", ConfigToken, 32, 128);
                    request.GetConfiguration().DependencyResolver.GetLogger().Error(message);
                    var noSecret = request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
                    throw new HttpResponseException(noSecret);
                }
            }

            return secretKey;
        }
    }
}
