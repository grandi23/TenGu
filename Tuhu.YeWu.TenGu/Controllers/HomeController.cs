using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThBiz.Business.CargoManagement;
using ThBiz.DataAccess.Entity;
using Tuhu.Component.Framework.Identity;
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
        /// <summary>
        /// 选择产品页面
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchSelectProduct()
        {
            return View();
        }
        /// <summary>
        /// 关键字查询产品信息
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult SelectProductList(string keyWord)
        {
            var list = new List<ProductMaintain>();

            if (!string.IsNullOrEmpty(keyWord))
            {
                list = new CargoManager().SelectProductListByKeyWord(keyWord);
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 导入Excel页面
        /// RenYongQiang 2017/02/16
        /// 模版路径
        /// </summary>
        /// <returns></returns>
        public ActionResult ImportExcel(string temp = "")
        {
            ViewBag.Template = temp;
            return View();
        }
        /// <summary>
        /// 记录原因
        /// RenYongQiang 2017/02/20
        /// </summary>
        public ActionResult InputReson()
        {
            return View();
        }
    }
}