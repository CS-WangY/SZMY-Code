using com.mymooo.mall.core.Model.Cache;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.Model.OldPlatformAdmin.Selection
{
	public class ProductionSelectionResponse
	{
		public DisplayParam? DisplayParamData { get; set; }
		public ProductSpecParamlist[] ProductSpecParamList { get; set; } = [];
	
		public bool IsDisplayPrice { get; set; }
		public bool IsMethodComplete { get; set; }
		public string Number { get; set; } = string.Empty;
		public bool IsEndEach { get; set; }
		public int DispatchDays { get; set; }
		public decimal UnitPriceWithTax { get; set; }
		public int MaxQty { get; set; }
		public int SupplyUnitPrice { get; set; }
		public SalesPrice? Sales { get; set; }
		public class DisplayParam
		{
			public ProductType[] ProductTypeList { get; set; } = [];
		}

		public class ProductType
		{
			public long TypeID { get; set; }
			public string TypeCode { get; set; } = string.Empty;
			public string TypeDesc { get; set; } = string.Empty;
			public Dictionary<long, long> TypeValues { get; set; } = [];
			public string RefTypeCode { get; set; } = string.Empty;
		}

		public class SalesPrice
		{
			[JsonPropertyName("PRD_ID")]
			public long ProductId { get; set; }
			[JsonPropertyName("TYPE_ID")]
			public long ProductTypeId { get; set; }
			[JsonPropertyName("D_DISCOUNT_SALELIST")]
			public SalesLadderPrice[] SalesPrices { get; set; } = [];
		}

		public class SalesLadderPrice
		{
			[JsonPropertyName("PDS_UPPER_LIMIT")]
			public int Qty { get; set; }
			[JsonPropertyName("PDS_DISCOUNT_RATE")]
			public decimal DiscountRate { get; set; }
			[JsonPropertyName("PDS_APPEND_DAYS")]
			public int AppendDays { get; set; }
			[JsonPropertyName("PDS_TYPE_ID")]
			public long ProductTypeId { get; set; }
		}

		public class ProductSpecParamlist
		{
			public string Number { get; set; } = string.Empty;
			public string ShortNumber { get; set; } = string.Empty;
			public decimal Price { get; set; }
			public bool IsDiscount { get; set; }
			public decimal OriginalPrcie { get; set; }
			public int Delivery { get; set; }
			public int OriginalDelivery { get; set; }
			public required Querys Querys { get; set; }
			public decimal SupplyUnitPrice { get; set; }
		}

		public class Querys
		{
			public long ProductId { get; set; }
			public List<long> TypeList { get; set; } = [];
			public List<ProductQueryParameter> ProductQueryParamList { get; set; } = [];
		}
	}
}
