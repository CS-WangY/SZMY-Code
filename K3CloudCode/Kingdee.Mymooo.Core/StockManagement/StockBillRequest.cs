using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
	/// <summary>
	/// 库存查询
	/// </summary>
	public class StockQuantityEntity
	{
		/// <summary>
		/// 组织ID
		/// </summary>
		public long OrgId { get; set; }
		/// <summary>
		/// 组织编码
		/// </summary>
		public string OrgNumber { get; set; }

		/// <summary>
		/// 仓库编码
		/// </summary>
		public long StockID { get; set; }

		/// <summary>
		/// 仓库编码
		/// </summary>
		public string StockNumber { get; set; }

		/// <summary>
		/// 产品型号ID
		/// </summary>
		public long ItemID { get; set; }
		/// <summary>
		/// 产品型号
		/// </summary>
		public string ItemNo { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		public decimal Quantity { get; set; }

		/// <summary>
		/// 是否外发仓库
		/// </summary>
		public bool IsOutSourceStock { get; set; }

		/// <summary>
		/// 此物料总待出库数量
		/// </summary>
		public decimal UnQtyShipdSum { get; set; }

		/// <summary>
		/// 实际可用库存
		/// </summary>
		public decimal UsableQty { get; set; }

		/// <summary>
		/// 采购在途量
		/// </summary>
		public decimal OnOrderQty { get; set; }

		/// <summary>
		/// 品检库存
		/// </summary>
		public decimal QtyInsp { get; set; }
	}

	public class KeyValue<T1, T2>
	{
		public T1 Key { get; set; }
		public T2 Value { get; set; }
	}
	public class StockPlatEntity
	{
		/// <summary>
		/// 物料ID
		/// </summary>
		public int FMATERIALID { get; set; }
		/// <summary>
		/// 组织ID
		/// </summary>
		public int FORGID { get; set; }
		/// <summary>
		/// 组织编码
		/// </summary>
		public string ORGNUM { get; set; }
		/// <summary>
		/// 组织名称
		/// </summary>
		public string ORGNAME { get; set; }
		/// <summary>
		/// 仓库名称
		/// </summary>
		public string STONAME { get; set; }
		/// <summary>
		/// 仓库ID
		/// </summary>
		public int STOID { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string STONUM { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MATERIALNUM { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string MATERIALNAME { get; set; }
		/// <summary>
		/// 库存数量
		/// </summary>
		public int FBASEQTY { get; set; }
		/// <summary>
		/// 可用库存量
		/// </summary>
		public int FAVBQTY { get; set; }
		/// <summary>
		/// 弱预留可用库存量
		/// </summary>
		public int FAVBQTYL { get; set; }
		/// <summary>
		/// 库存锁库量
		/// </summary>
		public int FLOCKQTY { get; set; }
		/// <summary>
		/// 单位编码
		/// </summary>
		public string UNITNUM { get; set; }
		/// <summary>
		/// 单位名称
		/// </summary>
		public string UNITNAME { get; set; }
		/// <summary>
		/// 是否为外放仓库
		/// </summary>
		public string FISOUTSOURCESTOCK { get; set; }
		/// <summary>
		/// 仓库地址
		/// </summary>
		public string FOUTSOURCESTOCKLOC { get; set; }
		/// <summary>
		/// 此物料总待出库数量
		/// </summary>
		public decimal UNQTYSHIPDSUM { get; set; }

		/// <summary>
		/// 实际可用库存
		/// </summary>
		public decimal USABLEQTY { get; set; }

		/// <summary>
		/// 采购在途量
		/// </summary>
		public decimal ONORDERQTY { get; set; }

		/// <summary>
		/// 品检库存
		/// </summary>
		public decimal QTYINSP { get; set; }
	}

	public class StockQuantityData
	{
		public decimal AvailableQuantity { get; set; }
		public string ItemNo { get; set; }
		public decimal Quantity { get; set; }

		public StockInfo StockInfo { get; set; }
	}

	public class StockInfo
	{
		/// <summary>
		/// 此物料总待出库数量
		/// </summary>
		public decimal UnQtyShipdSum { get; set; }

		/// <summary>
		/// 实际可用库存
		/// </summary>
		public decimal UsableQty { get; set; }

		/// <summary>
		/// 采购在途量
		/// </summary>
		public decimal OnOrderQty { get; set; }

		/// <summary>
		/// 品检库存
		/// </summary>
		public decimal QtyInsp { get; set; }

	}

	/// <summary>
	/// 云存储库存查询
	/// </summary>
	public class CloudStockBaseQtyRequest
	{
		/// <summary>
		/// 组织编码
		/// </summary>
		public string SupplyOrgCode { get; set; }

		/// <summary>
		/// 仓库编码
		/// </summary>
		public string CloudStockCode { get; set; }

		/// <summary>
		/// 物料编码集合
		/// </summary>
		public List<string> ItemNos { get; set; }

	}

	/// <summary>
	/// 云存储库存查询
	/// </summary>
	public class CloudStockBaseQtyEntity
	{
		/// <summary>
		/// 组织编码
		/// </summary>
		public string SupplyOrgCode { get; set; }

		/// <summary>
		/// 仓库编码
		/// </summary>
		public string CloudStockCode { get; set; }

		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemNo { get; set; }

		/// <summary>
		/// 即时库存
		/// </summary>
		public decimal Qty { get; set; }
	}

	/// <summary>
	/// 获取同一个组织多物料即时库存数
	/// </summary>
	public class InventoryQtyV2Entity
	{
		/// <summary>
		/// 组织ID
		/// </summary>
		public long OrgId { get; set; }

		/// <summary>
		/// 仓库ID
		/// </summary>
		public long StockID { get; set; }

		/// <summary>
		/// 产品型号MasterID
		/// </summary>
		public long ItemMasterID { get; set; }

		/// <summary>
		/// 即时库存
		/// </summary>
		public decimal Qty { get; set; } = 0;


	}

	/// <summary>
	/// MES根据型材，材质，长宽高模糊查询总库存
	/// </summary>
	public class MesFuzzyQueryStockBaseQtyEntity
	{
		/// <summary>
		/// 物料ID
		/// </summary>
		public int FMATERIALID { get; set; }
		/// <summary>
		/// 组织ID
		/// </summary>
		public int FORGID { get; set; }
		/// <summary>
		/// 组织编码
		/// </summary>
		public string ORGNUM { get; set; }
		/// <summary>
		/// 组织名称
		/// </summary>
		public string ORGNAME { get; set; }
		/// <summary>
		/// 仓库名称
		/// </summary>
		public string STONAME { get; set; }
		/// <summary>
		/// 仓库ID
		/// </summary>
		public int STOID { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string STONUM { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MATERIALNUM { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string MATERIALNAME { get; set; }
		/// <summary>
		/// 库存数量
		/// </summary>
		public decimal FBASEQTY { get; set; }

		/// <summary>
		/// 单位编码
		/// </summary>
		public string UNITNUM { get; set; }
		/// <summary>
		/// 单位名称
		/// </summary>
		public string UNITNAME { get; set; }

		/// <summary>
		/// 长
		/// </summary>
		public decimal Length { get; set; } = 0;

		/// <summary>
		/// 宽
		/// </summary>
		public decimal Width { get; set; } = 0;

		/// <summary>
		/// 高
		/// </summary>
		public decimal Height { get; set; } = 0;

		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialTypeCode { get; set; } = "";
		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialTypeName { get; set; } = "";

		/// <summary>
		/// 材质
		/// </summary>
		public string TexturesCode { get; set; } = "";
		/// <summary>
		/// 材质
		/// </summary>
		public string TexturesName { get; set; } = "";

		/// <summary>
		/// 型号规格
		/// </summary>
		public string Specification { get; set; }

		/// <summary>
		/// 大类编码
		/// </summary>
		public string ParentSmallCode { get; set; }

		/// <summary>
		/// 大类编码
		/// </summary>
		public string ParentSmallName { get; set; }

		/// <summary>
		/// 小类编码
		/// </summary>
		public string SmallCode { get; set; }

		/// <summary>
		/// 小类名称
		/// </summary>
		public string SmallName { get; set; }

	}

	public class MesStockPlatEntity
	{
		/// <summary>
		/// 物料ID
		/// </summary>
		public int FMATERIALID { get; set; }
		/// <summary>
		/// 组织ID
		/// </summary>
		public int FORGID { get; set; }
		/// <summary>
		/// 组织编码
		/// </summary>
		public string ORGNUM { get; set; }

		/// <summary>
		/// 仓库ID
		/// </summary>
		public int STOID { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string STONUM { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MATERIALNUM { get; set; }
		/// <summary>
		/// 库存数量
		/// </summary>
		public decimal FBASEQTY { get; set; }

		/// <summary>
		/// 单位编码
		/// </summary>
		public string UNITNUM { get; set; }

	}

}
