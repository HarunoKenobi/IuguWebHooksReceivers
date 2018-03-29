using Microsoft.AspNet.WebHooks.Config;
using System.ComponentModel;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtension
    {
        public static void InitializeIuguWebHooksReceiver(this HttpConfiguration configuration)
        {
            WebHooksConfig.Initialize(configuration);
        }
    }
}
