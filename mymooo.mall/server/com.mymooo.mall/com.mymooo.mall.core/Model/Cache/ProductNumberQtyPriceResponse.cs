using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 产品型号价格缓存
	/// </summary>
	[RedisKey("mymooo-product-number-reloadcache", 1)]
	public class ProductNumberQtyPriceResponse
	{
		private string productNumber = string.Empty;
		private string mymoooProductNumber = string.Empty;

		/// <summary>
		/// 蚂蚁型号Id
		/// </summary>
		public string MymoooProductNumberId { get; private set; } = string.Empty;

		/// <summary>
		/// 产品Id
		/// </summary>
		public long ProductId { get; set; }

		/// <summary>
		/// 产品型号
		/// </summary>
		public string ProductNumber
		{
			get => productNumber;
			set
			{
				productNumber = value;
				this.Id = value.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[JsonIgnore]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// 产品名称
		/// </summary>
		public string ProductName { get; set; } = string.Empty;

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
		/// 客户编码
		/// </summary>
		public string CompanyCode { get; set; } = string.Empty;

		/// <summary>
		/// 数量
		/// </summary>
		public decimal Qty { get; set; }

		/// <summary>
		/// 价格来源
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PriceSource PriceSource { get; set; } = PriceSource.none;

		/// <summary>
		/// 源单价
		/// </summary>
		public decimal OrgPrice { get; set; } = -1;

		/// <summary>
		/// 数量折扣
		/// </summary>
		public decimal QtyDiscount { get; set; } = 100;

		/// <summary>
		/// 等级折扣
		/// </summary>
		public decimal LevelDiscount { get; set; } = 100;

		/// <summary>
		/// 蚂蚁产品型号
		/// </summary>
		public string MymoooProductNumber { get => mymoooProductNumber; set
			{
				mymoooProductNumber = value;
				this.MymoooProductNumberId = value.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 蚂蚁产品名称
		/// </summary>
		public string MymoooProductName { get; set; } = string.Empty;

		/// <summary>
		/// 销售单价
		/// </summary>
		public decimal SalesPrice { get; set; } = -1;

		/// <summary>
		/// 产品工程师Id
		/// </summary>
		public long? ProductEngineerId { get; set; }

		/// <summary>
		/// 产品经理Id
		/// </summary>
		public long? ProductManagerId { get; set; }

		/// <summary>
		/// 事业部Id
		/// </summary>
		public string BusinessDivisionId { get; set; } = string.Empty;

		/// <summary>
		/// 事业部编码
		/// </summary>
		public string BusinessDivisionNumber { get; set; } = string.Empty;


		/// <summary>
		/// 事业部名称
		/// </summary>
		public string BusinessDivisionName { get; set; } = string.Empty;

		/// <summary>
		/// 供货组织Id
		/// </summary>
		public long SupplyOrgId { get; set; }

		/// <summary>
		/// 供货组织编码
		/// </summary>
		public string SupplyOrgNumber { get; set; } = string.Empty;

		/// <summary>
		/// 供货组织名称
		/// </summary>
		public string SupplyOrgName { get; set; } = string.Empty;

		/// <summary>
		/// 产品工程师名称
		/// </summary>
		public string? ProductEngineerName { get; set; }

		/// <summary>
		/// 产品经理
		/// </summary>
		public string? ProductManagerName { get; set; }

		/// <summary>
		/// 简易型号
		/// </summary>
		public string ShortNumber { get; set; } = string.Empty;

		/// <summary>
		/// 发货天数
		/// </summary>
		public int DeliveryDays { get; set; } = -1;

		/// <summary>
		/// 是否上架
		/// </summary>
		public bool IsRelease { get; set; }

		/// <summary>
		/// /图片地址
		/// </summary>
		public string? CatalogUrl { get; set; }

		/// <summary>
		/// 产品工程师Code
		/// </summary>
		public string? ProductEngineerWeChatCode { get; set; }

		/// <summary>
		/// 产品经理code
		/// </summary>
		public string? ProductManagerWeChatCode { get; set; }

		/// <summary>
		/// 数据来源
		/// 0.型号替换导入
		/// 1.供应商价目表
		/// 2.资料工程部门导入
		/// </summary>
		public int DataSource { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Memo { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public long CategoryId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte CategoryType { get; set; }

		/// <summary>
		/// 产品typeId
		/// </summary>
		public long TypeId { get; set; }

		/// <summary>
		/// 货期来源
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliverySource DeliverySource { get; set; } = DeliverySource.none;
	}
}
