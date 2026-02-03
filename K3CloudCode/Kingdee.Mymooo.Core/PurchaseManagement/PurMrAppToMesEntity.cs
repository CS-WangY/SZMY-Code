using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	/// <summary>
	/// 退料申请同步MES
	/// </summary>
	public class PurMrAppToMesEntity
	{
		/// <summary>
		/// 表单ID
		/// </summary>
		public string FormId { get; set; }
		/// <summary>
		/// 操作类型
		/// </summary>
		public string OperationNumber { get; set; }
		/// <summary>
		/// 订单ID
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 单据编号
		/// </summary>
		public string BillNo { get; set; }
		/// <summary>
		/// 日期
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// 审核日期
		/// </summary>
		public DateTime ApproveDate { get; set; }
		/// <summary>
		/// 申请组织编码
		/// </summary>
		public string AppOrgCode { get; set; }
		/// <summary>
		/// 申请组织名称
		/// </summary>
		public string AppOrgName { get; set; }
		/// <summary>
		/// 供应商ID
		/// </summary>
		public long SupplierId { get; set; }
		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// 明细
		/// </summary>
		public List<PurMrAppToMesDetEntity> Details { get; set; }
	}
	/// <summary>
	/// 明细
	/// </summary>
	public class PurMrAppToMesDetEntity
	{
		/// <summary>
		/// 明细ID
		/// </summary>
		public long EntryId { get; set; }
		/// <summary>
		/// 明细序号
		/// </summary>
		public int BillSeq { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string MaterialCode { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string MaterialName { get; set; }
		/// <summary>
		/// 产品大类ID
		/// </summary>
		public int ParentSmallId { get; set; }
		/// <summary>
		/// 产品大类编码
		/// </summary>
		public string ParentSmallCode { get; set; }
		/// <summary>
		/// 产品大类名称
		/// </summary>
		public string ParentSmallName { get; set; }
		/// <summary>
		/// 产品小类ID
		/// </summary>
		public int SmallId { get; set; }
		/// <summary>
		/// 产品小类编码
		/// </summary>
		public string SmallCode { get; set; }
		/// <summary>
		/// 产品小类名称
		/// </summary>
		public string SmallName { get; set; }
		/// <summary>
		/// 申请数量
		/// </summary>
		public decimal MrAppQty { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string StockCode { get; set; }
		/// <summary>
		/// 仓库名称
		/// </summary>
		public string StockName { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string NoteEntry { get; set; }
		/// <summary>
		/// 源单内码
		/// </summary>
		public long SrcId { get; set; }
		/// <summary>
		/// 源单单号
		/// </summary>
		public string SrcBillNo { get; set; }
		/// <summary>
		/// 源单分录内码
		/// </summary>
		public long SrcEntryId { get; set; }
		/// <summary>
		/// 源单序号
		/// </summary>
		public int SrcSeq { get; set; }
		/// <summary>
		/// 源单类型
		/// </summary>
		public string SrcFormId { get; set; }
	}
}
