using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.MaterialModel
{
	/// <summary>
	/// 物料请求信息
	/// </summary>
	public class MaterialInfoRequest
	{
		/// <summary>
		/// masterid
		/// </summary>
		public long MasterId { get; set; }
		/// <summary>
		/// MaterialId
		/// </summary>
		public long MaterialId { get; set; }
		/// <summary>
		/// 编码
		/// </summary>
		public string Code { get; set; } = string.Empty;
		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; } = string.Empty;
		/// <summary>
		/// 简称
		/// </summary>
		public string ShortNumber { get; set; } = string.Empty;
		/// <summary>
		/// 产品id
		/// </summary>
		public long ProductId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string PriceType { get; set; } = string.Empty;
		/// <summary>
		/// 物料分组
		/// </summary>
		public long MaterialGroupId { get; set; }
		/// <summary>
		/// 规格
		/// </summary>
		public string Specification { get; set; } = string.Empty;

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
		public string Textures { get; set; } = string.Empty;
		/// <summary>
		/// 材型
		/// </summary>
		public string MaterialType { get; set; } = string.Empty;

		/// <summary>
		/// 体积
		/// </summary>
		public decimal Volume { get; set; }
		/// <summary>
		/// 重量单位
		/// </summary>
		public Int64 WeightUnitid { get; set; }
		/// <summary>
		/// 尺寸单位
		/// </summary>
		public Int64 VolumeUnitid { get; set; }
	}
}
