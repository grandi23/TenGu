using System.ComponentModel.DataAnnotations;
using MVCControlsToolkit.DataAnnotations;

namespace Tuhu.YeWu.TenGu.Models
{
    [MetadataType(typeof(MetaStockLocation))]
    public partial class StockLocation
    {
    }
    public class MetaStockLocation
    {

        [CanSort, Display(Name = "库存编号")]
        public object PKID { get; set; }
        [Required, CanSort, Display(Name = "仓库编号")]
        public object LocationId { get; set; }
        [Required, CanSort, Display(Name = "仓库")]
        public object Location { get; set; }
        [Required, CanSort, Display(Name = "产品编号")]
        public object PID { get; set; }

        [Required, CanSort, Display(Name = "品名")]
        public object Name { get; set; }
        [Required, CanSort, Display(Name = "库存数量")]
        public object Num { get; set; }

        [Required, CanSort, Display(Name = "批次")]
        public object BatchId { get; set; }
        [Required, Display(Name = "成本单价")]
        public object CostPrice { get; set; }
        [Required, Display(Name = "成本总价")]
        public object TotalCost { get; set; }
        [Display(Name = "生产日期")]
        public object WeekYear { get; set; }
        [Display(Name = "备注")]
        public object Remark { get; set; }
        //[Display(Name = "可用数量")]
        //public object AvailableNum { get; set; }

        [CanSort, Display(Name = "入库日期"), Format(DataFormatString = "{0:D}")]
        public object UpdatedTime { get; set; }

    }

}