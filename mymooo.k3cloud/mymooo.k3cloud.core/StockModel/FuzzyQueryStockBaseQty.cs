using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.StockModel
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

	/// <summary>
	/// MES根据型材，材质，长宽高模糊查询总库存
	/// </summary>
	public class MesFuzzyQueryStockBaseQtyEntity
	{
		/// <summary>
		/// 物料ID
		/// </summary>
		public long FMATERIALID { get; set; }
		/// <summary>
		/// 组织ID
		/// </summary>
		public long FORGID { get; set; }
		/// <summary>
		/// 组织编码
		/// </summary>
		public string ORGNUM { get; set; } = "";
		/// <summary>
		/// 组织名称
		/// </summary>
		public string ORGNAME { get; set; } = "";
		/// <summary>
		/// 仓库名称
		/// </summary>
		public string STONAME { get; set; } = "";
		/// <summary>
		/// 仓库ID
		/// </summary>
		public long STOID { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string STONUM { get; set; } = "";
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MATERIALNUM { get; set; } = "";
		/// <summary>
		/// 物料名称
		/// </summary>
		public string MATERIALNAME { get; set; } = "";
		/// <summary>
		/// 库存数量
		/// </summary>
		public decimal QTY { get; set; } = 0;

		/// <summary>
		/// 单位编码
		/// </summary>
		public string UNITNUM { get; set; } = "";
		/// <summary>
		/// 单位名称
		/// </summary>
		public string UNITNAME { get; set; } = "";

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
		public string Specification { get; set; } = "";

		/// <summary>
		/// 大类编码
		/// </summary>
		public string ParentSmallCode { get; set; } = "";

		/// <summary>
		/// 大类编码
		/// </summary>
		public string ParentSmallName { get; set; } = "";

		/// <summary>
		/// 小类编码
		/// </summary>
		public string SmallCode { get; set; } = "";

		/// <summary>
		/// 小类名称
		/// </summary>
		public string SmallName { get; set; } = "";

	}
}
