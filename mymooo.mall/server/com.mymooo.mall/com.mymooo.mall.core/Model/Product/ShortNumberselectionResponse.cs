using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.Model.Product
{
	[RedisKey("mymooo-product-number", isSaveMain: false, databaseId: 1)]
	public class ShortNumberselectionResponse
	{
		private string number = string.Empty;

		[RedisPrimaryKey]
		public string Id { get; set; } = string.Empty;
		public long ProductId { get; set; }
		[RedisValue]
		public string ShortNumber { get; set; } = string.Empty;

		[RedisValue]
		public List<ProductSelectParameterValue> ProductSelectParameterValues { get; set; } = [];

		[JsonPropertyName("SupplierShortNumbers")]
		public List<SpecialShortNumber> ShortNumbers { get; set; } = [];
		public string Number
		{
			get => number;
			set
			{
				number = value;
				this.Id = number.Replace("-", "").Trim().ToLower();
			}
		}
		public class ProductSelectParameterValue
		{
			public string Code { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
			public string Value { get; set; } = string.Empty;
			public decimal ComparativeValue { get; set; }

			public bool IsSelect { get; set; }
		}

		[RedisKey("mymooo-product-number", databaseId: 1)]
		public class SpecialShortNumber
		{
			private string number = string.Empty;

			[RedisPrimaryKey]
			public string Id { get; set; } = string.Empty;

			[RedisMainField]
			public string Code { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
			public string ShortNumber { get; set; } = string.Empty;
			public string PriceType { get; set; } = string.Empty;
			public string Number
			{
				get => number;
				set
				{
					number = value;
					this.Id = number.Replace("-", "").Trim().ToLower();
				}
			}
		}
	}
}
