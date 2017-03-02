using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using ThBiz.Business.CargoManagement;
using ThBiz.Business.EmployeeManagement;
using ThBiz.Business.Monitor;
using ThBiz.Business.Purchase;
using ThBiz.Common.Common;
using ThBiz.Common.Entity;
using ThBiz.DataAccess.Entity;
using TheBiz.Common;
using TheBiz.Common.Common;
using Tuhu.Component.Common.Models;
using Tuhu.Component.Framework.Identity;

namespace Tuhu.YeWu.TenGu.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly Lazy<PurchaseNewManager> _lazyPurchaseNewManager = new Lazy<PurchaseNewManager>();
        private PurchaseNewManager PurchaseNewManager => _lazyPurchaseNewManager.Value;
        #region 采购任务池
        /// <summary>
        /// 采购任务池主页
        /// RenYongQiang 2017/02/10
        /// </summary>
        //[PowerManage]
        public ActionResult TaskPoolIndex(TaskPoolRequest request)
        {
            var result = new PurchaseNewManager().SelectPurchaseTaskPoolList(request);
            var dataList = result.ReturnValue;
            var totalRecord = result.OutValue;
            var pager = new PagerModel(request.PageNumber, request.PageSize)
            {
                TotalItem = totalRecord
            };
            if (!string.IsNullOrEmpty(request.Type))
            {
                return View("TaskPoolList", new ListModel<PurchaseTaskInfo>(pager, dataList));
            }
            ViewBag.CreateBy = new EmployeeManager().GetEmployeesByDeptName("采购部");
            ViewBag.ProductBrandList = new CargoManager().GetProductBrandList();
            ViewBag.TaskState = (from int s in Enum.GetValues(typeof (PurchaseTaskStateEnum))
                select new ListItem
                {
                    Value = s.ToString(),
                    Text = Enum.GetName(typeof (PurchaseTaskStateEnum), s)
                }).ToList();
            ViewBag.Area = PurchaseNewManager.SelectAreaList();
            ViewBag.HouseWare = PurchaseNewManager.SelectWareHouseAreaConfigList();

            return View(new ListModel<PurchaseTaskInfo>(pager, dataList));
        }
        /// <summary>
        /// 导出采购池信息
        /// RenYongQiang 2017/02/10
        /// </summary>
        public void ExportTaskPool(TaskPoolRequest request)
        {
            var browser = Request.Browser.Browser;
            request.PageNumber = 0;
            var list = PurchaseNewManager.SelectPurchaseTaskPoolList(request).ReturnValue;
            var response = System.Web.HttpContext.Current.Response;

            response.ClearContent();
            response.Buffer = true;
            response.Charset = "GB2312";
            var destFileName = "采购任务池(" + DateTime.Now.ToString("yyyy-MM-dd") + ").xls";

            if (browser.Contains("MS") && browser.Contains("IE"))
            {
                response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(destFileName), System.Text.Encoding.UTF8));
            }
            else if (browser.Contains("FIREFOX"))
            {
                var outputFileName = "\"" + Path.GetFileName(destFileName) + "\"";
                response.AppendHeader("Content-Disposition", "attachment;filename=" + outputFileName);
            }
            else
            {
                response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(destFileName), System.Text.Encoding.UTF8));
            }
            response.ContentType = "application/ms-excel";

            var ms = new MemoryStream();

            var excel = new HSSFWorkbook();
            var style = excel.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center; // 左右居中   
            style.VerticalAlignment = VerticalAlignment.Justify; // 上下居中   
            style.WrapText = false;

            //创建工作簿
            var sheet = excel.CreateSheet("采购任务池数据");

            var title = new List<string>
            {
                "任务编号",
                "产品名称",
                "区域",
                "区域需求量",
                "仓库名称",
                "需求量",
                "任务状态",
                "任务关闭原因",
                "任务主人",
                "创建时间",
                "完成时间"
            };

            //创建标题行
            var titleRow = sheet.CreateRow(0);
            for (var i = 0; i < title.Count; i++)
            {
                var text = title[i];

                if (string.IsNullOrEmpty(text)) continue;

                var cell = titleRow.CreateCell(i);
                cell.SetCellValue(text);
                cell.CellStyle = style;
            }
            sheet.CreateFreezePane(0, 1, 0, 1);
            //写入数据

            var rowcount = 1;
            foreach (var plans in list.GroupBy(p => new { p.PID, p.Region }))
            {
                var count = plans.Count();
                for (var i = 0; i < count; i++)
                {
                    var row = sheet.CreateRow(rowcount);

                    var info = plans.ToList()[i];

                    row.CreateCell(0).SetCellValue(info.PKID);
                    row.GetCell(0).CellStyle = style;
                    if (i == 0)
                    {
                        row.CreateCell(1).SetCellValue(plans.First().ProductName);
                        row.GetCell(1).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 1, 1));

                        row.CreateCell(2).SetCellValue(plans.First().Region);
                        row.GetCell(2).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 2, 2));

                        row.CreateCell(3).SetCellValue(plans.Sum(x => x.DemandCount));
                        row.GetCell(3).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 3, 3));
                    }

                    row.CreateCell(4).SetCellValue(info.WareHouseName);
                    row.GetCell(4).CellStyle = style;
                    row.CreateCell(5).SetCellValue(info.DemandCount);
                    row.GetCell(5).CellStyle = style;
                    row.CreateCell(6).SetCellValue(info.TaskState);
                    row.GetCell(6).CellStyle = style;
                    row.CreateCell(7).SetCellValue(info.CloseReson);
                    row.GetCell(7).CellStyle = style;
                    row.CreateCell(8).SetCellValue(info.TaskMaster);
                    row.GetCell(8).CellStyle = style;
                    row.CreateCell(9).SetCellValue(info.CreateDate.ToString("yyyy/MM/dd HH:mm:ss"));
                    row.GetCell(9).CellStyle = style;
                    row.CreateCell(10).SetCellValue(info.FinishDate <= Convert.ToDateTime("1900-01-01") ? "" : info.FinishDate.ToString("yyyy/MM/dd HH:mm:ss"));
                    row.GetCell(10).CellStyle = style;

                    rowcount++;
                }

            }
            //列宽自适应，只对英文和数字有效  
            for (var i = 0; i <= 14; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            //获取当前列的宽度，然后对比本列的长度，取最大值  
            for (var columnNum = 0; columnNum <= 18; columnNum++)
            {
                if (columnNum == 0)
                {
                    sheet.SetColumnWidth(columnNum, 9 * 256);
                    continue;
                }
                var columnWidth = sheet.GetColumnWidth(columnNum) / 256;
                for (var rowNum = 1; rowNum <= sheet.LastRowNum; rowNum++)
                {
                    IRow currentRow;
                    //当前行未被使用过  
                    if (sheet.GetRow(rowNum) == null)
                    {
                        currentRow = sheet.CreateRow(rowNum);
                    }
                    else
                    {
                        currentRow = sheet.GetRow(rowNum);
                    }

                    if (currentRow.GetCell(columnNum) != null)
                    {
                        var currentCell = currentRow.GetCell(columnNum);
                        var length = Encoding.Default.GetBytes(currentCell.ToString()).Length;
                        if (columnWidth < length)
                        {
                            columnWidth = length;
                        }
                    }
                }
                sheet.SetColumnWidth(columnNum, columnWidth * 256);
            }
            excel.Write(ms);
            response.BinaryWrite(ms.ToArray());
            ms.Close();
            ms.Dispose();
            response.End();
        }
        /// <summary>
        /// 手动分配采购任务
        /// RenYongQiang 2017/02/10
        /// </summary>
        public ActionResult AssignPurchaseTaskMaster(string master, string ids)
        {
            if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(master))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(PurchaseNewManager.UpdatePurchaseTaskMaster(master, ids), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 手动创建任务下单页面
        /// RenYongQiang 2017/02/22
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPurchaseTask()
        {
            ViewBag.VendorId = new VenderManager().SelectAllVenderForQueries();
            ViewBag.HouseWare = PurchaseNewManager.SelectWareHouseAreaConfigList();

            return View();
        }
        /// <summary>
        /// 获取人工创建采购任务下单数据
        /// RenYongQiang 2017/02/22
        /// </summary>
        public ActionResult GetCreatManuallyPlaceOrderList(int vendorId, int houseId)
        {
            if (vendorId <= 0 || houseId < 0)
            {
                return Json(new List<PurchaseTaskPlaceOrder>(), JsonRequestBehavior.AllowGet);
            }

            return Json(PurchaseNewManager.SelectCreatManuallyPlaceOrderList(vendorId, houseId),
                JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 人工批量新增采购任务下单数据
        /// RenYongQiang 2017/02/22
        /// </summary>
        public ActionResult InsertBatchCreatManuallyPlaceOrder(string placeOrder)
        {
            var list = JsonConvert.DeserializeObject<List<AjaxTaskPlaceOrder>>(placeOrder);

            if (list.Any( x => x.VendorId <= 0 || x.WareHouseId < 0 
                            || string.IsNullOrEmpty(x.PID) 
                            || string.IsNullOrEmpty(x.WareHouseName) 
                            || string.IsNullOrEmpty(x.VendorName)))
            {
                return Json("添加失败，数据异常！", JsonRequestBehavior.AllowGet);
            }

            PurchaseNewManager.BatchInsertPlaceOrderByManually(list);

            return Json("新增成功！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 导入采购任务下单数据
        /// RenYongQiang 2017/02/23
        /// </summary>
        [HttpPost]
        public ActionResult ImportCreatManuallyPlaceOrder(HttpPostedFileBase efile, int vendorId, string vendorName, int wareId, string wareName)
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
                var colums = "产品名称,产品编号,采购价格,采购数量";
                errors.AddRange(from msg in colums.Split(',') where dt.Columns[msg] == null select $"该导入的文件中没有\"{msg}\"这一列！");
                if (errors.Count > 0)
                {
                    return Json(errors, JsonRequestBehavior.AllowGet);
                }
                //过滤空白行
                dt = dt.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("产品名称"))).CopyToDataTable();

                var placeList = new List<AjaxTaskPlaceOrder>();
                //导入数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    var place = new AjaxTaskPlaceOrder()
                    {
                        VendorName = wareName,
                        VendorId = vendorId,
                        WareHouseName = vendorName,
                        WareHouseId = wareId
                    };
                    //产品名称
                    if (string.IsNullOrEmpty(row["产品名称"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品名称！");
                        continue;
                    }
                    place.ProductName = row["产品名称"].ToString();
                    //产品编号
                    if (string.IsNullOrEmpty(row["产品编号"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写产品编号！");
                        continue;
                    }
                    place.PID = row["产品编号"].ToString();
                    //采购价格
                    if (string.IsNullOrEmpty(row["采购价格"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写采购价格！");
                        continue;
                    }
                    place.PurchasePrice = Convert.ToDecimal(row["采购价格"]);
                    //采购数量
                    if (string.IsNullOrEmpty(row["采购数量"].ToString()))
                    {
                        errors.Add($"第{i + 1}行导入失败,未填写采购数量！");
                        continue;
                    }
                    place.PurchaseCount = Convert.ToInt32(row["采购数量"]);

                    placeList.Add(place);
                }

                if (placeList.Count > 0)
                {
                    PurchaseNewManager.BatchInsertPlaceOrderByManually(placeList);
                    errors.Add("本次共成功导入" + placeList.Count + "行!");
                }
                else
                {
                    errors.Add("导入失败,无有效数据！");
                }

                return Json(errors, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("采购任务下单", "导入", ex.ToString(), ThreadIdentity.Operator.Name, "采购任务下单导入",
                    MonitorLevel.Error, MonitorModule.Purchase);
                errors.Add("导入错误，操作无法进行！");
                return Json(errors, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// 删除采购任务下单产品
        /// RenYongQiang 2017/02/23
        /// </summary>
        public ActionResult DeleteTaskPlaceOrder(int tpId)
        {
            if (tpId > 0)
            {
                return Json(PurchaseNewManager.DeletePlaceOrderById(tpId), JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 创建采购任务提交
        /// RenYongQiang 2017/02/23
        /// </summary>
        /// <returns></returns>
        public ActionResult PresentTaskPlaceOrder(string purchOrder)
        {
            var order = JsonConvert.DeserializeObject<PurchasePlaceModel>(purchOrder);
            //判断数据
            if (order.VendorId <= 0 || order.WareHouseId < 0 || string.IsNullOrEmpty(order.ShipmentType) ||
                string.IsNullOrEmpty(order.DeliveryDate) || order.VendorRate <= 0 || !order.OrderProductList.Any()
                || order.OrderProductList.Any( x => string.IsNullOrEmpty(x.PID) || x.FreightPrice < 0 || 
                                                x.PurchasePrice <= 0 || x.PurchaseCount <= 0))
            {
                return Json("失败，提交的数据异常！", JsonRequestBehavior.AllowGet);
            }
            //判断重复产品
            if (order.OrderProductList.Distinct(x => x.PID).Count() != order.OrderProductList.Count())
            {
                return Json("失败，提交中包涵重复产品！", JsonRequestBehavior.AllowGet);
            }

            return Json(PurchaseNewManager.ManuallyTaskBatchCreatPurchaseOrder(order), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 采购任务执行
        /// <summary>
        /// 采购任务执行主页
        /// RenYongQiang 2017/02/13
        /// </summary>
        //[PowerManage]
        public ActionResult ExecuteTaskIndex(TaskPoolRequest request)
        {
            var list = PurchaseNewManager.SelectExecutePurchaseTaskList(request);

            ViewBag.VendorList = PurchaseNewManager.SelectAllExecuteTaskVendorList(request);
            //只刷新列表
            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                return View("ExecuteTaskList", list);
            }
            ViewBag.Brand = new CargoManager().GetProductBrandList();//品牌
            ViewBag.WareHouse = PurchaseNewManager.SelectWareHouseAreaConfigList();//仓库
            ViewBag.Area = PurchaseNewManager.SelectAreaList();//区域
            ViewBag.Complete = PurchaseNewManager.GetCompletedTaskCount();//完成的数量

            return View(list);

        }
        /// <summary>
        /// 导出采购任务执行信息
        /// RenYongQiang 2017/02/13
        /// </summary>
        public void ExportExecuteTask(TaskPoolRequest request)
        {
            var browser = Request.Browser.Browser;
            var response = System.Web.HttpContext.Current.Response;

            response.ClearContent();
            response.Buffer = true;
            response.Charset = "GB2312";
            var destFileName = "采购任务执行(" + DateTime.Now.ToString("yyyy-MM-dd") + ").xls";

            if (browser.Contains("MS") && browser.Contains("IE"))
            {
                response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(destFileName), System.Text.Encoding.UTF8));
            }
            else if (browser.Contains("FIREFOX"))
            {
                var outputFileName = "\"" + Path.GetFileName(destFileName) + "\"";
                response.AppendHeader("Content-Disposition", "attachment;filename=" + outputFileName);
            }
            else
            {
                response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(destFileName), System.Text.Encoding.UTF8));
            }
            response.ContentType = "application/ms-excel";

            var ms = new MemoryStream();

            var excel = new HSSFWorkbook();
            var style = excel.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center; // 左右居中   
            style.VerticalAlignment = VerticalAlignment.Justify; // 上下居中   
            style.WrapText = false;

            //创建工作簿
            var sheet = excel.CreateSheet("采购任务执行数据");
            var list = PurchaseNewManager.SelectExecutePurchaseTaskList(request);
            var vendors = PurchaseNewManager.SelectAllExecuteTaskVendorList(request);

            var title = new List<string>
            {
                "任务编号",
                "产品名称",
                "区域",
                "区域需求量",
                "平均采购价",
                "最低采购价",
                "工厂指导价",
                "活动指导价",
                "仓库名称",
                "需求量"
            };

            //创建标题行
            var titleRow = sheet.CreateRow(0);
            for (var i = 0; i < title.Count; i++)
            {
                var text = title[i];
                if (string.IsNullOrEmpty(text)) continue;
                var cell = titleRow.CreateCell(i);
                cell.SetCellValue(text);
                cell.CellStyle = style;
                sheet.AddMergedRegion(new CellRangeAddress(0, 1, i, i));
            }
            //供应商标题
            var towRow = sheet.CreateRow(1);
            for (var j = 0; j < vendors.Count; j++)
            {
                var cell = titleRow.CreateCell(j*2 + 10);
                cell.SetCellValue(vendors[j].VendorName);
                cell.CellStyle = style;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, j*2 + 10, j*2 + 11));

                var celt1 = towRow.CreateCell(j*2 + 10, CellType.String);
                celt1.SetCellValue("供货价格");
                celt1.CellStyle = style;

                var celt2 = towRow.CreateCell(j*2 + 11, CellType.String);
                celt2.SetCellValue("可供数量");
                celt2.CellStyle = style;
            }

            //写入数据
            var rowcount = 2;
            foreach (var plans in list.GroupBy(p => new { p.PID, p.Region }))
            {
                var count = plans.Count();
                for (var i = 0; i < count; i++)
                {
                    var row = sheet.CreateRow(rowcount);
                    var info = plans.ToList()[i];

                    row.CreateCell(0).SetCellValue(info.PKID);
                    row.GetCell(0).CellStyle = style;
                    if (i == 0)
                    {
                        row.CreateCell(1).SetCellValue(plans.First().ProductName);
                        row.GetCell(1).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 1, 1));

                        row.CreateCell(2).SetCellValue(plans.First().Region);
                        row.GetCell(2).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 2, 2));

                        row.CreateCell(3).SetCellValue(plans.Sum(x => x.DemandCount));
                        row.GetCell(3).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 3, 3));

                        row.CreateCell(4).SetCellValue(Convert.ToDouble(plans.First().AveragePrice));
                        row.GetCell(4).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 4, 4));

                        row.CreateCell(5).SetCellValue(Convert.ToDouble(plans.First().MinimumPrice));
                        row.GetCell(5).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 5, 5));

                        row.CreateCell(6).SetCellValue(Convert.ToDouble(plans.First().FactoryPrice));
                        row.GetCell(6).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 6, 6));

                        row.CreateCell(7).SetCellValue(Convert.ToDouble(plans.First().ActivityPrice));
                        row.GetCell(7).CellStyle = style;
                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), 7, 7));

                    }

                    row.CreateCell(8).SetCellValue(info.WareHouseName);
                    row.GetCell(8).CellStyle = style;
                    row.CreateCell(9).SetCellValue(info.DemandCount);
                    row.GetCell(9).CellStyle = style;

                    if (i == 0)
                    {
                        for (var j = 0; j < vendors.Count; j++)
                        {
                            var mv = plans.First().InquiryProductList.FirstOrDefault(x => x.VendorId == vendors[j].VendorId);

                            row.CreateCell(j*2 + 10).SetCellValue(mv == null ? "" : mv.InquiryProductState == "已提交" ? mv.QuotedPrice.ToString("0.00") : "0");
                            row.GetCell(j*2 + 10).CellStyle = style;
                            sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), j*2 + 10, j*2 + 10));

                            row.CreateCell(j*2 + 11).SetCellValue(mv == null ? "" : mv.InquiryProductState == "已提交" ? mv.ProvideCount.ToString() : "0");
                            row.GetCell(j*2 + 11).CellStyle = style;
                            sheet.AddMergedRegion(new CellRangeAddress(rowcount, (count + rowcount - 1), j*2 + 11, j*2 + 11));
                        }
                    }

                    rowcount++;
                }
            }
            excel.Write(ms);
            response.BinaryWrite(ms.ToArray());
            ms.Close();
            ms.Dispose();
            response.End();
        }
        /// <summary>
        /// 获取任务
        /// RenYongQiang 2017/02/13
        /// </summary>
        public ActionResult GetMyPurchaseTask()
        {
            return Json(PurchaseNewManager.UpdateTaskMasterPassive(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 关闭任务页面
        /// RenYongQiang 2017/02/14
        /// </summary>
        public ActionResult SelectTaskCloseReson()
        {
            return View();
        }
        /// <summary>
        /// 关闭任务
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult ClosePurchaseTask(int taskId, string closeReson)
        {
            if (!string.IsNullOrEmpty(closeReson))
            {
                PurchaseNewManager.UpdatePurchaseTaskStateToClose(taskId, closeReson);

                return Json("关闭任务成功！", JsonRequestBehavior.AllowGet);
            }

            return Json("关闭理由不能为空！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 重新推送页面
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult VendorPurshAgain(int taskId)
        {
            var model = PurchaseNewManager.GetPurchaseTaskInfoById(taskId);
            if (model != null)
            {
                model.InquiryProductList = PurchaseNewManager.SelectInquiryProductListByPId(model.PID);
            }
            return View(model);
        }
        /// <summary>
        /// 重新推送询价
        /// RenYongQiang 2017/02/15
        /// </summary>
        public ActionResult PushAgainVenderInquiryProduct(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                foreach (var id in ids.Split(','))
                {
                    PurchaseNewManager.InquiryProductPushAgain(Convert.ToInt32(id));
                }

                return Json("重新推送成功！", JsonRequestBehavior.AllowGet);
            }

            return Json("未选择可推送的供应商！", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 根据区域获取仓库列表
        /// 2017/02/10
        /// </summary>
        public ActionResult GetHousesByArea(string area, int type)
        {
            if (!string.IsNullOrWhiteSpace(area))
            {
                var data = "<option value=\"\">请选择</option>" +
                           PurchaseNewManager.SelectWareHouseAreaConfig(new PurchaseWareHouseAreaConfig() { Area = area }).Aggregate("",
                               (current, info) =>
                                   current +
                                   ("<option value='" + (type == 1 ? info.WareHouseId.ToString() : info.WareHouseName) +
                                    "'>" + info.WareHouseName + "</option>"));

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json("错误，area为空", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取供应商税率下拉框数据源
        /// RenYongQiang 2017/02/22
        /// </summary>
        public ActionResult GetVenderRateSelectData(int venderId)
        {
            if (venderId <= 0)
            {
                return Json("<option value=''>请选择</option>", JsonRequestBehavior.AllowGet);
            }
            var list = new PurchaseNewManager().SelectVenderInvoiceRateByVenderId(venderId);
            if (!list.Any())
            {
                return Json("<option value=''>请选择</option>", JsonRequestBehavior.AllowGet);
            }
            var options = list.Aggregate("", (current, info) => current + ("<option value='" + Math.Round(Convert.ToDecimal(info.Rate)/100, 2) + "'>" + info.Rate + "%</option>"));

            return Json(options, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}