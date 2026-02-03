using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	/// <summary>
	/// 收料通知单同步MES
	/// </summary>
	public class ReceiveDeliveryToMesEntity
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
		/// 收料组织编码
		/// </summary>
		public string StockOrgCode { get; set; }
		/// <summary>
		/// 收料组织名称
		/// </summary>
		public string StockOrgName { get; set; }
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
		/// 采购员编码
		/// </summary>
		public string PurchaserCode { get; set; }
		/// <summary>
		/// 采购员名称
		/// </summary>
		public string PurchaserName { get; set; }
		/// <summary>
		/// 供货方联系人
		/// </summary>
		public string Contact { get; set; }
		/// <summary>
		/// 供货方电话
		/// </summary>
		public string Tel { get; set; }
		/// <summary>
		/// 供货方手机
		/// </summary>
		public string Mobile { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// 明细
		/// </summary>
		public List<ReceiveDeliveryToMesDetEntity> Details { get; set; }
	}
	/// <summary>
	/// 明细
	/// </summary>
	public class ReceiveDeliveryToMesDetEntity
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
		/// 交货数量
		/// </summary>
		public decimal ActReceiveQty { get; set; }
		/// <summary>
		/// 预计到货日期
		/// </summary>
		public DateTime PreDeliveryDate { get; set; }
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
