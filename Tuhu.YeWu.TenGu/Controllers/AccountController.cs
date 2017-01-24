using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using ThBiz.Business;
using ThBiz.Business.EmployeeManagement;
using ThBiz.Business.OprLogManagement;
using ThBiz.Business.ProfileManagement;
using ThBiz.Common.Configurations;
using ThBiz.Common.ValidatedCode;
using Tuhu.Component.Framework;
using Tuhu.Component.Framework.Identity;
using Tuhu.YeWu.TenGu.Models;

namespace Tuhu.YeWu.TenGu.Controllers
{
    public class AccountController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        private const string DefaultPassword = "1234ABCD";
        public const string ShopKey = "Shops";

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }

            base.Initialize(requestContext);
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string ReturnUrl)
        {
            var dc = new TuhuBiz();
            if (!string.IsNullOrEmpty(model.UserName))
                model.UserName = model.UserName.Trim();
            if (!string.IsNullOrEmpty(model.Password))
                model.Password = model.Password.Trim();

            if (ModelState.IsValid)
            {
                try
                {
                    if (HttpContext.Session["ValidatedCode"] == null)
                    {
                        throw new AccountingException(0, "验证码过期！");
                    }

                    if (Request.Form["ValidatedCode"] != HttpContext.Session["ValidatedCode"].ToString())
                    {
                        throw new AccountingException(0, "验证码不正确，请重新输入！");
                    }

                    new EmployeeManager().OnDuty(model.UserName, model.Password);

                    var loginLog = string.Format("{0}于【{1}】成功登陆途虎业务管理系统，登陆IP为：{2}", model.UserName, DateTime.Now, Request.ServerVariables["REMOTE_ADDR"]);
                    SaveOprLog("UserLogin", 0, typeof(Account), "途虎业务系统登陆", loginLog, null);
                    FormsService.SignIn(model.UserName, model.RememberMe);

                    HrEmployee ep = dc.HrEmployee.SingleOrDefault(o => o.EmailAddress.Equals(model.UserName));
                    if (ep != null)
                    {
                        UserConfig uc = dc.UserConfig.SingleOrDefault(o => o.UserID.Equals(ep.PKID));
                        if (uc != null)
                        {
                            uc.ShowMyTask = true;
                            uc.ShowOtherTask = true;
                            dc.SaveChanges();
                        }
                    }

                    //获得当前用户仓库保存到Session
                    //var shops = new EmployeeManager().SelectShopsEmployeeByEmailAddress(model.UserName);
                    //Session[ShopKey] = shops;

                    //获取当前用户权限
                    try//禁止注释掉 否则会因缓存原因，权限得不到及时更新
                    {
                        //var cache = CacheFactory.Create(CacheType.CouchBase);
                        //string key = EnOrDeHelper.GetMd5(ThreadIdentity.Operator.Name + "power!@#$", Encoding.UTF8);
                        //cache.Remove(key);//登录时，先清理缓存。
                        string key = EnOrDeHelper.GetMd5(ThreadIdentity.Operator.Name + "power!@#$", Encoding.UTF8);
                        HttpSession httpSession = new HttpSession();
                        httpSession.Remove(key);
                    }
                    catch
                    {
                        //屏蔽错误 
                    }

                    var isSupper = Convert.ToByte(System.Configuration.ConfigurationManager.AppSettings["SupperUsers"].ToString().Contains(model.UserName) ? 1 : 0);
                    var caPowerList = new ThBiz.Business.Power.PowerManage().GetBusPower(model.UserName, isSupper);
                    Session["CaPowerList"] = caPowerList;

                    if (string.Equals(model.Password.Trim(), DefaultPassword, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return RedirectToAction("ChangePassword", "Account");
                    }

                    var password = UserObjectManager.HashPassword(model.Password);
                    var timeSpan = new ProfileManager().SelectUpdateTimeSpan(model.UserName, password);

                    if (timeSpan >= 30)
                    {
                        return RedirectToAction("ChangePassword", "Account");
                    }

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (TuhuBizException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    SaveOprLog("UserLogin", 0, typeof(Account), "途虎业务系统登陆", "Singed_In_failed,Tried_UserName:" + model.UserName + "_Pwd:_" + model.Password, null);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            if (User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
            {
                new EmployeeManager().OffDuty(User.Identity.Name);
            }

            var oprLog = new ThBiz.DataAccess.Entity.OprLog();
            oprLog.Author = ThreadIdentity.Operator.Name;
            oprLog.IPAddress = ThreadIdentity.Operator.IPAddress;
            oprLog.ObjectType = "UserLogin";
            oprLog.ChangeDatetime = DateTime.Now;
            oprLog.BeforeValue = "Sign In";
            oprLog.AfterValue = string.Format("{0}于【{1}】退出途虎业务", ThreadIdentity.Operator.Name, DateTime.Now);
            new OprLogManager().AddOprLogAsync(oprLog);

            //退出时，将缓存数据重置为空
            Session["CaPowerList"] = null;
            //var cache = CacheFactory.Create(CacheType.CouchBase);
            //string key = EnOrDeHelper.GetMd5(ThreadIdentity.Operator.Name + "power!@#$", Encoding.UTF8);
            //cache.Remove(key);
            try//禁止注释掉 否则会因缓存原因，权限得不到及时更新
            {
                //var cache = CacheFactory.Create(CacheType.CouchBase);
                //string key = EnOrDeHelper.GetMd5(ThreadIdentity.Operator.Name + "power!@#$", Encoding.UTF8);
                //cache.Remove(key);//登录时，先清理缓存。
                string key = EnOrDeHelper.GetMd5(ThreadIdentity.Operator.Name + "power!@#$", Encoding.UTF8);
                HttpSession httpSession = new HttpSession();
                httpSession.Remove(key);
            }
            catch
            {
                //屏蔽错误 
            }
            FormsService.SignOut();
            return RedirectPermanent(YewuDomainConfig.YewuSite + "/Account/LogOff");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = 8;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.Equals(model.OldPassword, model.NewPassword, StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("", "新密码和旧密码不能一样");
                    ViewBag.PasswordLength = 8;
                    return View(model);
                }
                try
                {
                    var profileManager = new ProfileManager();
                    profileManager.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);

                    var pwd = UserObjectManager.HashPassword(model.NewPassword);

                    profileManager.AddChangePasswordHistory(User.Identity.Name, pwd);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (TuhuBizException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.PasswordLength = 8;
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        public JsonResult GetUserLoginRecord(string author, DateTime time)
        {
            string json = "";
            try
            {
                json = new OprLogManager().SelectUserLoginLogs(author, time);
            }
            catch (Exception ex)
            {
                json = ex.Message;
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidatedCodeImg()
        {
            var ms = new System.IO.MemoryStream();

            var rand1 = new Random().Next(1, 10);
            var rand2 = new Random().Next(1, 9);

            Session["ValidatedCode"] = rand1 * rand2;
            var code = rand1.ToString() + "×" + rand2.ToString() + "=?";
            var image = new GenerateValidatedCode(8, 20, 3, false, true, 5, 14).CreateImageCode(code);
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            var bytes = ms.ToArray();

            return File(bytes, @"image/jpeg");
        }


        //public string IsWeakPassword(string password)
        //{
        //    var result = new AccountManager().IsWeakPassword(password);
        //    return result?"YES":"NO";
        //}

        private void SaveOprLog(string objType, int objId, Type otype, string operation, object beforeValue,
           object afterValue = null)
        {
            var rol = new ThBiz.DataAccess.Entity.OprLog { Author = User.Identity.Name };
            var dcs = new DataContractSerializer(otype);
            if (beforeValue != null)
            {
                var builder = new StringBuilder();
                var type = beforeValue.GetType();
                if (type == typeof(string))
                {
                    builder.Append(beforeValue);
                }
                else
                {
                    foreach (var property in beforeValue.GetType().GetProperties())
                    {
                        object value = property.GetValue(beforeValue, null);

                        builder.Append(property.Name)
                            .Append(" = ")
                            .Append((value ?? "null"))
                            .AppendLine();
                    }
                }

                rol.BeforeValue = builder.ToString();
            }
            if (afterValue != null)
            {
                var builder = new StringBuilder();
                var type = afterValue.GetType();
                if (type == typeof(string))
                {
                    builder.Append(afterValue);
                }
                else
                {
                    foreach (var property in afterValue.GetType().GetProperties())
                    {
                        object value = property.GetValue(afterValue, null);

                        builder.Append(property.Name)
                            .Append(" = ")
                            .Append((value ?? "null"))
                            .AppendLine();
                    }
                }

                rol.AfterValue = builder.ToString();
            }

            rol.ChangeDatetime = DateTime.Now;
            rol.ObjectID = objId;
            rol.ObjectType = objType;
            rol.IPAddress = Request.ServerVariables["REMOTE_ADDR"];
            rol.Operation = operation;
            new OprLogManager().AddOprLog(rol);
        }
    }
}