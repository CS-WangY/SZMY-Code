using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
	public class Mes_STK_MiscellaneousRequest
	{
		/// <summary>
		/// 单据编号
		/// </summary>
		public string BillNo { get; set; }
		/// <summary>
		/// 库存组织
		/// </summary>
		public string StockOrgNumber { get; set; }

		public long StockOrgId { get; set; }
		/// <summary>
		/// 单据类型
		/// </summary>
		public string BillTypeID { get; set; }
		/// <summary>
		/// 库存方向
		/// </summary>
		public string StockDirect { get; set; }
		/// <summary>
		/// 部门
		/// </summary>
		public string DeptID { get; set; }
		/// <summary>
		/// 单据时间
		/// </summary>
		public string FDate { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// 明细
		/// </summary>
		public List<Mes_STK_MiscellaneousEntity> Details { get; set; }
	}
	public class Mes_STK_MiscellaneousEntity
	{
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MaterialCode { get; set; }

		/// <summary>
		/// 物料名称
		/// </summary>
		public string MaterialName { get; set; }
		/// <summary>
		/// 收货仓库编码
		/// </summary>
		public string StockCode { get; set; }

		/// <summary>
		/// 客户物料编码
		/// </summary>
		public string CustItemNo { get; set; }

		/// <summary>
		/// 客户物料名称
		/// </summary>
		public string CustItemName { get; set; }

		/// <summary>
		/// 实收数量
		/// </summary>
		public decimal Qty { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Note { get; set; }

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
		/// 净重
		/// </summary>
		public decimal Weight { get; set; } = 0;

		/// <summary>
		/// 材质
		/// </summary>
		public string Textures { get; set; } = "";

		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialType { get; set; } = "";

		/// <summary>
		/// 体积
		/// </summary>
		public decimal Volume { get; set; } = 0;

		/// <summary>
		/// 重量单位
		/// </summary>
		public string WeightUnitid { get; set; }

		/// <summary>
		/// 尺寸单位
		/// </summary>
		public string VolumeUnitid { get; set; }
		/// <summary>
		/// 产品ID
		/// </summary>
		public long ProductId { get; set; } = 0;
		/// <summary>
		/// 型号规格
		/// </summary>
		public string Specs { get; set; } = "";

		public SalesOrderBillRequest.Productsmallclass ProductSmallClass { get; set; }
	}

}
