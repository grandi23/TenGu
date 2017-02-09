using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ThBiz.Business;
using ThBiz.Business.CargoManagement;
using ThBiz.Business.EmployeeManagement;
using ThBiz.Business.Monitor;
using ThBiz.Business.OprLogManagement;
using ThBiz.Business.Purchase;
using ThBiz.Common.Configurations;
using ThBiz.Common.Entity;
using ThBiz.Common.Common;
using ThBiz.DataAccess.Entity;
using TheBiz.Common;
using TheBiz.Common.Common;
using Tuhu.Component.Common;
using Tuhu.Component.Framework;
using Tuhu.Component.Framework.Identity;
using Tuhu.YeWu.TenGu.Models;
using Font = iTextSharp.text.Font;

namespace Tuhu.YeWu.TenGu.Controllers
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class PlatformController : Controller
    {
        private readonly Lazy<PurchaseNewManager> _lazyPurchaseNewManager = new Lazy<PurchaseNewManager>();
        private PurchaseNewManager PurchaseNewManager => _lazyPurchaseNewManager.Value;

        #region 仓库区域配置
        /// <summary>
        /// 仓库区域配置
        /// 2017/02/09
        /// </summary>
        //[PowerManage]
        public ActionResult RegionWareHouseConfigIndex()
        {
            return View(PurchaseNewManager.SelectWareHouseAreaConfigList());
        }

        /// <summary>
        /// 添加仓库管理配置
        /// 2017/02/09
        /// </summary>
        public ActionResult AddRegionWareHouseConfig()
        {
            ViewBag.WareHouse = PurchaseNewManager.SelectWareHouseList(new PurchaseWareHouseAreaConfig() { Area = "N''"});
            ViewBag.Area =
                PurchaseNewManager.SelectAreaList()
                    .Where(areas => !string.IsNullOrWhiteSpace(areas.Area))
                    .Aggregate(string.Empty,
                        (current, areas) => !string.IsNullOrEmpty(current) ? current + "," + areas.Area : areas.Area);

            return View();
        }

        /// <summary>
        /// 添加仓库管理配置
        /// 2017/02/09
        /// </summary>
        public ActionResult InsertWareHouseAreaConfigurationList(List<AjaxWareArea> request)
        {
            JosnMessage requests = new JosnMessage();
            try
            {
                if (request.Count > 0 && request != null)
                {
                    List<PurchaseWareHouseAreaConfig> wareHouseAreaList = new List<PurchaseWareHouseAreaConfig>();
                    foreach (var item in request)
                    {
                        PurchaseWareHouseAreaConfig wareHouseArea = new PurchaseWareHouseAreaConfig
                        {
                            Area = item.NewArea,
                            WareHouseId = item.WarehouseId,
                            WareHouseName = item.WarehouseName,
                            Creator = User.Identity.Name,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };
                        wareHouseAreaList.Add(wareHouseArea);
                    }
                    var result = PurchaseNewManager.InsertWareHouseAreaConfiguration(wareHouseAreaList);

                    requests.Result = result;
                    if (!result)
                    {
                        requests.Message = "添加仓库配置信息错误";
                    }
                    requests.Message += "" + "添加仓库的数据为：" + JsonConvert.SerializeObject(wareHouseAreaList) + " ";

                    //PurchaseNewManager purchaseTaskLog = new PurchaseTaskLog
                    //{
                    //    Author = User.Identity.Name,
                    //    ObjectType = "AddWareHouse",
                    //    ObjectID = 0,
                    //    Operation = "添加",
                    //    ChangeDatetime = DateTime.Now,
                    //    Note = requests.Message
                    //};
                    //List<PurchaseTaskLog> purchaseTaskLogList = new List<PurchaseTaskLog>();
                    //purchaseTaskLogList.Add(purchaseTaskLog);

                    //requests.Result = PurchaseFacade.CreatePurchaseTaskLog(purchaseTaskLogList);
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    requests.Result = false;
                    requests.Message = "添加仓库配置信息错误，添加数据为空";
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                requests.Result = false;
                requests.Message = ex.ToString();
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 修改仓库管理配置
        /// 2017/02/09
        /// </summary>
        public ActionResult EditRegionWareHouseConfig(string area)
        {
            if (!string.IsNullOrWhiteSpace(area))
            {
                PurchaseWareHouseAreaConfig wareHouseArea = new PurchaseWareHouseAreaConfig()
                {
                    Area = area
                };
                List<string> wareHouseNameList = new List<string>();
                List<int> wareHouseIdList = new List<int>();
                List<PurchaseWareHouseAreaConfig> warehouseGroupList = PurchaseNewManager.SelectWareHouseAreaConfig(wareHouseArea);
                foreach (var warehouseGroups in warehouseGroupList)
                {
                    wareHouseNameList.Add(warehouseGroups.WareHouseName);
                    wareHouseIdList.Add(warehouseGroups.WareHouseId);
                }

                ViewBag.Area = warehouseGroupList.First().Area;
                ViewBag.WareHouseName = JsonConvert.SerializeObject(wareHouseNameList);
                ViewBag.WareHouseId = JsonConvert.SerializeObject(wareHouseIdList);
                ViewBag.WareHouse = PurchaseNewManager.SelectWareHouseList(wareHouseArea);
            }
            return View();
        }

        /// <summary>
        /// 更新修改仓库配置
        /// 2017/02/09
        /// </summary>
        public ActionResult UpdateWareHouseAreaConfiguration(List<AjaxWareArea> request)
        {
            JosnMessage requests = new JosnMessage();
            try
            {
                List<PurchaseWareHouseAreaConfig> addWarehouseList = new List<PurchaseWareHouseAreaConfig>();
                List<PurchaseWareHouseAreaConfig> removeWarehouseList = new List<PurchaseWareHouseAreaConfig>();
                var areaList = request.GroupBy(n => n.Type).OrderBy(n => n.Key);
                var odlArea = areaList.ToList()[0];
                var newArea = areaList.ToList()[1];
                bool changeArea = odlArea.First().OdlArea == newArea.First().NewArea;

                if (request.Count > 0 && request != null)
                {
                    var AjaxWareArea = request.GroupBy(n => new { n.WarehouseId, n.WarehouseName });
                    foreach (var list in AjaxWareArea)
                    {
                        PurchaseWareHouseAreaConfig wareHouseArea = new PurchaseWareHouseAreaConfig();
                        //这是判断是否有重复项，有重复项为未修改数据不做操作
                        if (list.Count() == 1)
                        {
                            if (list.First().Type == 2)
                            {
                                wareHouseArea.WareHouseId = list.First().WarehouseId;
                                wareHouseArea.WareHouseName = list.First().WarehouseName;
                                wareHouseArea.Area = list.First().NewArea;
                                wareHouseArea.Creator = User.Identity.Name;
                                wareHouseArea.CreateDate = DateTime.Now;
                                wareHouseArea.UpdateDate = DateTime.Now;
                                addWarehouseList.Add(wareHouseArea);
                            }
                            else
                            {
                                wareHouseArea.WareHouseId = list.First().WarehouseId;
                                wareHouseArea.WareHouseName = list.First().WarehouseName;
                                wareHouseArea.Area = list.First().OdlArea;
                                wareHouseArea.Updator = User.Identity.Name;
                                wareHouseArea.CreateDate = DateTime.Now;
                                wareHouseArea.UpdateDate = DateTime.Now;
                                removeWarehouseList.Add(wareHouseArea);
                            }
                        }
                    }
                }
                bool result = false;
                //新添加的
                if (addWarehouseList.Count > 0 && addWarehouseList != null)
                {
                    result = PurchaseNewManager.InsertWareHouseAreaConfiguration(addWarehouseList);
                    requests.Result = result;
                    if (!result)
                    {
                        requests.Message = "添加修改的仓库配置的部分仓库错误";
                        return Json(requests, JsonRequestBehavior.AllowGet);
                    }
                }
                //删除的
                if (removeWarehouseList.Count > 0 && removeWarehouseList != null)
                {
                    result = PurchaseNewManager.UpdateWareHouseAreaConfig(removeWarehouseList);
                    if (!result)
                        requests.Message = "删除修改的仓库配置的部分仓库错误";
                }
                if (!changeArea) //判断区域名是否修改
                {
                    AjaxWareArea wareArea = new AjaxWareArea
                    {
                        OdlArea = odlArea.First().OdlArea,
                        NewArea = newArea.First().NewArea,
                        Updator = User.Identity.Name
                    };
                    result = PurchaseNewManager.UpdateWareHouseArea(wareArea);
                    requests.Result = result;
                    if (!result)
                    {
                        requests.Message = "修改仓库的区域名错误";
                    }
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }
                requests.Result = result;
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                requests.Result = false;
                requests.Message = ex.ToString();
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 删除仓库管理配置
        /// 2017/02/09
        /// </summary>
        public ActionResult DeleteWareHouseAreaConfiguration(string area)
        {
            JosnMessage requests = new JosnMessage();
            try
            {
                if (string.IsNullOrWhiteSpace(area))
                {
                    requests.Result = false;
                    requests.Message = "传过来的数据area为空";
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    PurchaseWareHouseAreaConfig wareHouseArea = new PurchaseWareHouseAreaConfig()
                    {
                        Area = area,
                        IsDeleted = true,
                        Updator = User.Identity.Name,
                    };
                    var result = PurchaseNewManager.DeleteWareHouseAreaConfig(wareHouseArea);
                    requests.Result = result;
                    if (!result)
                    {
                        requests.Message = "传过来的数据area为空";
                    }
                    requests.Message = "" + "删除的仓库的数据为：" + JsonConvert.SerializeObject(wareHouseArea) + "";
                    //PurchaseTaskLog purchaseTaskLog = new PurchaseTaskLog
                    //{
                    //    Author = User.Identity.Name,
                    //    ObjectType = "AddWareHouse",
                    //    ObjectID = 0,
                    //    Operation = "删除",
                    //    ChangeDatetime = DateTime.Now,
                    //    Note = requests.Message
                    //};
                    //List<PurchaseTaskLog> purchaseTaskLogList = new List<PurchaseTaskLog>();
                    //purchaseTaskLogList.Add(purchaseTaskLog);
                    //requests.Result = PurchaseFacade.CreatePurchaseTaskLog(purchaseTaskLogList);
                    return Json(requests, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                requests.Result = false;
                requests.Message = ex.ToString();
                return Json(requests, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 采购任务分配配置
        /// <summary>
        /// 采购任务分配配置页
        /// 2017/02/09
        /// </summary>
        public ActionResult PurchaseTaskAssignIndex(PurchaseTaskRequest request)
        {
            var model = PurchaseNewManager.SelectBatchPurchaseAssigner(new BatchPurchaseAssigner() { AssignerName = request.TaskMaster });
            //只刷新列表
            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                return View("PurchaseTaskAssignList", model);
            }

            ViewBag.Employee = new EmployeeManager().GetEmployeesByDeptName("采购部");

            return View(model);
        }

        /// <summary>
        /// 添加仓库管理配置
        /// 2017/02/09
        /// </summary>
        public ActionResult AddPurchaseTaskAssign()
        {
            ViewBag.Employee = new EmployeeManager().GetEmployeesByDeptName("采购部");
            var list = PurchaseNewManager.SelectBatchPurchaseAssigner(new BatchPurchaseAssigner() { });
            ViewBag.HouseWare = PurchaseNewManager.SelectWareHouseAreaConfigList();
            string strAssignerName = string.Empty;
            foreach (BatchPurchaseAssigner bpa in list)
            {
                if (!string.IsNullOrWhiteSpace(bpa.AssignerName))
                {
                    if (strAssignerName == string.Empty)
                    {
                        strAssignerName = bpa.AssignerName;
                    }
                    else
                    {
                        strAssignerName += "," + bpa.AssignerName;
                    }
                }
            }
            ViewBag.strAssignerName = strAssignerName;
            return View();
        }
        /// <summary>
        /// 添加分配人仓库信息
        /// 2017/02/09
        /// </summary>
        public ActionResult InsertPurchaseTaskAssign(string assigner, string warehouseId, string warehouseName)
        {
            if (string.IsNullOrEmpty(assigner) || string.IsNullOrEmpty(warehouseId) || string.IsNullOrEmpty(warehouseName))
            {
                return Json("分配人和仓库不能为空！", JsonRequestBehavior.AllowGet);
            }
            if (PurchaseNewManager.IsAnyAssigner(new BatchPurchaseAssigner() { EmailAddress = assigner }))
            {
                return Json("已存在此分配人！", JsonRequestBehavior.AllowGet);
            }
            var newAssigner = new EmployeeManager().GetEmployeesByDeptName("采购部").FirstOrDefault(_ => _.EmailAddress == assigner);
            if (newAssigner == null)
            {
                return Json("未获取到分配人信息！", JsonRequestBehavior.AllowGet);

            }
            string[] ArraywarehouseId = warehouseId.Split(',');
            string[] ArraywarehouseName = warehouseName.Split(',');
            List<BatchPurchaseAssigner> batchPurchaseAssignerList = new List<BatchPurchaseAssigner>();
            for (int i = 0; i < ArraywarehouseId.Length; i++)
            {
                var bpa = new BatchPurchaseAssigner()
                {
                    AssignerName = newAssigner.EmployeeName,
                    AssignerId = newAssigner.PKID,
                    WarehouseId = Convert.ToInt32(ArraywarehouseId[i]),
                    WarehouseName = ArraywarehouseName[i],
                    EmailAddress = newAssigner.EmailAddress,
                    CreateDateTime = DateTime.Now,
                    CreateBy = User.Identity.Name,
                    UpdateBy = "",
                    UpdateDate = DateTime.Now,
                    Brand = "全部品牌"
                };
                batchPurchaseAssignerList.Add(bpa);
            }

            bool result = PurchaseNewManager.InsertBatchPurchaseAssigner(batchPurchaseAssignerList);
            if (!result)
            {
                return Json("添加任务分配信息失败！", JsonRequestBehavior.AllowGet);
            }
            return Json("添加任务分配信息成功！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改任务分配仓库页
        /// 2017/02/09
        /// </summary>
        public ActionResult EditPurchaseTaskAssignWareHouse(string assigner)
        {
            ViewBag.HouseWare = PurchaseNewManager.SelectWareHouseAreaConfigList();

            string wareHouseName = "";
            string wareHouseid = "";
            var result = PurchaseNewManager.SelectBatchPurchaseAssigner(new BatchPurchaseAssigner { AssignerName = assigner });

            foreach (var item in result)
            {
                if (wareHouseName == "")
                {
                    wareHouseName += item.WarehouseName;
                    wareHouseid += item.WarehouseId.ToString();
                }
                else
                {
                    wareHouseName += "," + item.WarehouseName;
                    wareHouseid += "," + item.WarehouseId;
                }
            }

            ViewBag.ExistWarehouseId = wareHouseid;
            ViewBag.ExistWarehouseName = wareHouseName;

            ViewBag.AssignerId = result.Count > 0 ? result.First().AssignerId : 0;
            ViewBag.AssignerName = result.Count > 0 ? result.First().AssignerName : "";
            ViewBag.EmailAddress = result.Count > 0 ? result.First().EmailAddress : "";

            return View();
        }
        /// <summary>
        /// 分配人修改仓库
        /// 2017/02/09
        /// </summary>
        public ActionResult UpdateBatchPurchaseAssignerWarehouse(AjaxAssignerWareHouse ajax)
        {
            List<string> addWarehouseIds = new List<string>();
            List<string> addWarehouseNames = new List<string>();
            List<string> removeWarehouseIds = new List<string>();
            List<string> removeWarehouseNames = new List<string>();
            if (ajax.NewWarehouseId.Length > 0)
            {
                string[] warehouseId = ajax.NewWarehouseId.Split(',');
                addWarehouseIds = new List<string>(warehouseId);
            }

            if (ajax.NewWarehouseName.Length > 0)
            {
                string[] warehouseName = ajax.NewWarehouseName.Split(',');
                addWarehouseNames = new List<string>(warehouseName);
            }

            if (ajax.OdlWareHouseId.Length > 0)
            {
                string[] wareHouseId = ajax.OdlWareHouseId.Split(',');
                removeWarehouseIds = new List<string>(wareHouseId);
            }
            else
            {
                removeWarehouseIds.Add(ajax.OdlWareHouseId);
            }
            if (ajax.OdlWarehouseName.Length > 0)
            {
                string[] warehouseName = ajax.OdlWarehouseName.Split(',');
                removeWarehouseNames = new List<string>(warehouseName);
            }
            else
            {
                removeWarehouseNames.Add(ajax.OdlWarehouseName);
            }
            var addWarehouseId = addWarehouseIds.Except(removeWarehouseIds).ToArray();
            var addWarehouseName = addWarehouseNames.Except(removeWarehouseNames).ToArray();
            List<string> removeWarehouseId = removeWarehouseIds.Except(addWarehouseIds).ToList();

            var houseWare = PurchaseNewManager.SelectWareHouseAreaConfigList();

            //新添加的
            if (addWarehouseId.Any())
            {

                List<BatchPurchaseAssigner> batchPurchaseAssignerList = new List<BatchPurchaseAssigner>();
                for (int i = 0; i < addWarehouseId.Length; i++)
                {
                    bool reuslt = PurchaseNewManager.IsAnyAssignerWarehouseName(new BatchPurchaseAssigner()
                    {
                        EmailAddress = ajax.EmailAddress,
                        WarehouseId = Int32.Parse(addWarehouseId[i])
                    });
                    if (reuslt)
                    {
                        PurchaseNewManager.UpdateBatchPurchaseAssignerByWarehouseNameId(new BatchPurchaseAssigner
                        {
                            AssignerId = ajax.AssignerId,
                            WarehouseId = Int32.Parse(addWarehouseId[i]),
                            WarehouseName = houseWare.FirstOrDefault(_ => _.WareHouseId ==
                                                                      Int32.Parse(addWarehouseId[i])).WareHouseName,
                        });
                        continue;
                    }
                    var bpa = new BatchPurchaseAssigner
                    {
                        AssignerName = ajax.AssignerName,
                        AssignerId = ajax.AssignerId,
                        EmailAddress = ajax.EmailAddress,
                        WarehouseId = Int32.Parse(addWarehouseId[i]),
                        WarehouseName = houseWare.FirstOrDefault(_ => _.WareHouseId ==
                                                                      Int32.Parse(addWarehouseId[i])).WareHouseName,
                        Brand = "全部品牌",
                        CreateDateTime = DateTime.Now,
                        CreateBy = User.Identity.Name,
                        UpdateBy = "",
                        UpdateDate = DateTime.Now
                    };
                    batchPurchaseAssignerList.Add(bpa);
                }
                PurchaseNewManager.InsertBatchPurchaseAssigner(batchPurchaseAssignerList);
            }

            //删除的
            if (removeWarehouseId.Count > 0)
            {
                foreach (var item in removeWarehouseId)
                {
                    PurchaseNewManager.CloseBatchPurchaseAssignerByAssigner(new BatchPurchaseAssigner()
                    {
                        AssignerId = ajax.AssignerId,
                        WarehouseId = Int32.Parse(item)
                    });
                }
            }

            return Json("保存成功", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改任务分配品牌页
        /// 2017/02/09
        /// </summary>
        public ActionResult EditPurchaseTaskAssignBrand(string assigner)
        {
            var warehouse = PurchaseNewManager.SelectBatchPurchaseAssigner(new BatchPurchaseAssigner()
            {
                AssignerName = assigner
            });
            ViewBag.WareHouse = warehouse;
            List<ProductBrand> brand = new List<ProductBrand>();
            ProductBrand productBrand = new ProductBrand
            {
                CP_Brand = "全部品牌"
            };
            brand.Add(productBrand);
            brand.AddRange(new CargoManager().GetProductBrandList());
            ViewBag.Brand = brand;//品牌
            return View();
        }
        /// <summary>
        /// 删除分配人信息
        /// 2017/02/09
        /// </summary>
        public ActionResult CloseBatchPurchaseAssignerByAssignerByAssignerId(int assignerId)
        {
            var result = PurchaseNewManager.CloseBatchPurchaseAssignerByAssignerByAssignerId(new BatchPurchaseAssigner()
            {
                AssignerId = assignerId
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 分配人修改品牌
        /// 2017/02/09
        /// </summary>

        public ActionResult UpdateWareHouseBrandConfig(string wareHouseBrand)
        {
            var listJson = JsonConvert.DeserializeObject<List<BatchPurchaseAssigner>>(wareHouseBrand);
            foreach (var item in listJson)
            {
                if (item.Brand.Contains("全部品牌"))
                {
                    item.Brand = "全部品牌";
                }
            }
            var result = PurchaseNewManager.UpdateBatchPurchaseAssignerByAssigner(listJson);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}