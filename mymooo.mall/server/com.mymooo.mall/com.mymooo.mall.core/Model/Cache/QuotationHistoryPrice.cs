using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Cache
{

    [RedisKey("mymooo-product-number", 1)]
    public class QuotationHistoryPrice
    {
        private string productNumber = string.Empty;

        [RedisMainField]
        public string CompanyCode { get; set; } = string.Empty;
        [RedisMainField(2)]
        public PriceSource PriceSource { get; set; } 
        public DeliverySource DeliverySource { get; set; }
        [RedisPrimaryKey]
        public string Id { get; set; } = string.Empty;
        public string ProductNumber
        {
            get => productNumber;
            set
            {
                productNumber = value;
                this.Id = productNumber.Replace("-", "").Trim().ToLower();
            }
        }
        public string CompanyLevel { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime AuditTime { get; set; }

        public string InquiryNo { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public decimal QuotationTaxPrice { get; set; }
        public int DeliveDays { get; set; }
        public decimal Qty { get; set; }
    }


}
