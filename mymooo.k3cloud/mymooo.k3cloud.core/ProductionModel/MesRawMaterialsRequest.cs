using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.ProductionModel
{
	/// <summary>
	/// mes获取采购价请求
	/// </summary>
	public class MesRawMaterialsRequest
	{
		/// <summary>
		/// 生产订单id
		/// </summary>
		public long moId { get; set; }
		/// <summary>
		/// 明细id
		/// </summary>
		public long moEntryId { get; set; }
		/// <summary>
		/// 生产序号
		/// </summary>
		public int moSeq { get; set; }
		/// <summary>
		/// 生产订单编号
		/// </summary>
		public string moBillNo { get; set; } = string.Empty;
		/// <summary>
		/// 物料明细
		/// </summary>
		public List<MesRawMaterialsDetails> rawMtlInfo { get; set; } = [];
	}
	/// <summary>
	/// 请求明细
	/// </summary>
	public class MesRawMaterialsDetails
	{
		/// <summary>
		/// 物料编码
		/// </summary>
		public string mtlCode { get; set; } = string.Empty;
		/// <summary>
		/// 物料名称
		/// </summary>
		public string mtlName { get; set; } = string.Empty;
		/// <summary>
		/// 物料规格
		/// </summary>
		public string mtlSpec { get; set; } = string.Empty;
		/// <summary>
		/// 单价
		/// </summary>
		public decimal taxPrice { get; set; }
	}
	/// <summary>
	/// 销售信息
	/// </summary>
	public class MesRawSalInfo
	{
		/// <summary>
		/// 销售订单id
		/// </summary>
        public int FSALEORDERID { get; set; }
		/// <summary>
		/// 销售行id
		/// </summary>
		public int FSALEORDERENTRYID { get; set; }
	}
}
