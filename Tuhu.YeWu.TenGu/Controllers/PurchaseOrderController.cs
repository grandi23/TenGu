using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using ThBiz.Business.CargoManagement;
using ThBiz.Business.EmployeeManagement;
using ThBiz.Business.Purchase;
using ThBiz.Common.Entity;
using ThBiz.DataAccess.Entity;
using Tuhu.Component.Common.Models;
using Webdiyer.WebControls.Mvc;

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

        #endregion
    }
}