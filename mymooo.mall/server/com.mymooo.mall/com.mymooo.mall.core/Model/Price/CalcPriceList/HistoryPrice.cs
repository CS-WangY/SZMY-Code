using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryPrice
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal DiscountRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal QtyDiscount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal OriginalUnitPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal DeliveryDays { get; set; }



        public bool HasHistoryPrice { get; set; }
    }
    public class HistoryLowPrice
    {
        public string ProductCode { get; set; } = string.Empty;
        /// <summary>
        /// 历史最低报价(最近一年)
        /// </summary>
        public decimal? QuoLPrice { get; set; }
        /// <summary>
        /// 历史最低成交价(最近一年)
        /// </summary>
        public decimal? DealLPrice { get; set; }
    }
}
