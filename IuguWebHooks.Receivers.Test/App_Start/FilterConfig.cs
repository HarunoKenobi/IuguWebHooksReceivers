using System.Web;
using System.Web.Mvc;

namespace IuguWebHooks.Receivers.Test
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
