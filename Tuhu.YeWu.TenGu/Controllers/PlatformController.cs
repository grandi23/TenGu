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
using NPOI.OpenXmlFormats.Dml;
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
using Tuhu.Component.Common.Models;
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

        #region 工厂专供产品配置
        /// <summary>
        /// 工厂专供产品配置
        /// RenYongQiang 2017/02/15
        /// </summary>
        //[PowerManage]
        public ActionResult FactoryForProductIndex(TaskPoolRequest request)
        {
            var result = new PurchaseNewManager().SelectFactoryForProductList(request);
            var dataList = result.ReturnValue;
            var totalRecord = result.OutValue;
            var pager = new PagerModel(request.PageNumber, request.PageSize)
            {
                TotalItem = totalRecord
            };
            if (!string.IsNullOrEmpty(request.Type))
            {
                return View("FactoryForProductList", new ListModel<FactoryForProduct>(pager, dataList));
            }

            ViewBag.Brand = new CargoManager().GetProductBrandList();//品牌

            return View(new ListModel<FactoryForProduct>(pager, dataList));
        }
        /// <summary>
        /// 删除工厂专供产品
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult DeleteFactoryProduct(int fId)
        {
            return Json(PurchaseNewManager.DeleteFactoryForProductById(fId), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 新增工厂专供产品
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult InsertFactoryProduct(string products)
        {
            var list = JsonConvert.DeserializeObject<List<ProductMaintain>>(products);
            if (list.Any())
            {
                foreach (var product in list)
                {
                    PurchaseNewManager.InsertFactoryForProduct(product);
                }
                return Json("新增成功！", JsonRequestBehavior.AllowGet);
            }
            return Json("添加失败，未选择任何产品！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 导入工厂专供产品
        /// RenYongQiang 2017/02/16
        /// </summary>
        [HttpPost]
        public ActionResult ImportFactoryForProduct(HttpPostedFileBase efile)
        {
            var errors = new List<string>();
            try
            {
                if (string.IsNullOrWhiteSpace(efile?.FileName))
                {
                    errors.Add("失败,未获取到文件！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                var fileName = efile.FileName;

                if (!fileName.EndsWith(".xls") && !fileName.EndsWith(".xlsx"))
                {
                    errors.Add("失败,仅能上传.xls或.xlsx文件的格式！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }

                if (efile.ContentLength / (1024 * 1024) > 10) //大于10M
                {
                    errors.Add("失败,文件容量大于10M！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }

                var strem = efile.InputStream;

                var ds = TuhuUtil.ExcelToDataTableByStream(true, strem, fileName);

                if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count == 0)
                {
                    errors.Add("失败,文件内没有内容！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                var dt = ds.Tables[0];
                //验证列是否存在
                var colums = "产品名称,产品编号,品牌";
                errors.AddRange(from msg in colums.Split(',') where dt.Columns[msg] == null select $"该导入的文件中没有\"{msg}\"这一列！");
                if (errors.Count > 0)
                {
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                //过滤空白行
                dt = dt.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("产品名称"))).CopyToDataTable();

                var itemList = new List<ProductMaintain>();

                //导入数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    var item = new ProductMaintain();
                    //产品名称
                    if (string.IsNullOrEmpty(row["产品名称"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品名称！");
                        continue;
                    }
                    item.ProductName = row["产品名称"].ToString();
                    //产品编号
                    if (string.IsNullOrEmpty(row["产品编号"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品编号！");
                        continue;
                    }
                    item.ProductId = row["产品编号"].ToString();
                    //品牌
                    if (string.IsNullOrEmpty(row["品牌"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写品牌！");
                        continue;
                    }
                    item.Brand = row["品牌"].ToString();

                    itemList.Add(item);
                }

                if (itemList.Count > 0)
                {
                    PurchaseNewManager.BatchFactoryForProduct(itemList);
                    errors.Add("本次共成功导入" + itemList.Count + "行!");
                }
                else
                {
                    errors.Add("导入失败,无有效数据！");
                }

                return Json(errors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("工厂专供配置", "导入", ex.ToString(), ThreadIdentity.Operator.Name, "工厂专供配置导入",
                    MonitorLevel.Error, MonitorModule.Purchase);
                errors.Add("导入错误，操作无法进行！");
                return Json(errors, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 运营活动配置
        /// <summary>
        /// 运营活动配置
        /// RenYongQiang 2017/02/16
        /// </summary>
        //[PowerManage]
        public ActionResult OperateActiveIndex(OperateActiveRequest request)
        {
            var result = new PurchaseNewManager().SelectOperateActiveList(request);
            var dataList = result.ReturnValue;
            var totalRecord = result.OutValue;
            var pager = new PagerModel(request.PageNumber, request.PageSize)
            {
                TotalItem = totalRecord
            };
            if (!string.IsNullOrEmpty(request.Type))
            {
                return View("OperateActiveList", new ListModel<OperateActive>(pager, dataList));
            }

            ViewBag.CreateBy = new EmployeeManager().GetEmployeesByDeptName("采购部");

            return View(new ListModel<OperateActive>(pager, dataList));
        }
        /// <summary>
        /// 创建活动页面
        /// RenYongQiang 2017/02/16
        /// </summary>
        public ActionResult AddOperateActive()
        {
            return View();
        }
        /// <summary>
        /// 运营活动选择产品页面
        /// RenYongQiang 2017/02/17
        /// </summary>
        public ActionResult SelectOperateActiveProduct()
        {
            return View();
        }
        /// <summary>
        /// 获取运营活动产品备选数据
        /// RenYongQiang 2017/02/17
        /// </summary>
        public ActionResult SelectActiveProductList(string keyWord)
        {
            var list = new List<ProductShortInfo>();

            if (string.IsNullOrEmpty(keyWord)) return Json(list, JsonRequestBehavior.AllowGet);

            var products = new CargoManager().SelectProductListByKeyWord(keyWord);
            if (!products.Any()) return Json(list, JsonRequestBehavior.AllowGet);

            var kls = PurchaseNewManager.SelectAveragePurchasePrice(products);

            list.AddRange(from pr in products
                let ks = kls.FirstOrDefault(x => x.PID == pr.ProductId)
                select new ProductShortInfo()
                {
                    ProductName = pr.ProductName,
                    PID = pr.ProductId,
                    DailyPrice = ks?.DailyPrice ?? 0,
                    PurchasePrice = ks?.PurchasePrice ?? 0
                });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 导入运营活动产品
        /// RenYongQiang 2017/02/17
        /// </summary>

        public ActionResult ImportActiveProductList(HttpPostedFileBase efile)
        {
            var errors = new List<string>();
            try
            {
                if (string.IsNullOrWhiteSpace(efile?.FileName))
                {
                    errors.Add("失败,未获取到文件！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                var fileName = efile.FileName;

                if (!fileName.EndsWith(".xls") && !fileName.EndsWith(".xlsx"))
                {
                    errors.Add("失败,仅能上传.xls或.xlsx文件的格式！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }

                if (efile.ContentLength / (1024 * 1024) > 10) //大于10M
                {
                    errors.Add("失败,文件容量大于10M！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }

                var strem = efile.InputStream;

                var ds = TuhuUtil.ExcelToDataTableByStream(true, strem, fileName);

                if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count == 0)
                {
                    errors.Add("失败,文件内没有内容！");
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                var dt = ds.Tables[0];
                //验证列是否存在
                var colums = "产品名称,产品编号,活动价,预计销量,活动底价";
                errors.AddRange(from msg in colums.Split(',') where dt.Columns[msg] == null select $"该导入的文件中没有\"{msg}\"这一列！");
                if (errors.Count > 0)
                {
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                //过滤空白行
                dt = dt.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("产品名称"))).CopyToDataTable();

                var itemList = new List<OperateActiveProduct>();
                var productList = new List<ProductMaintain>();

                //导入数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    var item = new OperateActiveProduct();
                    var product = new ProductMaintain();
                    //产品名称
                    if (string.IsNullOrEmpty(row["产品名称"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品名称！");
                        continue;
                    }
                    item.ProductName = row["产品名称"].ToString();
                    //产品编号
                    if (string.IsNullOrEmpty(row["产品编号"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品编号！");
                        continue;
                    }
                    item.PID = row["产品编号"].ToString();
                    product.ProductId = item.PID;
                    //活动价
                    if (string.IsNullOrEmpty(row["活动价"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写活动价！");
                        continue;
                    }
                    item.ActivePrice = Convert.ToDecimal(row["活动价"]);
                    if (item.ActivePrice <= 0)
                    {
                        errors.Add($"第{i + 1}行导入失败,活动价小于0！");
                        continue;
                    }
                    //预计销量
                    if (string.IsNullOrEmpty(row["预计销量"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写预计销量！");
                        continue;
                    }
                    item.ExpecteSale = Convert.ToInt32(row["预计销量"]);
                    if (item.ExpecteSale <= 0)
                    {
                        errors.Add($"第{i + 1}行导入失败,预计销量小于0！");
                        continue;
                    }
                    //活动底价
                    if (string.IsNullOrEmpty(row["活动底价"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写活动底价！");
                        continue;
                    }
                    item.FloorPrice = Convert.ToDecimal(row["活动底价"]);
                    if (item.FloorPrice <= 0)
                    {
                        errors.Add($"第{i + 1}行导入失败,预计销量小于0！");
                        continue;
                    }
                    item.ExpecteSaleAmount = item.ExpecteSale*item.ActivePrice;
                    itemList.Add(item);
                    productList.Add(product);
                }

                if (itemList.Count > 0)
                {
                    var list = PurchaseNewManager.SelectAveragePurchasePrice(productList);
                    foreach (var ap in itemList)
                    {
                        var ks = list.FirstOrDefault(x => x.PID == ap.PID);
                        if (ks == null) continue;

                        ap.PurchasePrice = ks.PurchasePrice;
                        ap.DailyPrice = ks.DailyPrice;
                        ap.MinMargin = ap.FloorPrice - ap.PurchasePrice;
                    }
                    return Json(itemList, JsonRequestBehavior.AllowGet);
                }

                errors.Add("导入失败,无有效数据！");

                return Json(errors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("运营活动产品", "导入", ex.ToString(), ThreadIdentity.Operator.Name, "运营活动产品导入",
                    MonitorLevel.Error, MonitorModule.Purchase);
                errors.Add("导入错误，操作无法进行！");
                return Json(errors, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// 新增运营活动信息
        /// RenYongQiang 2017/02/17
        /// </summary>
        public ActionResult InsertOperateActiveInfo(string name, string channel, string starDate, string endDate,
            string point, string coupon, string link, string products)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(starDate) ||
                string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(point) || string.IsNullOrEmpty(coupon) ||
                string.IsNullOrEmpty(link) || string.IsNullOrEmpty(products))
            {
                return Json("活动信息填写不完整，创建活动失败！", JsonRequestBehavior.AllowGet);
            }
            var activeProduct = JsonConvert.DeserializeObject<List<OperateActiveProduct>>(products);
            if (activeProduct.Any(x => x.ActivePrice <= 0 || x.ExpecteSale <= 0 || x.FloorPrice <= 0))
            {
                return Json("活动产品填写错误，创建活动失败！", JsonRequestBehavior.AllowGet);
            }
            var operate = new OperateActive()
            {
                ActiveName = name,
                ActiveChannel = channel,
                ActiveStarDate = Convert.ToDateTime(starDate),
                ActiveEndDate = Convert.ToDateTime(endDate),
                InterestPoint = point,
                Coupon = coupon,
                ActiveLink = link,
                ActiveState = "待运营审核",
                ActiveProductList = activeProduct
            };

            PurchaseNewManager.InsertOperateActive(operate);

            return Json("创建活动成功！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 运营活动详情
        /// RenYongQiang 2017/02/20
        /// </summary>
        public ActionResult OperateActiveDetile(int aId)
        {
            var model = PurchaseNewManager.GetOperateActiveById(aId);
            if (model != null)
            {
                model.ActiveProductList = PurchaseNewManager.SelectOperateActiveProductByActiveId(aId);
                model.AuditMonitorList = PurchaseNewManager.GetPurchaseMonitorLogList(new PurchaseMonitorLog()
                {
                    ObjectID = model.PKID,
                    ObjectType = PurchaseMonitorTypeEnum.OperateActive.ToString()
                });
            }
            return View(model);
        }
        /// <summary>
        /// 审核运营活动
        /// RenYongQiang 2017/02/20
        /// </summary>
        public ActionResult AuditOperateActive(int aId, int step, string reson = "")
        {
            var active = PurchaseNewManager.GetOperateActiveById(aId);

            var log = new PurchaseMonitorLog
            {
                ObjectType = PurchaseMonitorTypeEnum.OperateActive.ToString(),
                ObjectID = aId,
                AfterValue = string.Empty,
                BeforeValue = string.Empty,
                CreateUser = User.Identity.Name,
                IPAddress = ClientIp
            };
            var result = false;
            switch (step)
            {
                case 1://运营，审核通过
                    active.ActiveState = "待采购审核";

                    log.Operation = "运营审核";
                    break;
                case 2://运营，驳回活动
                    active.ActiveState = active.OperateReject == 4 ? "已取消" : "运营驳回";//驳回次数限制
                    active.OperateReject += 1;

                    log.Operation = "运营驳回";
                    log.AfterValue = reson;
                    break;
                case 3://采购，审核通过
                    active.ActiveState = "已通过";

                    log.Operation = "采购审核";
                    break;
                case 4://采购，驳回活动
                    active.ActiveState = active.PurchaseReject == 4 ? "已取消" : "采购驳回";//驳回次数限制
                    active.OperateReject += 1;

                    log.Operation = "采购驳回";
                    log.AfterValue = reson;
                    break;
                case 5://撤销申请
                    active.ActiveState = "已取消";

                    log.Operation = "撤销申请";
                    break;
                default:
                    active.ActiveState = string.Empty;
                    break;
            }
            if (!string.IsNullOrEmpty(active.ActiveState))
            {
                result = PurchaseNewManager.AuditOperateActive(active);
                PurchaseNewManager.InsertPurchaseMonitorLog(log);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改运营活动信息
        /// RenYongQiang 2017/02/20
        /// </summary>
        public ActionResult EditOperateActive(int aId)
        {
            var model = PurchaseNewManager.GetOperateActiveById(aId);
            if (model != null)
            {
                model.ActiveProductList = PurchaseNewManager.SelectOperateActiveProductByActiveId(aId);
            }
            return View(model);
        }
        /// <summary>
        /// 修改运营活动操作
        /// RenYongQiang 2017/02/20
        /// </summary>
        public ActionResult EditOperateActiveInfo(int aId, string name, string channel, string starDate, string endDate,
            string point, string coupon, string link, string products)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(starDate) ||
                string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(point) || string.IsNullOrEmpty(coupon) ||
                string.IsNullOrEmpty(link) || string.IsNullOrEmpty(products) || aId <= 0)
            {
                return Json("活动信息填写不完整，修改活动失败！", JsonRequestBehavior.AllowGet);
            }
            var active = PurchaseNewManager.GetOperateActiveById(aId);

            var activeProduct = JsonConvert.DeserializeObject<List<OperateActiveProduct>>(products);
            if (activeProduct.Any(x => x.ActivePrice <= 0 || x.ExpecteSale <= 0 || x.FloorPrice <= 0))
            {
                return Json("活动产品填写错误，修改活动失败！", JsonRequestBehavior.AllowGet);
            }

            active.ActiveName = name;
            active.ActiveChannel = channel;
            active.ActiveStarDate = Convert.ToDateTime(starDate);
            active.ActiveEndDate = Convert.ToDateTime(endDate);
            active.InterestPoint = point;
            active.Coupon = coupon;
            active.ActiveLink = link;
            active.ActiveState = active.ActiveState == "运营驳回" ? "待运营审核" : "待采购审核";
            active.ActiveProductList = activeProduct;

            PurchaseNewManager.UpdateOperateActive(active);

            return Json("修改活动成功！", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取IP地址
        /// 2017/01/24
        /// </summary>
        private string ClientIp
        {
            get
            {
                var userHostAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Split(',')[0].Trim() ?? Request.ServerVariables["REMOTE_ADDR"];

                if (string.IsNullOrEmpty(userHostAddress))
                {
                    userHostAddress = Request.UserHostAddress;
                }
                else if (Regex.IsMatch(userHostAddress,
                    @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"))
                {
                    return userHostAddress;
                }
                return ((!string.IsNullOrEmpty(userHostAddress) &&
                         Regex.IsMatch(userHostAddress,
                             @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"))
                    ? userHostAddress
                    : "127.0.0.1");
            }
        }
        #endregion
    }
}