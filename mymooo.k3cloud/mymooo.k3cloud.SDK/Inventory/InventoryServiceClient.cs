using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.k3cloud.core.Stock;

namespace mymooo.k3cloud.SDK.Inventory
{
	/// <summary>
	/// 库存缓存服务
	/// </summary>
	/// <param name="redisCache"></param>
	[AutoInject(InJectType.Single)]
	public class InventoryServiceClient(RedisCache redisCache)
	{
		private readonly RedisCache _redisCache = redisCache;

		/// <summary>
		/// 获取物料编码的库存信息  库存数量以及锁库数量
		/// </summary>
		/// <param name="materialNumber">物料编码</param>
		/// <returns></returns>
		public List<InventoryInfo> GetInventory(string materialNumber)
		{
			InventoryInfo inventory = new() { MaterialNumber = materialNumber };
			var result = _redisCache.HashGetMatchs(inventory, $"inventory-*");
			foreach (var item in result)
			{
				item.LockQty = _redisCache.HashGet(item, t => t.LockQty);
				item.UsableQty = item.BaseQty - item.LockQty;
			}

			return result;
		}

		/// <summary>
		/// 获取物料编码的库存信息  库存数量以及锁库数量  汇总
		/// </summary>
		/// <param name="materialNumber">物料编码</param>
		/// <returns></returns>
		public InventoryInfo? GetInventoryTotal(string materialNumber)
		{
			InventoryInfo inventory = new() { MaterialNumber = materialNumber };
			var datas = _redisCache.HashGetMatchs(inventory, $"inventory-*");
			foreach (var item in datas)
			{
				inventory.MaterialName = item.MaterialName;
				inventory.BaseQty += item.BaseQty;
				inventory.LockQty += _redisCache.HashGet(item, t => t.LockQty);
			}
			inventory.UnQtyShipdSum += _redisCache.HashGet(inventory, t => t.UnQtyShipdSum);
			inventory.InspQty += _redisCache.HashGet(inventory, t => t.InspQty);
			inventory.OnOrderQty += _redisCache.HashGet(inventory, t => t.OnOrderQty);
			inventory.UsableQty = inventory.BaseQty - inventory.LockQty;

			return inventory;
		}

		/// <summary>
		/// 获取组织下物料编码的库存信息  库存数量以及锁库数量
		/// </summary>
		/// <param name="materialNumber">物料编码</param>
		/// <param name="orgNumber">组织编码</param>
		/// <returns></returns>
		public List<InventoryInfo> GetInventory(string materialNumber, string orgNumber)
		{
			InventoryInfo inventory = new() { MaterialNumber = materialNumber };
			var result = _redisCache.HashGetMatchs(inventory, $"inventory-{orgNumber.ToLower()}-*");
			foreach (var item in result)
			{
				item.LockQty = _redisCache.HashGet(item, t => t.LockQty);
				item.UsableQty = item.BaseQty - item.LockQty;
			}

			return result;
		}

		/// <summary>
		/// 获取组织下物料编码的库存信息  库存数量以及锁库数量  汇总
		/// </summary>
		/// <param name="materialNumber">物料编码</param>
		/// <param name="orgNumber">组织编码</param>
		/// <returns></returns>
		public InventoryInfo? GetInventoryTotal(string materialNumber, string orgNumber)
		{
			InventoryInfo inventory = new() { MaterialNumber = materialNumber };
			var datas = _redisCache.HashGetMatchs(inventory, $"inventory-{orgNumber.ToLower()}-*");
			foreach (var item in datas)
			{
				inventory.MaterialName = item.MaterialName;
				inventory.StockOrgName = item.StockOrgName;
				inventory.BaseQty += item.BaseQty;
				inventory.LockQty += _redisCache.HashGet(item, t => t.LockQty);
			}
			inventory.UnQtyShipdSum += _redisCache.HashGet(inventory, t => t.UnQtyShipdSum);
			inventory.InspQty += _redisCache.HashGet(inventory, t => t.InspQty);
			inventory.OnOrderQty += _redisCache.HashGet(inventory, t => t.OnOrderQty);
			inventory.UsableQty = inventory.BaseQty - inventory.LockQty;

			return inventory;
		}

		/// <summary>
		/// 获取库存 库存以及锁库数量
		/// </summary>
		/// <param name="inventory">库存信息 必须传 materialNumber orgNumber StockNumber</param>
		/// <returns></returns>
		public InventoryInfo? GetInventory(InventoryInfo inventory)
		{
			var result = _redisCache.HashGet(inventory, "inventory");
			if (result != null)
			{
				result.LockQty = _redisCache.HashGet(inventory, p => p.LockQty);
				result.UsableQty = result.BaseQty - result.LockQty;
			}

			return result;
		}

		/// <summary>
		///  获取库存  缓存全部信息包含 总待出数量,在途数 以及 品检数
		/// </summary>
		/// <param name="inventory">库存信息 必须传 materialNumber orgNumber StockNumber</param>
		/// <returns></returns>
		public InventoryInfo? GetInventoryTotal(InventoryInfo inventory)
		{
			var result = GetInventory(inventory);
			if (result != null)
			{
				result.InspQty = _redisCache.HashGet(inventory, p => p.InspQty);
				result.UnQtyShipdSum = _redisCache.HashGet(inventory, p => p.UnQtyShipdSum);
				result.OnOrderQty = _redisCache.HashGet(inventory, p => p.OnOrderQty);
			}

			return result;
		}
	}
}
