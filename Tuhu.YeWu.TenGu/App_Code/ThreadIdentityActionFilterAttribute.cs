using System.Web.Mvc;

using Tuhu.Component.Framework.Identity;

namespace Tuhu.YeWu.TenGu
{
    public class ThreadIdentityActionFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            if (user != null && user.Identity != null && !string.IsNullOrWhiteSpace(user.Identity.Name))
            {
                ThreadIdentity.Identifier = new UserIdentifier(user.Identity.Name, filterContext.HttpContext.Request.UserHostAddress, string.Empty, "采购系统");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}