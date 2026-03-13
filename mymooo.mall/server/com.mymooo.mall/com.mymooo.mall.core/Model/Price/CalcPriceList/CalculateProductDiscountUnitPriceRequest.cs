using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Quotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{

    public class CalculateProductDiscountUnitPriceRequest
    {
        public string CompanyCode { get; set; } = string.Empty;

        public long CategoryId { get; set; }

        public string ParentCompanyCode { get; set; } = string.Empty;

		public decimal SalesUnitPrice { get; set; }

        public decimal FirstCost { get; set; }
        public long ProductId { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        public long TypeId { get; set; }

        public long? CustomerId { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        public PriceListDataType PriceListDataType { get; set; }

        public decimal DeliveryDays { get; set; }

        public List<StockPlatEntity>? Stock { get; set; }

    }
}
