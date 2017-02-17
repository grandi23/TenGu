using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ThBiz.Business.Power;
using ThBiz.DataAccess.Entity;

namespace Tuhu.YeWu.TenGu
{
    /// <summary>
    ///　权限拦截
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PowerManageAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// ActionKey
        /// </summary>
        public string ActionKey{get;set;}

        /// <summary>
        /// 是否是视图的ACTION
        /// </summary>
        public bool IsViewAction { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
                filterContext.HttpContext.Response.Redirect("/Home/ErrorPage?error=权限不足，请联系部门管理员配置访问权限");

            var listPower = new List<ActionPower>();//(List<ActionPower>)(filterContext.HttpContext.Session["CaPowerList"]);
            //if (listPower == null || listPower.Count == 0)
            //{
                var userno = filterContext.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(userno))
                {
                    byte issupper = 0;
                    if (System.Configuration.ConfigurationManager.AppSettings["SupperUsers"].Contains(userno))
                    {
                        issupper = 1;
                    }
                    //filterContext.HttpContext.Session["CaPowerList"] = new PowerManage().GetBusPower(userno, issupper);
                    listPower = new PowerManage().GetBusPower(userno, issupper, filterContext); //(List<ActionPower>)(filterContext.HttpContext.Session["CaPowerList"]);
                }
                else
                {
                    filterContext.HttpContext.Response.Redirect("/Home/ErrorPage?error=请先登录");
                    filterContext.HttpContext.Response.End();
                    filterContext.Result = new EmptyResult();
                }
            //}
            string userNo = filterContext.HttpContext.User.Identity.Name;
            string controllerName = filterContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.RouteData.Values["action"].ToString();
            string msg = "";
            string query = filterContext.HttpContext.Request.Url.Query;
            if (string.IsNullOrEmpty(ActionKey))
                IsViewAction = true;
            bool bol = PowerHandle.PowerValidServer(listPower, userNo, controllerName, actionName, query, ActionKey, IsViewAction, out msg);
            string type = msg.Split('|')[1];
            msg = msg.Split('|')[0];
            if (!bol)
            {
                if (type.ToLower() == "function")
                    filterContext.HttpContext.Response.Write("{Status:false,Msg:\"" + msg + "\",IsPower:1}");
                else
                    filterContext.HttpContext.Response.Redirect("/Home/ErrorPage?error=" + msg);

                filterContext.HttpContext.Response.End();
                filterContext.Result = new EmptyResult();
            }
            base.OnActionExecuting(filterContext);
        }
    }
}