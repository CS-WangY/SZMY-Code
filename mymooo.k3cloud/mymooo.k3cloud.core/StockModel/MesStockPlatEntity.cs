using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.StockModel
{
	/// <summary>
	/// 获取即时库存(MES专用)
	/// </summary>
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
		public string ORGNUM { get; set; } = string.Empty;

		/// <summary>
		/// 仓库ID
		/// </summary>
		public int STOID { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string STONUM { get; set; } = string.Empty;
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MATERIALNUM { get; set; } = string.Empty;
		/// <summary>
		/// 库存数量
		/// </summary>
		public decimal QTY { get; set; }

		/// <summary>
		/// 单位编码
		/// </summary>
		public string UNITNUM { get; set; } = string.Empty;

	}
}
