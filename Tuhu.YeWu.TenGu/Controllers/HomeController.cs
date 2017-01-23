using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tuhu.YeWu.TenGu.Models;

namespace Tuhu.YeWu.TenGu.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 初始页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Session["MenuList"] = new LayoutMenu().MenuList;
            return View();
        }

        public ActionResult Show()
        {
            return View();
        }
    }
}