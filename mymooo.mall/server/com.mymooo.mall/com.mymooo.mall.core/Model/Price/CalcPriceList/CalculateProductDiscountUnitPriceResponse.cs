using com.mymooo.mall.core.Model.Quotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    public class CalculateProductDiscountUnitPriceResponse
    {
        public CalculateProductDiscountUnitPriceResponse()
        {
            this.DiscountRate = 100;
        }
        public decimal DiscountRate { get; set; }
        public decimal PreferentialUnitPrice { get; set; }

        public decimal QtyDiscount { get; set; }

        public decimal OriginalUnitPrice { get; set; }

        public bool IsHistory { get; set; }
        public bool IsPriceList { get; set; }
        public decimal DeliveryDays { get; set; }

        public PriceSource? PriceSource { get; set; }


        public DeliverySource? DeliverySource { get; set; }
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
