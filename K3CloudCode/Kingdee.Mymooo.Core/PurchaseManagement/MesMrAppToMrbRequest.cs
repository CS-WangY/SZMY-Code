using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	/// <summary>
	/// MES退料申请下推采购退料
	/// </summary>
	public class MesMrAppToMrbRequest
	{
		/// <summary>
		/// 采购退料单据编号
		/// </summary>
		public string MrbBillNo { get; set; }

		/// <summary>
		/// 退料订单ID
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 退料订单编号
		/// </summary>
		public string BillNo { get; set; }

		/// <summary>
		/// 退料明细
		/// </summary>
		public List<MesMrAppToMrbDet> Details { get; set; }
       
    }

	/// <summary>
	///MES退料申请下推采购退料明细请求
	/// </summary>
	public class MesMrAppToMrbDet
	{
		/// <summary>
		/// 退料申请订单明细ID
		/// </summary>
		public long EntryId { get; set; }

		/// <summary>
		/// 退料申请订单序号
		/// </summary>
		public int BillSeq { get; set; } = 0;

        /// <summary>
        /// 物料编号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

		/// <summary>
		/// 实退数量	
		/// </summary>
		public decimal RmrealQty { get; set; }

		/// <summary>
		/// 仓库编码
		/// </summary>
		public string StockCode { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string StockName { get; set; }
    }
}
