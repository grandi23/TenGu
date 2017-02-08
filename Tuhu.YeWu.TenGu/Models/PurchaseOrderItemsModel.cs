using System;
using Tuhu.Component.ExportImport;

namespace Tuhu.YeWu.TenGu.Models
{
    [TableExportImport(ExportImportName = "采购订单列表")]
    public class PurchaseOrderItems
    {
        [ColumnExportImport(ExportImportName = "采购单号")]
        public int PKID { get; set; }
        [ColumnExportImport(ExportImportName = "所属仓库")]
        public string WareHouse { set; get; }
        public int VendorId { get; set; }
        [ColumnExportImport(ExportImportName = "供应商")]
        public string VendorName { get; set; }
        [ColumnExportImport(ExportImportName = "产品名称")]
        public string Name { get; set; }
        public int POId { get; set; }
        [ColumnExportImport(ExportImportName = "产品编号")]
        public string PID { get; set; }
        [ColumnExportImport(ExportImportName = "采购数量")]
        public int Num { get; set; }
        [ColumnExportImport(ExportImportName = "已入库")]
        public int? InstockNum { get; set; }
        [ColumnExportImport(ExportImportName = "采购单价")]
        public decimal PurchasePrice { get; set; }
        [ColumnExportImport(ExportImportName = "采购总价")]
        public decimal? TotalPrice { get; set; }
        [ColumnExportImport(ExportImportName = "入库总价")]
        public decimal InstockTotalPrice { get; set; }
        [ColumnExportImport(ExportImportName = "返利1")]
        public decimal? Rebate1 { set; get; }
        [ColumnExportImport(ExportImportName = "返利2")]
        public decimal? Rebate2 { set; get; }
        [ColumnExportImport(ExportImportName = "返利3")]
        public decimal? Rebate3 { set; get; }
        public string Status { get; set; }

        [ColumnExportImport(ExportImportName = "抵扣单价")]
        /// <summary>
        /// 抵扣单价
        /// </summary>
        public decimal DiffPrice { get; set; }
        
        
        [ColumnExportImport(ExportImportName = "成本单价")]
        public decimal CostPrice { get; set; }
        [ColumnExportImport(ExportImportName = "入库成本总价")]
        public decimal CostTotalPrice { get; set; }
        public decimal? TotalCost { get; set; }
        public bool Invoice { set; get; }
        public string CreatedBy { set; get; }
        [ColumnExportImport(ExportImportName = "创建时间")]
        public DateTime CreatedDatetime { set; get; }
        
        public DateTime? Rebate1PlanDate { set; get; }
        
        public DateTime? Rebate2PlanDate { set; get; }
        
        public DateTime? Rebate3PlanDate { set; get; }
        [ColumnExportImport(ExportImportName = "提货方式")]
        public string ShipmentType { set; get; }
        [ColumnExportImport(ExportImportName = "计划入库时间")]
        public DateTime PlanedInstockDate { set; get; }
        public DateTime? InstockDate { set; get; }
        [ColumnExportImport(ExportImportName = "备注")]
        public string Remark { set; get; }
        public bool? HasVoucher { set; get; }
        public DateTime? VendorDate { set; get; }
        public string VendorCode { set; get; }
        public int WareHouseID { set; get; }
        public string AccountPeriod { set; get; }
        public string DriverName { set; get; }
        public decimal? AssetCost { set; get; }
        
        public DateTime? UpdateTime { set; get; }
        public string UpdatedBy { set; get; }
        public int? OrderId { set; get; }
        public int? TotalCount { set; get; }
        public decimal BasePrice { set; get; }

        public int MergeId { get; set; }
        /// <summary>
        /// 是否收到发票
        /// </summary>
        public bool IsReceiptInvoice { get; set; }

        
        
        public string PayStatus { get; set; }
        public string BillNo { get; set; }
        public bool IsAudited { get; set; }
        public int AuditStatus { get; set; }
        public int Refusal { get; set; }
        public bool ApplyPayStatus { get; set; }
        public string VoucherUrl { get; set; }
        public int Downloads { get; set; }
        public bool IsTurnDown { get; set; }
        public bool IsAuditingReverse { get; set; }
        public int Reverse { get; set; }
        public string DataType { get; set; }
        public bool IsApplyRejected { get; set; }
        public string ApplyRejectionReason { get; set; }
        /// <summary>
        /// 税点
        /// </summary>
        public decimal TaxPoint { get; set; }
        public string OriginalCreateUser { get; set; }
        /// <summary>
        /// 入库后移库到指定仓库
        /// </summary>
        public string TransferToWareHouse { get; set; }
        [ColumnExportImport(ExportImportName = "创建人")]
        public string CreatedByName { get; set; }

        public bool IsFromVenderSys { get; set; }


        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliveryTime { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string LogisticCode { get; set; }

        /// <summary>
        /// 物流公司
        /// </summary>
        public string LogisticCompany { get; set; }

        /// <summary>
        /// 物流联系方式
        /// </summary>
        public string LogisticTelNum { get; set; }

        public string PurchaseRemark { get; set; }

        public int PurchaseMode { get; set; }

        public int BizPaymentDays { get; set; }

        public int BulkPaymentDays { get; set; }
        /// <summary>
        /// 运费单价
        /// 2016/12/05
        /// </summary>
        public decimal FreightAmount { get; set; }
        /// <summary>
        /// 去税成本
        /// </summary>
        public decimal NoTaxCost { get; set; }
    }
}