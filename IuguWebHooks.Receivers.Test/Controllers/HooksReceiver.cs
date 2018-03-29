using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace IuguWebHooks.Receivers.Test.Controllers
{
    public class HooksReceiver : WebHookHandler
    {
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            if ("invoice.created".Equals(context.Actions.ElementAt(0), System.StringComparison.CurrentCultureIgnoreCase))
            {
                var data = context.GetDataOrDefault<test>();
            }

            return Task.FromResult(true);
        }

        public class test
        {
            [JsonProperty("event")]
            public string Acao { get; set; }

            //[JsonProperty("data[id]")]
            [JsonProperty("data.id")]
            public string Id { get; set; }

            //[JsonProperty("data[status]")]
            [JsonProperty("data.status")]
            public string Status { get; set; }

            //[JsonProperty("data[account_id]")]
            [JsonProperty("data.account_id")]
            public string AccountId { get; set; }

            //[JsonProperty("data[subscription_id]")]
            [JsonProperty("data.subscription_id")]
            public string SubscriptionId { get; set; }
        }
    }
}