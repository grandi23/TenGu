using System;
using System.Collections.Generic;
using ThBiz.DataAccess.Entity;

namespace Tuhu.YeWu.TenGu.Models
{
    public class PurchaseOccupyModel
    {
        public int OrderId { get; set; }
        public string SoNo { get; set; }
        public string OrderStatus { get; set; }
        public string OrderChannel { get; set; }
        public int OrderPurchaseStatus { get; set; }
        public int InstallShopId { get; set; }
        public int OrderlistId { get; set; }
        public string Pid { get; set; }
        public string ProductName { get; set; }
        public int ProductType { get; set; }
        public string SoItemPurchaseStatus { get; set; }
        public int SoItemNum { get; set; }
        //订单配送状态
        public string DeliveryStatus { get; set; }
        //订单仓库名称
        public string WareHouseName { get; set; }
        //订单仓库ID
        public int WareHouseId { get; set; }
        public string WeekYear { get; set; }
        public IEnumerable<PurchaseSoStock> BookedStockList { get; set; }
        public IEnumerable<PurchaseSoPo> BookedPoItemList { get; set; }
        public List<StockLocationView> StockLocationList { get; set; }
        //仓库列表
        public List<BizShops> WareHouseList { get; set; }

        //不受更改仓库限制的仓库列表
        public List<BizShops> NoLimitedWareHouseList { get; set; }

        //仓库对应的颜色
        public string WareHouseColor { get; set; }
        public IEnumerable<SoTransferOrderList> SoTransferOrderList { get; set; }

    }

    public class PurchaseSoStock
    {
        public int SoStockId { get; set; }
        public int PoItemId { get; set; }
        public string VendorName { get; set; }
        public int SoId { get; set; }
        public int SoItemId { get; set; }
        public int StockId { get; set; }
        public int Num { get; set; }
        public string Pid { get; set; }
        public int BatchId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public string Location { set; get; }
        public decimal CostPrice { set; get; }
        public string Status { set; get; }
        public DateTime UpdatedDatetime { set; get; }
        public string ExpressCompany { set; get; }
        public decimal FirstWeightFee { set; get; }
        public decimal FirstWeightKg { set; get; }
        public decimal ContinueWeight { set; get; }
        public string Duration { set; get; }
        public int TotalCount { get; set; }
        public decimal DeliveryFee => PublicFunction.ComputeDeliveryFee(FirstWeightFee, FirstWeightKg, ContinueWeight, TotalCount);
        public string WeekYear { get; set; }
        public int OwnerId { get; set; }
    }

    public class PurchaseSoPo
    {
        public int SoPoId { get; set; }
        public int SoId { get; set; }
        public int SoItemId { get; set; }
        public int PoItemId { get; set; }
        public int PoId { get; set; }
        public string PoStatus { get; set; }
        public int Num { get; set; }
        public string Pid { get; set; }
        public string Name { get; set; }
        public string VendorName { get; set; }
        public int Number { get; set; }
        public decimal PurchasePrice { set; get; }
        public decimal CostPrice { set; get; }
        public string Remark { set; get; }
        public string Status { set; get; }
        public DateTime UpdatedDatetime { set; get; }
        public string WareHouse { set; get; }
        public DateTime PlanedInStockDate { set; get; }
        public string ExpressCompany { set; get; }
        public decimal FirstWeightFee { set; get; }
        public decimal FirstWeightKg { set; get; }
        public decimal ContinueWeight { set; get; }
        public string Duration { set; get; }
        public int TotalCount { get; set; }
        public decimal DeliveryFee { get { return PublicFunction.ComputeDeliveryFee(FirstWeightFee, FirstWeightKg, ContinueWeight, TotalCount); } }
    }

    public class SoTransferOrderList
    {
        public string WareHouseName { get; set; }
        public int TaskProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int OccupationQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TransferFee { get; set; }
        public DateTime? ArrivalTime { get; set; }
    }
}