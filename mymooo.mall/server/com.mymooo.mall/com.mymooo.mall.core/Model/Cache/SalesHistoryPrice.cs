using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Attributes.Redis;

namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 销售历史价缓存
	/// </summary>
	[RedisKey("mymooo-product-number", 1)]
	public class SalesHistoryPrice
	{
		private string productNumber = string.Empty;

		/// <summary>
		/// 公司编码
		/// </summary>
        [RedisMainField]
        public string CompanyCode { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		[RedisMainField(2)]
		public PriceSource PriceSource { get; } = PriceSource.history;

		/// <summary>
		/// 
		/// </summary>
        public DeliverySource DeliverySource { get; set; } 

		/// <summary>
		/// 索引key
		/// </summary>
		[RedisPrimaryKey]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// 产品型号
		/// </summary>
		public string ProductNumber
		{
			get => productNumber;
			set
			{
				productNumber = value;
				this.Id = productNumber.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 审核时间
		/// </summary>
		public DateTime AuditTime { get; set; }

		/// <summary>
		/// 产品小类Id
		/// </summary>
		public long SmallId { get; set; }

		/// <summary>
		/// 产品小类编码
		/// </summary>
		public string SmallCode { get; set; } = string.Empty;

		/// <summary>
		/// 产品小类名称
		/// </summary>
		public string SmallName { get; set; } = string.Empty;

		/// <summary>
		/// 是否FA产品
		/// </summary>
		public bool IsFa {  get; set; }

		/// <summary>
		/// 销售单号
		/// </summary>
		public string SalesOrderNo { get; set; } = string.Empty;

		/// <summary>
		/// 产品Id
		/// </summary>
		public long ProductId { get; set; }

		/// <summary>
		/// 产品类型
		/// </summary>
		public long ProductTypeId { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
        public decimal Qty { get; set; }

		/// <summary>
		/// 原价
		/// </summary>
        public decimal OriginalPrice { get; set; }

		/// <summary>
		/// 含税单价
		/// </summary>
		public decimal TaxPrice { get; set; }

		/// <summary>
		/// 发货天数
		/// </summary>
		public int DeliveDays { get; set; }

		/// <summary>
		/// 等级折扣
		/// </summary>
		public decimal LevelDiscount { get; set; }

		/// <summary>
		/// 数量折扣
		/// </summary>
		public decimal QtyDiscount { get; set; }

		/// <summary>
		/// 公司等级
		/// </summary>
        public string CompanyLevel { get; set; } = string.Empty;

		/// <summary>
		/// 公司名称
		/// </summary>
        public string CompanyName { get; set; } = string.Empty;
    }



}
