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
    public class InfoListController : Controller
    {
        private static readonly ILog Logger = LoggerFactory.GetLogger("PurchaseOrderController");
        private readonly Lazy<PurchaseManager> _lazyPurchaseManager = new Lazy<PurchaseManager>();
        private readonly Lazy<PurchaseAlwaysOnReadManager> _lazyPurchaseAlwaysOnReadManager = new Lazy<PurchaseAlwaysOnReadManager>();
        private readonly Lazy<PurchaseReadOnlyManager> _lazyPurchaseReadOnlyManager = new Lazy<PurchaseReadOnlyManager>();
        private PurchaseManager PurchaseManager => _lazyPurchaseManager.Value;
        private PurchaseAlwaysOnReadManager PurchaseAlwaysOnReadManager => _lazyPurchaseAlwaysOnReadManager.Value;
        private PurchaseReadOnlyManager PurchaseReadOnlyManager => _lazyPurchaseReadOnlyManager.Value;

        #region 采购订单列表
        /// <summary>
        /// 采购订单列表主页
        /// 2017/01/24
        /// </summary>
        //[PowerManage]
        public ActionResult PurchaseOrderIndex(int? wareHouseId, int? purchaseOrderId, int? purchaseId, string status, string productName = null)
        {
            ViewBag.Employee = new EmployeeAlwaysOnManager().SelectHrEmployees();
            ViewBag.VendorList = new VenderManager().SelectAllVenderForQueries(false);
            ViewBag.WareHourse = PublicFunction.GetShopMessagesByShopType(4);
            ViewBag.Status = status;

            return View();
        }
        /// <summary>
        /// 采购订单列表数据
        /// </summary>
        public ActionResult PurchaseOrderList(int pageNumber = 1)
        {
            try
            {
                var searchModel = new PurchaseOrderSearchModel()
                {
                    WareHouseId =
                        string.IsNullOrWhiteSpace(Request.QueryString["wareHouseId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["wareHouseId"]),
                    ProductName = Request.QueryString["productName"],
                    PurchaseOrderId =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseOrderId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseOrderId"]),
                    PurchaseId =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseId"]),
                    VendorId =
                        string.IsNullOrWhiteSpace(Request.QueryString["vendorId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["vendorId"]),
                    StartDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["startDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["startDate"]),
                    EndDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["endDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["endDate"]),
                    PlanStartDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["planStartDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["planStartDate"]),
                    PlanEndDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["planEndDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["planEndDate"]),
                    Employee = Request.QueryString["employee"],
                    PayStatus = Request.QueryString["payStatus"],
                    AuditStatus = Convert.ToInt32(Request.QueryString["auditStatus"]),
                    Status = Request.QueryString["status"],
                    ReceiptInvoice =
                        string.IsNullOrWhiteSpace(Request.QueryString["receiptInvoice"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["receiptInvoice"]),
                    PurchaseMode =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseMode"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseMode"]),
                    IsBookingOrder =
                        string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrder"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["isBookingOrder"]),
                    IsBookingOrdered =
                        string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrdered"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["isBookingOrdered"]),
                    isShopPurchaseOrder = string.IsNullOrWhiteSpace(Request.QueryString["isShopPurchaseOrder"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["isShopPurchaseOrder"]),
                };
                int totalCount;
                var model = PurchaseAlwaysOnReadManager.GetPurchaseOrderItemList(searchModel, pageNumber, 15, out totalCount);
                return View((List<PurchaseOrderModel>) model.Item1);
            }
            catch (Exception ex)
            {
                new PurchaseNewManager().InsertPurchaseMonitorLog(new PurchaseMonitorLog()
                {
                    ObjectID = string.IsNullOrWhiteSpace(Request.QueryString["purchaseId"])
                            ? 0
                            : Convert.ToInt32(Request.QueryString["purchaseId"]),
                    ObjectType = PurchaseMonitorTypeEnum.Error.ToString(),
                    AfterValue = "错误信息:" + ex,
                    BeforeValue = "错误数据源:" + JsonConvert.SerializeObject(Request.Params),
                    CreateUser = User.Identity.Name,
                    Operation = "采购订单列表数据错误",
                    IPAddress = ClientIp
                });
                return View();
            }
        }
        /// <summary>
        /// 导出采购订单列表
        /// 2017/01/24
        /// </summary>
        //[PowerManage]
        public void ExportPurchaseOrNew()
        {
            try
            {
                var searchModel = new PurchaseOrderSearchModel()
                {
                    WareHouseId =
                        string.IsNullOrWhiteSpace(Request.QueryString["wareHouseId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["wareHouseId"]),
                    ProductName = Request.QueryString["productName"],
                    PurchaseOrderId =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseOrderId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseOrderId"]),
                    PurchaseId =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseId"]),
                    VendorId =
                        string.IsNullOrWhiteSpace(Request.QueryString["vendorId"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["vendorId"]),
                    StartDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["startDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["startDate"]),
                    EndDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["endDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["endDate"]),
                    PlanStartDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["planStartDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["planStartDate"]),
                    PlanEndDate =
                        string.IsNullOrWhiteSpace(Request.QueryString["planEndDate"])
                            ? (DateTime?)null
                            : Convert.ToDateTime(Request.QueryString["planEndDate"]),
                    Employee = Request.QueryString["employee"],
                    PayStatus = Request.QueryString["payStatus"],
                    AuditStatus = Convert.ToInt32(Request.QueryString["auditStatus"]),
                    Status = Request.QueryString["status"],
                    ReceiptInvoice =
                        string.IsNullOrWhiteSpace(Request.QueryString["receiptInvoice"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["receiptInvoice"]),
                    PurchaseMode =
                        string.IsNullOrWhiteSpace(Request.QueryString["purchaseMode"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["purchaseMode"]),
                    IsBookingOrder =
                        string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrder"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["isBookingOrder"]),
                    IsBookingOrdered =
                        string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrdered"])
                            ? (int?)null
                            : Convert.ToInt32(Request.QueryString["isBookingOrdered"])
                };

                var outData = PurchaseManager.ExportPurchaseOrderItem_New(searchModel);
                HttpResponse response = System.Web.HttpContext.Current.Response;
                response.ClearContent();
                response.Buffer = true;
                response.Charset = "GB2312";
                string destFileName = "新采购订单列表(" + DateTime.Now.ToString("yyyy-MM-dd") + ").xlsx";
                response.AppendHeader("Content-Disposition", "attachment;filename=" +
                                                                HttpUtility.UrlEncode(Path.GetFileName(destFileName),
                                                                    System.Text.Encoding.UTF8));
                response.ContentType = "application/ms-excel";

                MemoryStream ms = new MemoryStream();

                XSSFWorkbook excel = new XSSFWorkbook();
                ICellStyle style = excel.CreateCellStyle();
                style.Alignment = HorizontalAlignment.Center; // 左右居中

                //创建工作簿
                ISheet sheet = excel.CreateSheet("新采购订单列表导出");

                List<string> title = new List<string>
                {
                    "订单号",
                    "产品单号",
                    "采购方式",
                    "状态",
                    "所属仓库",
                    "供应商",
                    "产品名称",
                    "产品编号",
                    "采购数量",
                    "已入库",
                    "采购单价",
                    "采购总价",
                    "运费单价",
                    "运费记账类型",
                    "创建时间",
                    "提货方式",
                    "计划入库时间",
                    "备注",
                    "创建人",
                    "付款状态",
                };

                //创建标题行
                IRow titleRow = sheet.CreateRow(0);
                for (int i = 0; i < title.Count; i++)
                {
                    string text = title[i];
                    if (!string.IsNullOrEmpty(text))
                    {
                        ICell cell = titleRow.CreateCell(i, CellType.String);
                        cell.SetCellValue(text);
                        cell.CellStyle = style;
                    }
                }
                sheet.CreateFreezePane(0, 1, 0, 1);
                //写入数据

                int rowcount = 1;
                if (outData != null && outData.Any())
                {
                    foreach (var item in outData.OrderByDescending(_ => _.PurchaseOrderId))
                    {
                        IRow row = sheet.CreateRow(rowcount);
                        row.CreateCell(0).SetCellValue(item.PurchaseOrderId);
                        row.CreateCell(1).SetCellValue(item.PurchaseId);
                        var purchaseModeText = "暂无";
                        switch (item.PurchaseMode)
                        {
                            case 0:
                                purchaseModeText = "正常";
                                break;
                            case 1:
                                purchaseModeText = "补货";
                                break;
                            case 2:
                                purchaseModeText = "专项";
                                break;
                            case 3:
                                purchaseModeText = "扫货";
                                break;
                            case 4:
                                purchaseModeText = "零采";
                                break;
                            case 7:
                                purchaseModeText = "汽配龙";
                                break;
                            case 8:
                                purchaseModeText = "预约";
                                break;
                            case 9:
                                purchaseModeText = "活动备货";
                                break;
                            case 10:
                                purchaseModeText = "大客户";
                                break;
                            case 11:
                                purchaseModeText = "门店外采";
                                break;
                            case 12:
                                purchaseModeText = "货权转移";
                                break;
                            case 13:
                                purchaseModeText = "即采即销";
                                break;
                            case 16:
                                purchaseModeText = "汽配龙外采";
                                break;
                        }
                        row.CreateCell(2).SetCellValue(purchaseModeText);
                        row.CreateCell(3).SetCellValue(item.Status);
                        row.CreateCell(4).SetCellValue(item.WareHouse);
                        row.CreateCell(5).SetCellValue(item.VendorName);
                        row.CreateCell(6).SetCellValue(item.Name);
                        row.CreateCell(7).SetCellValue(item.Pid);
                        row.CreateCell(8).SetCellValue(item.Num);
                        row.CreateCell(9).SetCellValue(item.InstockNum.Value);
                        row.CreateCell(10).SetCellValue(item.PurchasePrice.ToString("0.00"));
                        row.CreateCell(11).SetCellValue(item.TotalPrice.ToString());
                        row.CreateCell(12).SetCellValue(item.ProductFreight.ToString("0.00"));
                        row.CreateCell(13).SetCellValue(item.FreightTpye);
                        row.CreateCell(14).SetCellValue(item.CreatedDatetime.ToString("yyyy-MM-dd HH:mm"));
                        row.CreateCell(15).SetCellValue(item.ShipmentType);
                        row.CreateCell(16).SetCellValue(item.PlanedInstockDate.ToString("yyyy-MM-dd HH:mm"));
                        row.CreateCell(17).SetCellValue(item.Remark);
                        row.CreateCell(18).SetCellValue(item.CreatedByName);

                        if (item.PayStatus == "0UnPaid")
                        {
                            row.CreateCell(19).SetCellValue("未付款");
                        }
                        else if (item.PayStatus == "2PartPaid")
                        {
                            row.CreateCell(19).SetCellValue("部分付款");
                        }
                        else if (item.PayStatus == "2Paid")
                        {
                            row.CreateCell(19).SetCellValue("已付款");

                        }
                        rowcount++;
                    }
                    excel.Write(ms);
                    response.BinaryWrite(ms.ToArray());
                    ms.Close();
                    ms.Dispose();
                    response.End();
                }
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("PurchaseOrderItem", "导出", ex.Message, ThreadIdentity.Operator.Name,
                    "导出采购单", MonitorLevel.Error, MonitorModule.Purchase);
            }
        }
        /// <summary>
        /// 导出采购订单数据到pdf
        /// 2017/01/24
        /// </summary>
        //[PowerManage]
        public ActionResult ExportDataToPdf(int purchaseId)
        {
            List<DeliveryOrderDetail> orderList = PurchaseManager.GetDeliveryOrderDetailInfo(Convert.ToInt32(purchaseId));

            string filePath = Server.MapPath(@"~/Content/export/送货预约单.pdf");

            BaseFont bf0 = BaseFont.CreateFont(Server.MapPath(@"~/Content/export/SIMHEI.TTF"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            // 创建 PDF 文档
            Document document = new Document();

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            //需要判断该文件流是否被使用
            if (fs.CanRead)
            {
                // 创建写入器实例，PDF 文件将会保存到这里
                PdfWriter.GetInstance(document, fs);
            }
            //如果已使用,需要先释放
            else
            {
                fs.Close();
                fs.Dispose();
                fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                PdfWriter.GetInstance(document, fs);
            }

            // 打开文档
            document.Open();

            Font font = new Font(bf0);



            var pTitle = new Paragraph("送货预约单", font)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };

            document.Add(pTitle);

            DeliveryOrderDetail orderDetail = orderList[0];

            document.Add(new Paragraph("订单号:" + purchaseId, font));
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            foreach (var item in orderList)
            {
                document.Add(new Paragraph("产品单号:" + item.OrderId, font));
                document.Add(new Paragraph("预约送货时间:" + item.DeliveryTime.ToString("yyyy/MM/dd HH:mm:ss"), font));

            }

            var spaceP = new Paragraph {SpacingAfter = 10};

            document.Add(spaceP);
            document.Add(new Paragraph("仓库:" + orderDetail.WareHouseName, font));
            document.Add(new Paragraph("仓库联系人:" + orderDetail.WareHouseContactUser, font));
            document.Add(new Paragraph("联系方式:" + orderDetail.ContactTeleNum, font));
            document.Add(new Paragraph("仓库地址:" + orderDetail.WareHouseAddress, font));
            document.Add(new Paragraph());
            document.Close();

            fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 5000);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();

            FileResult fileResult = new FileContentResult(bytes, "application/pdf");
            fileResult.FileDownloadName = "邀约单" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";

            return fileResult;
        }
        /// <summary>
        /// 入库操作
        /// 2017/01/24
        /// </summary>
        //[PowerManage]
        public ActionResult PoInstock(int id, string returnUrl = "/PurchaseOrder/PoItemIndex")
        {
            try
            {
                ViewBag.returnURL = returnUrl;

                var poi = PurchaseManager.GetPurchaseOrderItemEntity(id);

                if (poi.Status.StartsWith("已收货"))
                {
                    return View();
                }
                int reverseQuantity = 0;
                var purchaseReverse = PurchaseManager.GetPurchaseReverseByPoId(id);
                if (purchaseReverse.Any())
                {
                    reverseQuantity =
                        purchaseReverse.Where(_ => !_.IsRejected).Sum(_ => _.RelatedNum);
                }
                var ss = new StockLocationView
                {
                    PID = poi.PID,
                    Num = poi.Num - poi.InstockNum + reverseQuantity,
                    Name = poi.Name,
                    CostPrice = poi.CostPrice,
                    UpdatedTime = DateTime.Today,
                    TotalCost = (poi.CostPrice) * (poi.Num - poi.InstockNum),
                    Location = poi.WareHouse,
                    LocationId = poi.WareHouseID,
                };


                // 查找仓库信息,排除上海在途，北京在途，广州在途，盘亏，在途,shopType=4
                ViewBag.Location = PublicFunction.GetShopMessagesByShopType(4);
                return View(ss);
            }
            catch (Exception ex)
            {
                Logger.Log(Level.Error, ex, "Error in PoInstock,id:" + id);
            }

            return View();
        }
        /// <summary>
        /// 采购单操作历史
        /// 2017/01/24
        /// </summary>
        public ActionResult ShowModifiedHistory(int purchaseId)
        {
            var purchaseLogList = new PurchaseNewManager().GetPurchaseMonitorLogList(new PurchaseMonitorLog()
            {
                ObjectID = purchaseId,
                ObjectType = PurchaseMonitorTypeEnum.PoItem.ToString()
            });

            return View(purchaseLogList);
        }
        /// <summary>
        /// 采购订单列表获取总页数
        /// 2017/02/06
        /// </summary>
        public ActionResult SearchTotalCount()
        {
            var searchModel = new PurchaseOrderSearchModel()
            {
                WareHouseId =
                    string.IsNullOrWhiteSpace(Request.QueryString["wareHouseId"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["wareHouseId"]),
                ProductName = Request.QueryString["productName"],
                PurchaseOrderId =
                    string.IsNullOrWhiteSpace(Request.QueryString["purchaseOrderId"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["purchaseOrderId"]),
                PurchaseId =
                    string.IsNullOrWhiteSpace(Request.QueryString["purchaseId"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["purchaseId"]),
                VendorId =
                    string.IsNullOrWhiteSpace(Request.QueryString["vendorId"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["vendorId"]),
                StartDate =
                    string.IsNullOrWhiteSpace(Request.QueryString["startDate"])
                        ? (DateTime?)null
                        : Convert.ToDateTime(Request.QueryString["startDate"]),
                EndDate =
                    string.IsNullOrWhiteSpace(Request.QueryString["endDate"])
                        ? (DateTime?)null
                        : Convert.ToDateTime(Request.QueryString["endDate"]),
                PlanStartDate =
                    string.IsNullOrWhiteSpace(Request.QueryString["planStartDate"])
                        ? (DateTime?)null
                        : Convert.ToDateTime(Request.QueryString["planStartDate"]),
                PlanEndDate =
                    string.IsNullOrWhiteSpace(Request.QueryString["planEndDate"])
                        ? (DateTime?)null
                        : Convert.ToDateTime(Request.QueryString["planEndDate"]),
                Employee = Request.QueryString["employee"],
                PayStatus = Request.QueryString["payStatus"],
                AuditStatus = Convert.ToInt32(Request.QueryString["auditStatus"]),
                Status = Request.QueryString["status"],
                ReceiptInvoice =
                    string.IsNullOrWhiteSpace(Request.QueryString["receiptInvoice"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["receiptInvoice"]),
                PurchaseMode =
                    string.IsNullOrWhiteSpace(Request.QueryString["purchaseMode"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["purchaseMode"]),
                IsBookingOrder =
                    string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrder"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["isBookingOrder"]),
                IsBookingOrdered =
                    string.IsNullOrWhiteSpace(Request.QueryString["isBookingOrdered"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["isBookingOrdered"]),
                isShopPurchaseOrder = string.IsNullOrWhiteSpace(Request.QueryString["isShopPurchaseOrder"])
                        ? (int?)null
                        : Convert.ToInt32(Request.QueryString["isShopPurchaseOrder"]),
            };

            int totalCount = new PurchaseReadOnlyManager().SearchTotalCount(searchModel);

            return Content(totalCount.ToString());
        }
        /// <summary>
        /// 删除采购订单页面
        /// 2017/02/07
        /// </summary>
        public ActionResult PurchaseOrderCause(int purchaseId, int orderNo, string pid, string type = "single")
        {
            if (type == "single")
            {
                var isOccupy = PurchaseManager.CheckPoItemIsOccupy(purchaseId);
                if (isOccupy)
                {
                    return Json("Occupy", JsonRequestBehavior.AllowGet);
                }
                var poItem = PurchaseManager.GetPurchaseOrderByPurchaseId(purchaseId);
                var deletePower = new List<string>
                {
                    "fanzhou@tuhu.cn",
                    "zhangxiaowen@tuhu.cn",
                    "huqi@tuhu.cn",
                    "jiangxiaoling@tuhu.cn"
                };

                if (!deletePower.Contains(ThreadIdentity.Operator.Name) && poItem.Status.StartsWith("已发货") || poItem.Status.StartsWith("部分收货") || poItem.Status.StartsWith("已收货") || poItem.Status.StartsWith("已取消"))
                {
                    return Json("false" + poItem.Status, JsonRequestBehavior.AllowGet);
                }
                if (poItem.PayStatus == "2Paid")
                {
                    return Json("2Paid", JsonRequestBehavior.AllowGet);
                }
                if (poItem.PayStatus == "2PartPaid" && poItem.InstockNum == 0)
                {
                    if (!PurchaseManager.CheckReverseGreaterThanEqualsApplyQuantity(purchaseId))
                    {
                        return Json("2PartPaid", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var result = PurchaseManager.IsPurchaseApplyPayment(purchaseId);
                    if (result)
                        return Json("Forbid", JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.OrderNo = orderNo;
            ViewBag.PID = pid;
            return View();
        }
        /// <summary>
        /// 删除采购单操作
        /// 2017/02/07
        /// </summary>
        //[PowerManage]
        public ActionResult DeletePurchaseOrderForPurchaseList(string purchaseOrderJson)
        {

            List<PurchaseOrderObject> purchaseOrderList = JsonConvert.DeserializeObject<List<PurchaseOrderObject>>(purchaseOrderJson);
            List<BasicInfo> basicInfoList = new List<BasicInfo>();

            foreach (var item in purchaseOrderList)
            {
                var isOccupy = PurchaseManager.CheckPoItemIsOccupy(item.PurchaseOrderId);
                var result = PurchaseManager.IsPurchaseApplyPayment(item.PurchaseOrderId);
                var poItem = PurchaseManager.GetPurchaseOrderByPurchaseId(item.PurchaseOrderId);

                if (isOccupy)
                {
                    basicInfoList.Add(new BasicInfo { Name = item.PurchaseOrderId.ToString(), Value = "Occupy" });
                }
                else if (result)
                {
                    basicInfoList.Add(new BasicInfo { Name = item.PurchaseOrderId.ToString(), Value = "Forbid" });
                }


                else if (poItem.Status.StartsWith("已发货") || poItem.Status.StartsWith("部分收货") || poItem.Status.StartsWith("已收货") ||
                     poItem.Status.StartsWith("已取消"))
                {
                    basicInfoList.Add(new BasicInfo { Name = item.PurchaseOrderId.ToString(), Value = poItem.Status });

                }
                else if (poItem.PayStatus == "2Paid")
                {
                    basicInfoList.Add(new BasicInfo { Name = item.PurchaseOrderId.ToString(), Value = "2Paid" });
                }
                else
                {
                    var pocm = new PurchaseOrderCauseModel
                    {
                        OrderNo = item.PurchaseId,
                        Pid = item.Pid,
                        DeleteReason = item.RejectReason,
                        CreaterName = ThreadIdentity.Operator.Name.Trim(),
                        CreaterDate = DateTime.Now,
                        purchaseId = item.PurchaseOrderId
                    };
                    PurchaseManager.DeletePoItem(item.PurchaseOrderId, pocm);

                    var log = new PurchaseMonitorLog
                    {
                        ObjectType = PurchaseMonitorTypeEnum.PoItem.ToString(),
                        ObjectID = item.PurchaseOrderId,
                        AfterValue = string.Empty,
                        BeforeValue = "删除原因：" + pocm.DeleteReason,
                        CreateUser = ThreadIdentity.Operator.Name,
                        Operation = "删除采购订单",
                        IPAddress = ClientIp.ToString()


                    };
                    new PurchaseNewManager().InsertPurchaseMonitorLog(log);

                    if ((poItem.Status.StartsWith("新建") || poItem.Status.StartsWith("待审核")) && poItem.PayStatus == "0UnPaid" && poItem.DataType == "Purchase")
                    {
                        new AccountingManager().UpdateUsedAdjustMoney(poItem.VendorId, -poItem.DiffPrice * poItem.Num, ThreadIdentity.Operator.Name);
                        PurchaseManager.DeletePurchaseDeductionDetails(item.PurchaseOrderId);
                    }
                    basicInfoList.Add(new BasicInfo { Name = item.PurchaseOrderId.ToString(), Value = "Success" });

                }
            }

            //string result = string.Empty;
            //只需要更新PurchaseOrderItem 的状态为"已取消"  PurchaseOrder 的状态不需要改变
            //result = new PurchaseManager().DeletePurchaseOrderForPurchaseList(purchaseId);
            return Json(basicInfoList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 编辑采购订单页面
        /// 2017/02/07
        /// </summary>
        public ActionResult EditPurchaseOrderItem(int purchaseId, string status)
        {
            ViewBag.VendorList = new VenderManager().SelectAllVenderForQueries();
            ViewBag.ShipmentType = new PurchaseManager().GetBizDictionaryByDicType("ShipmentType");
            var orderInfo = new OrderManager().GetOrderByPurchaseOrderId(purchaseId);
            ViewBag.OrderInfo = orderInfo;
            var poItem = DbHelper.Parse<PurchaseOrderItems>(new PurchaseManager().GetPurchaseItemByPurchaseId(purchaseId));
            ViewBag.status = status;
            ViewBag.Reason = new PurchaseManager().GetReasonByPurchaseId(purchaseId);

            return View(poItem);
        }
        /// <summary>
        /// 编辑采购订单操作
        /// 2017/02/07
        /// </summary>
        [HttpPost]
        public ActionResult EditPoItem(PurchaseOrderItems model)
        {
            try
            {
                var modelId = model.PKID;
                var poItem = new PurchaseManager().GetPurchaseOrderByPurchaseId(modelId);
                var flag = PurchaseManager.CheckPoItemIsOccupy(modelId);
                var oldpoItem = poItem.DeepCopy();
                if (!flag)//未占用
                {
                    model.UpdatedBy = User.Identity.Name;

                    var status = oldpoItem.Status.Trim() == "已驳回" ||
                                 oldpoItem.Status.Trim() == "新建" &&
                                 (oldpoItem.PurchasePrice != model.PurchasePrice || oldpoItem.Num != model.Num) &&
                                 oldpoItem.PID.StartsWith("TR-")
                        ? "待审核"
                        : oldpoItem.Status;

                    poItem.Num = model.Num;
                    poItem.TotalPrice = (poItem.Num * poItem.PurchasePrice);
                    poItem.TotalCost = (poItem.Num * poItem.CostPrice);
                    poItem.PlanedInstockDate = model.PlanedInstockDate;
                    poItem.Remark = model.Remark;
                    poItem.ShipmentType = model.ShipmentType;

                    //查找这个采购订单占用的情况,
                    ModelState.Clear();
                    var pm = new PurchaseManager();
                    pm.UpdateShortPurchaseOrderItem(poItem);
                    //pm.UpdatePurchaseInforByPkid(poItem); //PurchaseOrderItemEntity

                    if (status == "待审核")
                    {
                        pm.UpdatePurchaseOrderStatus(poItem.POId, status); //更新采购单（PurchaseOrder表）状态
                    }

                    new PurchaseNewManager().InsertPurchaseMonitorSerializLog(ClientIp, string.Empty,
                        PurchaseMonitorTypeEnum.PoItem.ToString(), model.PKID,
                        typeof (Models.PurchaseOrderItem), "编辑采购单", oldpoItem, poItem);

                    SaveOprLog("PoItem", model.PKID, typeof(Models.PurchaseOrderItem), "编辑采购订单", oldpoItem, poItem);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }

                if (poItem.Remark != model.Remark && poItem.ShipmentType == model.ShipmentType
                    && poItem.PlanedInstockDate.ToShortDateString() == model.PlanedInstockDate.ToShortDateString())//已占用，只修改备注
                {
                    poItem.Remark = model.Remark;

                    new PurchaseManager().UpdateShortPurchaseOrderItemRemark(poItem.PKID, poItem.Remark);

                    new PurchaseNewManager().InsertPurchaseMonitorSerializLog(ClientIp, string.Empty,
                        PurchaseMonitorTypeEnum.PoItem.ToString(), model.PKID,
                        typeof(Models.PurchaseOrderItem), "编辑采购单备注", oldpoItem, poItem);

                    SaveOprLog("PoItem", model.PKID, typeof(Models.PurchaseOrderItem), "编辑采购订单备注", poItem, model);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("采购", model.PKID.ToString(CultureInfo.InvariantCulture), ex.Message,
                    User.Identity.Name, "编辑采购", MonitorLevel.Error, MonitorModule.Purchase);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 释放销售单
        /// 2017/02/07
        /// </summary>
        [HttpGet]
        public string RemovePruchaseOrder(int purchaseOrderId = 0)
        {
            var soItmeId = 0;

            var saleOrderAndPurchaseOrder = new PurchaseManager().GetSoPoList(purchaseOrderId, -1);

            foreach (var item in saleOrderAndPurchaseOrder)
            {
                RemoveSoPoWhenEditArrivedTime(item.PKID, out soItmeId);
            }
            return "true";
        }
        /// <summary>
        /// 删除子采购单
        /// 2017/02/07
        /// </summary>
        //[PowerManage]
        public ActionResult DeletePoItem(int purchaseId, int orderNo, string pid, string deleteReason)
        {
            var isOccupy = PurchaseManager.CheckPoItemIsOccupy(purchaseId);
            if (isOccupy)
            {
                return Json("Occupy", JsonRequestBehavior.AllowGet);
            }
            var poItem = PurchaseManager.GetPurchaseOrderByPurchaseId(purchaseId);
            var deletePower = new List<string>
            {
                "fanzhou@tuhu.cn",
                "zhangxiaowen@tuhu.cn",
                "huqi@tuhu.cn",
                "jiangxiaoling@tuhu.cn"
            };
            if (!deletePower.Contains(ThreadIdentity.Operator.Name) && poItem.Status.StartsWith("已发货") || poItem.Status.StartsWith("部分收货") || poItem.Status.StartsWith("已收货") || poItem.Status.StartsWith("已取消"))
            {
                return Json("false" + poItem.Status, JsonRequestBehavior.AllowGet);
            }
            if (poItem.PayStatus == "2Paid")
            {
                return Json("2Paid", JsonRequestBehavior.AllowGet);
            }
            if (poItem.PayStatus == "2PartPaid" && poItem.InstockNum == 0)
            {
                if (!PurchaseManager.CheckReverseGreaterThanEqualsApplyQuantity(purchaseId))
                {
                    return Json("2PartPaid", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var result = PurchaseManager.IsPurchaseApplyPayment(purchaseId);
                if (result)
                    return Json("Forbid", JsonRequestBehavior.AllowGet);
            }
            var pocm = new PurchaseOrderCauseModel
            {
                OrderNo = orderNo,
                Pid = pid,
                DeleteReason = deleteReason,
                CreaterName = ThreadIdentity.Operator.Name.Trim(),
                CreaterDate = DateTime.Now,
                purchaseId = purchaseId
            };
            PurchaseManager.DeletePoItem(purchaseId, pocm);

            if (poItem.PayStatus == "0UnPaid" && poItem.DataType == "Purchase")
            {
                new AccountingManager().UpdateUsedAdjustMoney(poItem.VendorId, -poItem.DiffPrice * poItem.Num, ThreadIdentity.Operator.Name);
                PurchaseManager.DeletePurchaseDeductionDetails(purchaseId);
            }

            #region 百世同步取消订单

            //if (poItem != null && BaishiWmsUtil.GetIsOpenBaishiWMS() &&
            //    new BaishiManager().IsExternalShop(poItem.WareHouseID))
            //{
            //    SyncCancelAsnOrder(purchaseId);
            //    SaveOprLog("PoItem", purchaseId, typeof(PurchaseOrderItem), "同步删除采购订单", null);
            //}

            #endregion
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 创建采购红冲页面
        /// 2017/02/07
        /// </summary>
        public ActionResult GetPurchaseReverseData(int purchaseId, string type)
        {
            var reverseData = new PurchaseOrderItemEntity();
            var purchaseData = new PurchaseManager().FetchPurchaseOrderDataById(purchaseId);

            if (type == "audit")
            {
                return View("CreatePurchaseReverse", purchaseData);
            }
            if (!string.IsNullOrWhiteSpace(purchaseData.Status) && (purchaseData.Status.Trim() == "新建" || purchaseData.Status.Trim() == "已发货")
                && (purchaseData.PayStatus == "2Paid" || purchaseData.PayStatus == "2PartPaid"))
            {
                reverseData.PKID = purchaseId;
                reverseData.POId = purchaseData.POId;
                reverseData.PID = purchaseData.PID;
                reverseData.Name = purchaseData.Name;
                reverseData.VendorId = purchaseData.VendorId;
                reverseData.VendorName = purchaseData.VendorName;
                reverseData.Num = purchaseData.Num;
                reverseData.PurchasePrice = purchaseData.PurchasePrice;
                reverseData.Rebate1 = purchaseData.Rebate1;
                reverseData.Rebate2 = purchaseData.Rebate2;
                reverseData.Rebate3 = purchaseData.Rebate3;
                reverseData.Invoice = purchaseData.Invoice;
                reverseData.CostPrice = purchaseData.PurchasePrice - purchaseData.Rebate1 - purchaseData.Rebate2 -
                                        purchaseData.Rebate3;
                reverseData.TotalPrice = purchaseData.PurchasePrice * (-purchaseData.PaymentQuantity);
                reverseData.TotalCost = (purchaseData.PurchasePrice - purchaseData.Rebate1 - purchaseData.Rebate2 -
                                         purchaseData.Rebate3) * (-purchaseData.PaymentQuantity);
                reverseData.Status = "已收货";
                reverseData.ShipmentType = "1途虎提货";
                reverseData.VendorDate = purchaseData.VendorDate;
                reverseData.VendorCode = purchaseData.VendorCode;
                reverseData.PlanedInstockDate = DateTime.Now;
                reverseData.WareHouse = purchaseData.WareHouse;
                reverseData.WareHouseID = purchaseData.WareHouseID;
                reverseData.AssetCost = purchaseData.AssetCost;
                reverseData.AccountPeriod = purchaseData.AccountPeriod;
                reverseData.Reverse = purchaseData.Reverse;
                reverseData.CreatedBy = purchaseData.CreatedBy;
                reverseData.CreatedDatetime = purchaseData.CreatedDatetime;
                reverseData.PaymentQuantity = purchaseData.PaymentQuantity;
                return View("CreatePurchaseReverse", reverseData);
            }
            return Json("只有新建或已发货且已付款的采购订单才能创建红冲！", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存红冲
        /// </summary>
        public string SavePurchaseReverse(int PKID, int PoId, string createdBy, DateTime CreatedDatetime,
            string WareHouse, int WareHouseID, int Reverse, string VendorName, int VendorId, string PID,
            string Name, int PaymentQuantity, decimal PurchasePrice, decimal TotalPrice, decimal BasePrice, decimal Rebate1,
            decimal Rebate2, decimal Rebate3, decimal CostPrice, decimal TotalCost, DateTime PlanedInstockDate,
            string ShipmentType, string AccountPeriod, decimal AssetCost, string Remark, decimal DiffPrice)
        {
            var model = new PurchaseOrderItemEntity();
            model.PKID = PKID;
            model.POId = PoId;
            model.CreatedBy = createdBy;
            model.CreatedDatetime = CreatedDatetime;
            model.WareHouse = WareHouse;
            model.WareHouseID = WareHouseID;
            model.Reverse = Reverse;
            model.VendorName = VendorName;
            model.VendorId = VendorId;
            model.PID = PID;
            model.Name = Name;
            model.Num = PaymentQuantity;
            model.PurchasePrice = PurchasePrice;
            model.TotalPrice = TotalPrice;
            model.BasePrice = BasePrice;
            model.Rebate1 = Rebate1;
            model.Rebate2 = Rebate2;
            model.Rebate3 = Rebate3;
            model.CostPrice = CostPrice;
            model.TotalCost = TotalCost;
            model.PlanedInstockDate = PlanedInstockDate;
            model.ShipmentType = ShipmentType;
            model.AccountPeriod = AccountPeriod;
            model.AssetCost = AssetCost;
            model.Remark = Remark;
            model.ShipmentType = "3店面自备";
            model.DiffPrice = DiffPrice;
            var result = new PurchaseManager().AddPurchaseOrderItem(model);
            return result ? "SUCCESS" : "FAIL";
        }
        /// <summary>
        /// 审核或者驳回红冲
        /// 2017/02/07
        /// </summary>
        public string AuditOrTurnDownReverseOrTurnedGoods(int purchaseId, string reason, string operateType, int relatedPoItemId)
        {
            var result = PurchaseManager.AuditReverseOrReturnGoods(purchaseId, operateType, relatedPoItemId, reason);
            var purchaseOrderItem = PurchaseManager.GetPurchaseOrderItemEntity(relatedPoItemId);
            //红冲采购订单审核通过时，还原供应商差价金额
            if (operateType == "AuditData" && purchaseOrderItem.DiffPrice != 0)
            {
                var purchaseReverse = PurchaseManager.GetPurchaseReverseByRelatedId(purchaseId);
                if (purchaseReverse != null)
                {
                    new AccountingManager().UpdateUsedAdjustMoney(purchaseOrderItem.VendorId,
                    purchaseOrderItem.DiffPrice * purchaseReverse.RelatedNum, ThreadIdentity.Operator.Name);
                    //purchaseManager.DeletePurchaseDeductionDetails(relatedPoItemId);//最好还是不要取消这张表的信息
                }
            }
            return result ? "SUCCESS" : "FAIL";
        }
        /// <summary>
        /// 采购单占用明细显示
        /// 2017/02/07
        /// </summary>
        public ActionResult ShowPoItemUsedDetail(int poItemId)
        {
            var model = PurchaseAlwaysOnReadManager.ShowPoItemUsedDetail(poItemId);
            if (model.Error)
            {
                return Json(model.Error, JsonRequestBehavior.AllowGet);
            }
            return View(model);
        }
        /// <summary>
        /// 获取物流信息
        /// 2017/02/07
        /// </summary>
        public ActionResult GetLogisticInformation(int venderId, int purchaseId)
        {
            var model = new LogisticsInformation
            {
                PurchaseId = purchaseId,
                VenderId = venderId
            };
            var list = new VenderManager().SelectLogisticInformation(model);
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 创建物流信息
        /// 2017/02/07
        /// </summary>
        public ActionResult CreateLogistic(string objJson, int venderId, int purchaseId)
        {
            try
            {
                var lgstic = new LogisticsInformation
                {
                    Updater = User.Identity.Name,
                    VenderId = venderId,
                    PurchaseId = purchaseId
                };
                new VenderManager().DeleteLogisticInformation(lgstic);
                if (objJson != null)
                {
                    var listJson = JsonConvert.DeserializeObject<List<LogisticsInformation>>(objJson);
                    if (listJson.Any())
                    {
                        foreach (var ranking in listJson)
                        {
                            var model = new LogisticsInformation();
                            model.CreateDate = DateTime.Now;
                            model.Creater = User.Identity.Name;
                            model.Updater = User.Identity.Name;
                            model.LogisticCode = ranking.LogisticCode;
                            model.LogisticCompany = ranking.LogisticCompany;
                            model.VenderId = venderId;
                            model.PurchaseId = purchaseId;
                            new VenderManager().AddLogisticInformation(model);
                        }
                        // new InviteConfigurationManager().EditPriceConfiguration(list);
                    }
                    return Json("保存成功");
                }

                return Json("保存失败");
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
        }
        /// <summary>
        /// 申请调价页面
        /// </summary>
        /// <returns></returns>
        public ActionResult MakeUpDiffPrice(decimal purchasePrice)
        {
            ViewBag.PurchasePrice = purchasePrice;
            return View("_MakeUpDifference");
        }
        /// <summary>
        /// 判断采购是否已经补过差价
        /// 2017/02/07
        /// </summary>
        public string IsMakedUpDiffPrice(int orderId, int itemId)
        {
            var result = new PurchaseManager().IsMakedUpDiffPrice(orderId, itemId);
            return result ? "true" : "false";
        }
        /// <summary>
        /// 添加采购补差价
        /// 2017/02/07
        /// </summary>
        public string PurchaseMakeUpDiffPrice(int orderId, int itemId, decimal diffPrice, string reason)
        {
            var poItem = PurchaseManager.GetPurchaseOrderItemEntity(itemId);
            if (!string.IsNullOrWhiteSpace(poItem.Status) && poItem.Status.Trim() == "已收货")
            {
                var result = new PurchaseManager().AddPurchaseDiffPrice(poItem, orderId, diffPrice, reason);
                return result ? "true" : "false";
            }
            return "只有已收货的采购订单才能调整价格！";
        }
        /// <summary>
        /// 采购相关任务列表显示
        /// 2017/02/08
        /// </summary>
        public ActionResult QuhuoTaskLink(int id, string type = "wuliu")
        {
            var dt = new PurchaseManager().PurchseToHouseTaskListById(User.Identity.Name, type, id);
            ViewBag.type = type;
            return View(dt);
        }
        /// <summary>
        /// 采购创建任务
        /// 2017/02/08
        /// </summary>
        public JsonResult CreateTask(int pkId, string category, string remark, string type)
        {
            string message = "";
            try
            {
                if (new PurchaseManager().CreatPurchseToHouseTask(pkId, category, remark, User.Identity.Name, type) > 0)
                    message = "创建成功！";
                else
                    message = "创建失败！可能有重复任务！请查看！";
            }
            catch (Exception ex)
            {
                message = ex.InnerException.Message;
            }
            return Json(message);
        }
        /// <summary>
        /// 查看历史操作详情
        /// 2017/02/08
        /// </summary>
        public ActionResult CompareDifferent(int oprLogId)
        {
            var log = new PurchaseNewManager().GetPurchaseMonitorLog(new PurchaseMonitorLog() { PKID = oprLogId });

            string[] BeforeValues = log.BeforeValue.Split('\r');
            string[] AfterValues = log.AfterValue.Split('\r');
            var before = new StringBuilder(log.BeforeValue);
            var after = new StringBuilder(log.AfterValue);

            if (BeforeValues.Count() == AfterValues.Count())
            {
                before = new StringBuilder();
                for (int i = 0; i < BeforeValues.Length; i++)
                {
                    if (BeforeValues[i].Trim() != AfterValues[i].Trim())
                        before.Append($"<span style='color:red;'>{BeforeValues[i]}</span>");
                    else
                        before.Append(BeforeValues[i]);
                }
            }
            else if (BeforeValues.Count() > AfterValues.Count() && AfterValues.Count() > 1)
            {
                before = new StringBuilder();
                for (int i = 0; i < BeforeValues.Length; i++)
                {
                    if (log.AfterValue.Replace(BeforeValues[i], "0") == log.AfterValue)
                        before.Append($"<span style='color:red;'>{BeforeValues[i]}</span>");
                    else
                        before.Append(BeforeValues[i]);
                }
            }
            else if (1 < BeforeValues.Count() && BeforeValues.Count() < AfterValues.Count())
            {
                after = new StringBuilder();
                for (int i = 0; i < AfterValues.Length; i++)
                {
                    if (log.BeforeValue.Replace(AfterValues[i], "0") == log.BeforeValue)
                        after.Append($"<span style='color:red;'>{AfterValues[i]}</span>");
                    else
                        after.Append(AfterValues[i]);
                }
            }
            log.BeforeValue = before.ToString();
            log.AfterValue = after.ToString();
            ViewBag.BeforeMvcString = MvcHtmlString.Create(Regex.Replace(GetPRCString(log.BeforeValue), "\r?\n", "<br />"));
            ViewBag.AfterMvcString = MvcHtmlString.Create(Regex.Replace(GetPRCString(log.AfterValue), "\r?\n", "<br />"));
            return View(log);
        }
        /// <summary>
        /// 获取调价驳回原因
        /// 2017/02/08
        /// </summary>
        public string GetPurchaseDiffPriceRejectReason(int orderId, int itemId)
        {
            var reason = new PurchaseManager().SelectRejectReason(orderId, itemId);
            return reason;
        }
        /// <summary>
        /// 判断凭证是否在新表中
        /// 2017/02/08
        /// </summary>
        public string CheckPurchasePaymentVoucherExist(int purchaseId, string billNo, int flag)
        {
            bool isExist = false;
            if (flag == 1)
            {
                isExist = new PurchaseManager().CheckPurchasePaymentVoucherExist(billNo, -1, 1);
            }
            else
            {
                isExist = new PurchaseManager().CheckPurchasePaymentVoucherExist(string.Empty, purchaseId, 2);

            }

            return isExist ? "Yes" : "No";
            // new PurchaseManager().CheckPurchasePaymentVoucherExist()
        }
        /// <summary>
        /// 从新表中获取付款凭证(PurchasePaymentVoucherInfo)
        /// 2017/02/08
        /// </summary>
        public FileResult GetVoucherImgFromByteTable(string purchaseId)
        {
            List<byte[]> fileByteList = new PurchaseManager().GetPurchasePaymentVoucherImage(string.Empty, Convert.ToInt32(purchaseId), 2);

            string imageFormat = string.Empty;
            string imageType = string.Empty;
            if (fileByteList != null && fileByteList.Count > 0)
            {
                MemoryStream buf = new MemoryStream(fileByteList[0]);
                System.Drawing.Image image = System.Drawing.Image.FromStream(buf, true);

                if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                {
                    imageFormat = "image/jpeg";
                    imageType = "jpeg";
                }
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                {
                    imageFormat = "image/gif";
                    imageType = "gif";
                }
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                {
                    imageFormat = "image/png";
                    imageType = "png";
                }
            }
            return new FileContentResult(fileByteList[0], imageFormat);
        }
        /// <summary>
        /// 从新表中下载付款凭证(PurchasePaymentVoucherInfo)
        /// 2017/02/08
        /// </summary>
        //[ErrorMessageSettingHandle]
        public void DownloadVoucherFromByteTable(string purchaseId)
        {
            //需要判断DB中是否存在新的记录

            bool isExist = new PurchaseManager().CheckPurchasePaymentVoucherExist(string.Empty, Convert.ToInt32(purchaseId), 2);

            if (isExist)
            {
                List<byte[]> fileByteList = new PurchaseManager().GetPurchasePaymentVoucherImage(string.Empty, Convert.ToInt32(purchaseId), 2);

                string imageFormat = string.Empty;
                string imageType = string.Empty;
                if (fileByteList != null && fileByteList.Count > 0)
                {
                    MemoryStream buf = new MemoryStream(fileByteList[0]);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(buf, true);

                    if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                    {
                        imageFormat = "image/jpeg";
                        imageType = "jpeg";
                    }
                    else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    {
                        imageFormat = "image/gif";
                        imageType = "gif";
                    }
                    else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    {
                        imageFormat = "image/png";
                        imageType = "png";
                    }
                }

                Response.Clear();
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                Response.ContentType = imageFormat;

                Response.AddHeader("Content-Disposition",
                    "attachment; filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + imageType);
                Response.BinaryWrite(fileByteList[0]);
                Response.End();
            }
            else
            {
                var data = new PurchaseManager().FetchVoucherIamgesUrlByPurchaseId(purchaseId);
                var voucherUrls = data.Split(',');

                var filePath = voucherUrls[0];
                if (string.IsNullOrWhiteSpace(filePath)) return;
                filePath = FileUrlConfig.VoucherDownloadUrl + filePath;
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    Response.Clear();
                    Response.Charset = "UTF-8";
                    Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                    Response.ContentType = "image/gif";

                    Response.AddHeader("Content-Disposition",
                        "attachment; filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpeg");
                    Response.BinaryWrite(bytes);
                    Response.End();
                }
            }
            new PurchaseManager().UpdateVoucherDownloadsByPurchaseId(purchaseId);
        }
        /// <summary>
        /// 从旧表中获取付款凭证(PurchaseApplyPayment)
        /// 2017/02/08
        /// </summary>
        public string GetVoucherImg(string purchaseId)
        {
            var data = new PurchaseManager().FetchVoucherIamgesUrlByPurchaseId(purchaseId);
            var voucherUrls = data.Split(',');
            var filePath = voucherUrls[0];
            return filePath;
        }
        /// <summary>
        /// 从旧表中下载付款凭证(PurchaseApplyPayment)
        /// 2017/02/08
        /// </summary>
        //[ErrorMessageSettingHandle]
        public void DownloadVoucher(string purchaseId)
        {
            var data = new PurchaseManager().FetchVoucherIamgesUrlByPurchaseId(purchaseId);
            var voucherUrls = data.Split(',');

            var filePath = voucherUrls[0];
            if (string.IsNullOrWhiteSpace(filePath)) return;
            filePath = FileUrlConfig.VoucherDownloadUrl + filePath;
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                var bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.Clear();
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                Response.ContentType = "image/gif";

                Response.AddHeader("Content-Disposition",
                    "attachment; filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpeg");
                Response.BinaryWrite(bytes);
                Response.End();
            }

            new PurchaseManager().UpdateVoucherDownloadsByPurchaseId(purchaseId);
        }
        /// <summary>
        /// 确认运费耗材
        /// 2017/02/08
        /// </summary>
        public ActionResult ConfirmationFreight(int poItemId)
        {
            var model = new PurchaseManager().GetPurchaseOrderByPurchaseId(poItemId);
            if (model.Status.StartsWith("新建") &&
                (model.PID == "T-TransportSupplies|1" || model.PID == "TS-TransportSupplies|1"))
            {
                model.Status = PurchaseOrderStatus.Received;
                model.InstockNum = model.Num;
                model.CostTotalPrice = model.Num * model.PurchasePrice;
                model.InstockTotalPrice = model.Num * model.PurchasePrice;
                new PurchaseManager().UpdatePurchaseInforByPkid(model);
                new AccountingManager().CreatePurchasePaymentVoucher(poItemId, ThreadIdentity.Operator.Name);   //id 采购产品单号   userno操作账号
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 结束收货
        /// 2017/02/08
        /// </summary>
        public ActionResult FinishedGoodsReceipt(int poItemId)
        {
            var poItem = new PurchaseManager().GetPurchaseOrderItemEntity(poItemId);
            if (!string.IsNullOrWhiteSpace(poItem.Status) && poItem.Status.Trim() == "部分收货")
            {
                var whetherFinishedGoodsReceipt = PurchaseManager.FinishedGoodsReceipt(poItemId);
                if (whetherFinishedGoodsReceipt)
                {
                    SaveOprLog("PoItem", poItemId, typeof(Models.PurchaseOrderItem), "结束收货", null);
                }
                return Json(whetherFinishedGoodsReceipt, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("只有部分收货的采购订单才能结束收货，请刷新页面！", JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 判断是否可创建退货
        /// 2017/02/08
        /// </summary>
        public string IsCanReturnGoods(int purchaseId)
        {
            var returnValue = "";
            var poItem = PurchaseManager.GetPurchaseOrderItemEntity(purchaseId);
            if (!string.IsNullOrWhiteSpace(poItem.Status) && poItem.Status.Trim() == "已收货")
            {
                var result = PurchaseManager.IsCanReturnGoods(purchaseId);
                returnValue = result > 0 ? "FALSE" : "TRUE";
            }
            else
            {
                returnValue = "已收货的采购单才能进行退货，请刷新页面！";
            }
            return returnValue;
        }
        /// <summary>
        /// 申请退货显示
        /// 2017/02/08
        /// </summary>
        public ActionResult GetPurchaseReturnGoods(int purchaseId)
        {
            var reverseData = new PurchaseManager().SelectReturnPurchaseOrderWithStockLocation(purchaseId);

            ViewBag.OriginalPurchaseOrderItemId = purchaseId;
            return View("PoReturnGoodsList", reverseData);
        }
        /// <summary>
        /// 采购请款(采购订单列表)
        /// 2017/02/08
        /// </summary>
        public int SaveApplyPayment(string purchaseOrderList, string data, decimal storedMoney = 0)
        {
            var dataList = JsonConvert.DeserializeObject<List<PurchaseApplyPayment>>(data);
            var purchaseOrders = JsonConvert.DeserializeObject<List<ThBiz.DataAccess.Entity.PurchaseOrder>>(purchaseOrderList);
            var returnValue = PurchaseManager.AddPurchaseAppyPayment(purchaseOrders, dataList, storedMoney);
            return returnValue;
        }
        /// <summary>
        /// 判断请款的采购单是否属于同一供应商
        /// 2017/02/08
        /// </summary>
        public string IsNotOneVendor(string purchaseId)
        {
            var result = new PurchaseManager().IsCanApplyPayment(purchaseId);
            return result ? "YES" : "NO";
        }
        /// <summary>
        /// 获取准备请款的采购单信息
        /// 2017/02/08
        /// </summary>
        public ActionResult ApplyPaymentPurchaseOrder(string id)
        {
            id = id.TrimEnd(',');
            var purchaseOrders = PurchaseManager.ApplyPaymentPurchaseOrder(id).Distinct();
            return View(purchaseOrders);
        }
        /// <summary>
        /// 获取需要请款的数据
        /// </summary>
        public ActionResult ApplyPayment(string purchaseOrderList, string id)
        {
            var applyList = new List<PurchaseOrderItemEntity>();
            var accountList = new List<PurchaseOrderItemEntity>();
            id = id.TrimEnd(',');
            var purchaseOrders = JsonConvert.DeserializeObject<List<ThBiz.DataAccess.Entity.PurchaseOrder>>(purchaseOrderList);
            var purchaseListData = new PurchaseManager().SelectApplyPaymentData(id);

            if (purchaseOrders != null && purchaseOrders.Any())
            {
                foreach (var data in purchaseListData)
                {
                    var applyData = new PurchaseOrderItemEntity();
                    var accountData = new PurchaseOrderItemEntity();
                    applyData.PKID = data.PKID;
                    applyData.Name = data.Name;
                    applyData.PurchasePrice = data.PurchasePrice;
                    applyData.PName = data.PName;
                    applyData.AccountPeriod = data.AccountPeriod;
                    applyData.Status = data.Status;
                    applyData.InstockNum = data.InstockNum;
                    applyData.TaxPoint = data.TaxPoint;
                    applyData.DataType = data.DataType;
                    applyData.TempName = data.TempName;
                    applyData.VendorId = data.VendorId;
                    applyData.VendorName = data.VendorName;
                    applyData.vVendorId = data.vVendorId;
                    applyData.vVendorName = data.vVendorName;
                    if (data.Num > 0)
                    {
                        var purchaseOrder = purchaseOrders.FirstOrDefault(_ => _.PKID == data.PKID);
                        if (purchaseOrder != null)
                        {
                            applyData.Num = purchaseOrder.Num;
                            applyData.TotalPrice = purchaseOrder.Num * (data.PurchasePrice + data.DiffPrice);
                            applyList.Add(applyData);
                            accountData.Payee = data.Payee;
                            accountData.Bank = data.Bank;
                            accountData.Account = data.Account;
                            accountData.VendorId = data.VendorId;
                            accountList.Add(accountData);
                        }
                    }
                    else
                    {
                        applyData.Num = data.Num;
                        applyData.TotalPrice = data.TotalPrice;
                        applyList.Add(applyData);
                        accountData.Payee = data.Payee;
                        accountData.Bank = data.Bank;
                        accountData.Account = data.Account;
                        accountData.VendorId = data.VendorId;
                        accountList.Add(accountData);
                    }
                }
            }

            var vendorId = 1;
            if (purchaseListData.Any())
            {
                vendorId = purchaseListData.First().VendorId;
            }
            var dataList = applyList.Distinct(x => x.PKID).ToList();
            var accDataList = accountList.Distinct(a => a.Account).ToList();
            ViewBag.DtVendorName = new PurchaseManager().SelectApplyPaymentData_VendorName(vendorId);
            ViewBag.PurchaseListData = dataList;
            ViewBag.AccountData = accDataList;
            ViewBag.TaxRebate = new PurchaseManager().GetAvailableRebateBalance(purchaseListData.FirstOrDefault().VendorName);

            return View();
        }
        #endregion
        #region 私有方法
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
        /// <summary>
        /// 添加操作日志
        /// 2017/02/07
        /// </summary>
        private void SaveOprLog(string objType, int objId, Type otype, string operation, object beforeValue, object afterValue = null)
        {
            var rol = new ThBiz.DataAccess.Entity.OprLog { Author = ThreadIdentity.Operator.Name };
            var dcs = new DataContractSerializer(otype);
            if (beforeValue != null)
            {
                var builder = new StringBuilder();
                foreach (var property in beforeValue.GetType().GetProperties())
                {
                    object value = property.GetValue(beforeValue, null);

                    builder.Append(property.Name)
                        .Append(" = ")
                        .Append((value ?? "null"))
                        .AppendLine();
                }
                rol.BeforeValue = builder.ToString();
            }
            if (afterValue != null)
            {
                var builder = new StringBuilder();
                foreach (var property in afterValue.GetType().GetProperties())
                {
                    object value = property.GetValue(afterValue, null);

                    builder.Append(property.Name)
                        .Append(" = ")
                        .Append((value ?? "null"))
                        .AppendLine();
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
        /// <summary>
        /// 释放销售单
        /// 2017/02/07
        /// </summary>
        private void RemoveSoPoWhenEditArrivedTime(int id, out int soItmeId)
        {
            try
            {
                var sp = new PurchaseManager().GetSoPoList(-1, id).SingleOrDefault();

                soItmeId = sp.SoItemId;

                new PurchaseManager().DeleteSoPo(sp.PKID);

                UpdateCost(soItmeId, User.Identity.Name);
            }
            catch (Exception ex)
            {
                soItmeId = 0;
                Logger.Log(Level.Error, ex,
                    "Error in RemoveSoPo,id:" + id);
            }
        }
        /// <summary>
        /// 更新成本
        /// 2017/02/07
        /// </summary>
        public void UpdateCost(int soItemId, string email = "System")
        {
            try
            {
                new OrderManager().RefreshCost(soItemId);
                PurchaseManager.UpdatePurchaseTaskStatusByOrderListId(soItemId);
            }
            catch (TuhuBizException ex)
            {
                ExceptionMonitor.AddNewMonitor("OrderList", soItemId.ToString(), ex.Message, "",
                    "UpdateCost");
                Logger.Log(Level.Info, ex,
                    "Not exists or deleted,soitemid:" + soItemId);
            }
            catch (Exception ex)
            {
                ExceptionMonitor.AddNewMonitor("OrderList", soItemId.ToString(), ex.Message, "",
                    "UpdateCost");

                Logger.Log(Level.Error, ex,
                    "Error in UpdateCost,soitemid:" + soItemId);
            }
        }
        /// <summary>
        /// 获取PRC内容
        /// 2017/02/08
        /// </summary>
        public string GetPRCString(string enString)
        {
            var purchaseOrderItemDic = new PurchaseMonitorLog().GetPurchaseOrderItemDic();

            foreach (var item in purchaseOrderItemDic)
            {
                //做一个妥协 判断
                if (item.Key == "PKID")
                {
                    enString = enString.Replace("PKID", "产品单号");
                }
                else
                {
                    enString = enString.Replace("\n" + item.Key + " = ",
                        "\n" + (string.IsNullOrWhiteSpace(item.Value) ? item.Key : item.Value) + " = ");
                }
            }
            return enString;
        }

        #endregion
    }
}