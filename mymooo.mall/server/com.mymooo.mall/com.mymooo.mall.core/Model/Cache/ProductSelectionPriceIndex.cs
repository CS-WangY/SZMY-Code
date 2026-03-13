using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 产品价格缓存
	/// </summary>
	[RedisKey("mymooo-product-number", databaseId: 1)]
	public class ProductSelectionPriceIndex
	{
		private string porductNumber = string.Empty;

		/// <summary>
		/// 主键
		/// </summary>
		[RedisPrimaryKey]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// 产品型号
		/// </summary>
		[JsonPropertyName("productModel")]
		public string PorductNumber
		{
			get => porductNumber;
			set
			{
				porductNumber = value;
				this.Id = porductNumber.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 产品小类
		/// </summary>
		[JsonPropertyName("smallClassId")]
		public long SmallId { get; set; }

		/// <summary>
		/// 企业客户编码
		/// </summary>
		[RedisMainField(1)]
		public string CompanyCode { get; set; } = string.Empty;

		/// <summary>
		/// 阶梯价
		/// </summary>
		[JsonPropertyName("priceList")]
		public List<ProductSelectionLadderPrice> LadderPrices { get; set; } = [];

		/// <summary>
		/// 价格来源
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[JsonPropertyName("dataType")]
		[RedisMainField(2)]
		public PriceSource PriceSource { get; set; }

		/// <summary>
		/// 货期来源
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliverySource DeliverySource { get; set; } = DeliverySource.none;

		/// <summary>
		/// 如果是历史价格,有个失效日期
		/// </summary>
		public DateTime? ExpiryDate { get; set; }
	}
}
