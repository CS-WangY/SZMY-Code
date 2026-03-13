using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Attributes.Redis;

namespace com.mymooo.mall.core.Model.Cache
{
	[RedisKey("mymooo-product-number", 1)]
	public class SalesFullHistoryPrice
    {
		private string productNumber = string.Empty;

		
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
		public DateTime AuditTime { get; set; }
		public long SmallId { get; set; }
		public string SmallCode { get; set; } = string.Empty;
		public string SmallName { get; set; } = string.Empty;
		public string SalesOrderNo { get; set; } = string.Empty;
		public long ProductId { get; set; }
		public long ProductTypeId { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal TaxPrice { get; set; }
		public int DeliveDays { get; set; }
		public decimal LevelDiscount { get; set; }
		public decimal QtyDiscount { get; set; }
        public decimal Qty { get; set; }
        public string CompanyLevel { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }


	public class PrdCodePrice
	{
		public string PrdCode { get; set; } = string.Empty;

		public decimal Price { get; set; }
	}
}
