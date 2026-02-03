using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
	/// <summary>
	/// MES根据型材，材质，长宽高模糊查询总库存
	/// </summary>
	public class FuzzyQueryStockBaseQtyRequest
	{
		/// <summary>
		/// 材质
		/// </summary>
		public string Textures { get; set; } = "";

		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialType { get; set; } = "";

		/// <summary>
		/// 长度(开始)
		/// </summary>
		public decimal StartLength { get; set; } = 0;

		/// <summary>
		/// 长度(结束)
		/// </summary>
		public decimal EndLength { get; set; } = 0;

		/// <summary>
		/// 宽度(开始)
		/// </summary>
		public decimal StartWidth { get; set; } = 0;

		/// <summary>
		/// 宽度(结束)
		/// </summary>
		public decimal EndWidth { get; set; } = 0;

		/// <summary>
		/// 高度(开始)
		/// </summary>
		public decimal StartHeight { get; set; } = 0;

		/// <summary>
		/// 高度(结束)
		/// </summary>
		public decimal EndHeight { get; set; } = 0;
	}
}
