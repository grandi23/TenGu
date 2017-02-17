using System;
using System.Web;
using System.Web.Mvc;
using Tuhu.Component.Framework;
using ThBiz.Business.Monitor;

namespace Tuhu.YeWu.TenGu
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class HandleErrorByLoggingAttribute : HandleErrorAttribute, IExceptionFilter
    {
        #region Private Fields

        private static readonly ILog logger = LoggerFactory.GetLogger("ThBIZ");

        #endregion

        public override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled && filterContext.Exception != null)
            {
                logger.Log(Level.Critial, filterContext.Exception, "Unhandled exception occurred in ThBIZ system.");
            }

            base.OnException(filterContext);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ErrorMessageSettingHandle : HandleErrorAttribute, IExceptionFilter, IActionFilter
    {
        /// <summary>
        /// 使用记录ACTION执行成功的操作
        /// </summary>
        private bool _isUseExecuted = false;
        public bool IsUseExecuted
        {
            get { return _isUseExecuted; }
            set { _isUseExecuted = value; }
        }

        /// <summary>
        /// 主题
        /// </summary>
        private string _subjectType = "Power";
        public string SubjectType
        {
            get { return _subjectType; }
            set { _subjectType = value; }
        }

        /// <summary>
        /// 参数名称串
        /// </summary>
        public string Args { get; set; }

        /// <summary>
        /// 模块
        /// </summary>
        private ThBiz.Common.Common.MonitorModule _module = ThBiz.Common.Common.MonitorModule.Financial;
        public ThBiz.Common.Common.MonitorModule Module
        {
            get { return _module; }
            set { _module = value; }
        }

        private TheBiz.Common.Common.MonitorLevel _level = TheBiz.Common.Common.MonitorLevel.Info;

        /// <summary>
        /// 等级
        /// </summary>
        public TheBiz.Common.Common.MonitorLevel Level
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string OprationName { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception == null) return;

            //获取异常信息，入库保存
            var exception = filterContext.Exception.Message;
            var Url = HttpContext.Current.Request.RawUrl; //错误发生地址

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Redirect("/Error/ErrorPage?exception=" + exception);
            ExceptionMonitor.AddNewMonitor("系统", "Unhandled exception occurred in " + Url, exception,
                HttpContext.Current.User.Identity.Name, "Action错误");
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsUseExecuted)
                return;
            string userno = HttpContext.Current.User.Identity.Name;
            var request = HttpContext.Current.Request;
            var Url = request.RawUrl;
            string msg = "";
            try
            {
                string[] keys = new string[] { };
                if (request.HttpMethod.ToLower() == "get")
                    keys = request.QueryString.AllKeys;
                else
                    keys = request.Form.AllKeys;
                foreach (string arg in keys)
                    msg += arg + ":" + request[arg] + ";";
                msg = msg.TrimEnd(';');
                if (string.IsNullOrEmpty(msg))
                    msg = request.Url.ToString();
                string hostName = System.Net.Dns.GetHostName();
                ExceptionMonitor.AddNewMonitor(SubjectType, "Successing occurred in " + Url, "参数信息，" + msg + ",服务器名：" + hostName, userno, OprationName, Level, Module);
            }
            catch { }
        }

        /// <summary>
        /// 执行完成后
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            return;
        }
    }

}