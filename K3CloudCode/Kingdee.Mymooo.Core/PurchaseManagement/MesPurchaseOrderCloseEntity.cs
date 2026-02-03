using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	public class MesPurchaseOrderCloseEntity
	{
		/// <summary>
		/// 表单ID
		/// </summary>
		public string FormId { get; set; }
		/// <summary>
		/// 订单ID
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 单据编号
		/// </summary>
		public string BillNo { get; set; }
		/// <summary>
		/// 明细
		/// </summary>
		public List<MesPurchaseOrderCloseDetEntity> Details { get; set; }
	}

	/// <summary>
	/// 明细
	/// </summary>
	public class MesPurchaseOrderCloseDetEntity
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
		/// 关闭人
		/// </summary>
		public string CloseUserName { get; set; }
	}
}
