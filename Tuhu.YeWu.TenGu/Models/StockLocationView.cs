using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MVCControlsToolkit.DataAnnotations;

namespace Tuhu.YeWu.TenGu.Models
{
    [MetadataType(typeof(MetaStockLocation))]
    public class StockLocationView
    {
        public int PKID { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public string PID { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
        public int BatchId { get; set; }
        public int AvailableNum { get; set; }

        public decimal CostPrice { set; get; }
        public decimal TotalCost { set; get; }
        public string WeekYear { get; set; }
        public string Remark { get; set; }

        [DateRange(SMinimum = "Today-7d", SMaximum = "Today")]
        public DateTime UpdatedTime { set; get; }
        //上海库存数量
        public int numShangHai { get; set; }
        //北京库存数量
        public int numBieJing { get; set; }
        //广州库存数量
        public int numGuangZhou { get; set; }

        //官网价格
        public decimal Prices { get; set; }

        //提成都
        public double Commission { get; set; }

        //开始时间
        public DateTime? StartTime { get; set; }

        //结束时间
        public DateTime? EndTime { get; set; }

        //保存各种仓库的数量可用数量
        public Dictionary<string, int> DicWareHouseNum { get; set; }

        public int PoId { get; set; }
    }
}
