using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Tuhu.Component.Framework;

namespace Tuhu.YeWu.TenGu
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ThreadIdentityActionFilterAttribute());
            filters.Add(new HandleErrorByLoggingAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "ZeroActivity", // Route name
                "{controller}/{action}" // URL with parameters
            );
        }

        protected void Application_Start()
        {
            DistributedCache.Initialize();
            Component.Framework.CacheManagement.CacheFactory.Initialize();

           // Qiniu.Conf.Config.Init();
            MVCControlsToolkit.Core.Extensions.Register();//this is the line of code to add
            AreaRegistration.RegisterAllAreas();
            MVCControlsToolkit.Core.ClientDataTypeModelValidatorProviderExt.NumericErrorKey = "NumericError";
            MVCControlsToolkit.Core.ClientDataTypeModelValidatorProviderExt.DateTimeErrorKey = "DateError";
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            LoggerFactory.ConfigureAndWatch(Server.MapPath("Log4net.config"));
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var redirectUrl = Response.RedirectLocation;
            if (Response.StatusCode == 302 && !string.IsNullOrWhiteSpace(redirectUrl))
            {
                Response.RedirectLocation = Regex.Replace(redirectUrl, @"(?'1'\?|&)ReturnUrl=(?'2'.+?)(?'3'&|$)", m =>
                {
                    return string.Format("{0}ReturnUrl={1}{2}", m.Groups["1"].Value, HttpUtility.UrlEncode(new Uri(Request.Url, HttpUtility.UrlDecode(m.Groups["2"].Value)).ToString()), m.Groups["3"].Value);
                }, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            }
        }
    }
}
