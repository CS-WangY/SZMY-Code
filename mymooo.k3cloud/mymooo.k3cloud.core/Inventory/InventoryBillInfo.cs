namespace mymooo.k3cloud.core.Inventory
{
	/// <summary>
	/// 库存单据信息
	/// </summary>
	public class InventoryBillInfo
	{
		/// <summary>
		/// Id 
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 单据编码
		/// </summary>
		public required string BillNo { get; set; }

		/// <summary>
		/// 是否为增加库存
		/// </summary>
		public bool AddQty { get; set; }

		/// <summary>
		/// 单据日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 库存组织
		/// </summary>
		public required StockOrganization StockOrg { get; set; }

		/// <summary>
		/// 单据明细
		/// </summary>
		public required InventoryDetail[] Details { get; set; }
	}

	/// <summary>
	/// 库存组织
	/// </summary>
	public class StockOrganization
	{
		/// <summary>
		/// 组织Id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 组织编码
		/// </summary>
		public required string Code { get; set; }

		/// <summary>
		/// 组织名称
		/// </summary>
		public required string Name { get; set; }

		/// <summary>
		/// 云仓储发货位置
		/// </summary>
		public string? OutSourceStockLoc { get; set; }

		/// <summary>
		/// 是否同步云仓储
		/// </summary>
		public bool TransIsSynCloudStock { get; set; }
	}

	/// <summary>
	/// 单据明细信息
	/// </summary>
	public class InventoryDetail
    {
		/// <summary>
		/// 明细Id
		/// </summary>
		public long DetailId { get; set; }

		/// <summary>
		/// 序号
		/// </summary>
		public int Seq { get; set; }

		/// <summary>
		/// 订单号
		/// </summary>
		public string? OrderNo { get; set; }

		/// <summary>
		/// 订单明细Id
		/// </summary>
		public long OrderEntryId { get; set; }

		/// <summary>
		/// 物料信息
		/// </summary>
		public required Material Material { get; set; }

		/// <summary>
		/// 仓库信息
		/// </summary>
		public required Stock Stock { get; set; }

		/// <summary>
		/// d单位
		/// </summary>
		public required BasisInfo Unit { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		public decimal Qty { get; set; }

		/// <summary>
		/// 基本单位
		/// </summary>
		public required BasisInfo BaseUnit { get; set; }

		/// <summary>
		/// 基本单位数量
		/// </summary>
		public decimal BaseQty { get; set; }

		/// <summary>
		/// 辅助单位
		/// </summary>
		public BasisInfo? SecUnit { get; set; }

		/// <summary>
		/// 辅助单位数量
		/// </summary>
		public decimal SecQty { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string? ProjectNo { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string? MtoNo { get; set; }

		/// <summary>
		/// 库存状态
		/// </summary>
		public required BasisInfo StockStatus { get; set; }
	}

	/// <summary>
	/// 物料信息
	/// </summary>
	public class Material : BasisInfo
	{
		/// <summary>
		/// 材质
		/// </summary>
		public string? Textures { get; set; }

		/// <summary>
		/// 净重
		/// </summary>
		public decimal NetWeight { get; set; }

		/// <summary>
		/// 是否启用库存
		/// </summary>
		public bool IsInventory { get; set; }
	}

	/// <summary>
	/// 仓库
	/// </summary>
	public class Stock : BasisInfo
	{
		/// <summary>
		/// 云仓储编码
		/// </summary>
		public string? CloudStockCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string? OutSourceStockLoc { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool PrintAutoStockOut { get; set; }

		/// <summary>
		/// 是否同步云仓储
		/// </summary>
		public bool SyncToWarehouse { get; set; }

		/// <summary>
		/// 是否直发仓库
		/// </summary>
		public bool IsDirStock { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool IsOutSourceStock { get; set; }
	}

	/// <summary>
	/// 基础信息
	/// </summary>
	public class BasisInfo
	{
		/// <summary>
		/// Id
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// 编码
		/// </summary>
		public required string Code { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		public required string Name { get; set; }
	}
}
