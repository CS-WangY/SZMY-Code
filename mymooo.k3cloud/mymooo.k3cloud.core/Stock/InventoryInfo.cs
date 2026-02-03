using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace mymooo.k3cloud.core.Stock
{
	/// <summary>
	/// 库存相关信息
	/// </summary>
	[RedisKey("mymooo-product-stock", 4)]
	public class InventoryInfo
	{
		/// <summary>
		/// 物料编码
		/// </summary>
		[RedisPrimaryKey]
		[RedisValue]
		public string MaterialNumber { get; set; } = string.Empty;

		/// <summary>
		/// 物料名称
		/// </summary>
		public string MaterialName { get; set; } = string.Empty;

		/// <summary>
		/// 库存组织编码
		/// </summary>
		[RedisMainField(seq: 1, groupId: "inventory")]
		public string StockOrgNumber { get; set; } = string.Empty;

		/// <summary>
		/// 库存组织名称
		/// </summary>
		public string StockOrgName { get; set; } = string.Empty;

		/// <summary>
		/// 仓库编码
		/// </summary>
		[RedisMainField(seq: 2, groupId: "inventory")]
		public string StockNumber { get; set; } = string.Empty;

		/// <summary>
		/// 仓库名称
		/// </summary>
		public string StockName { get; set; } = string.Empty;

		/// <summary>
		/// 库存数量
		/// </summary>
		public decimal BaseQty { get; set; }

		/// <summary>
		/// 库存锁库量
		/// </summary>
		[RedisValue(mainKey: "inventory", prefix: "lockqty"), JsonIgnore]
		public decimal LockQty { get; set; }

		/// <summary>
		/// 单位编码
		/// </summary>
		public string UnitNumber { get; set; } = string.Empty;

		/// <summary>
		/// 单位名称
		/// </summary>
		public string UnitName { get; set; } = string.Empty;

		/// <summary>
		/// 此物料总待出库数量
		/// </summary>
		[RedisValue, JsonIgnore]
		public decimal UnQtyShipdSum { get; set; }

		/// <summary>
		/// 实际可用库存
		/// </summary>
		[JsonIgnore]
		public decimal UsableQty { get; set; }

		/// <summary>
		/// 采购在途量
		/// </summary>
		[RedisValue, JsonIgnore]
		public decimal OnOrderQty { get; set; }

		/// <summary>
		/// 品检库存
		/// </summary>
		[RedisValue, JsonIgnore]
		public decimal InspQty { get; set; }
	}
}
