using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ThBiz.Business.EmployeeManagement;
using ThBiz.Business.Monitor;
using ThBiz.Business.Purchase;
using ThBiz.Common.Common;
using ThBiz.Common.Entity;
using ThBiz.DataAccess.Entity;
using TheBiz.Common.Common;
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
        /// 获取IP地址
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