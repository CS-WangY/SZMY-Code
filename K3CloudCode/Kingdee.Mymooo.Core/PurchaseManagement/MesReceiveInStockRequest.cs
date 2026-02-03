using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	/// <summary>
	/// MES收料下推采购入库
	/// </summary>
	public class MesReceiveInStockRequest
	{
		/// <summary>
		/// 采购入库单据编号
		/// </summary>
		public string InStockBillNo { get; set; }

		/// <summary>
		/// 收料订单ID
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 收料订单编号
		/// </summary>
		public string BillNo { get; set; }

		/// <summary>
		/// 收料明细
		/// </summary>
		public List<MesReceiveInStockDet> Details { get; set; }
       
    }

	/// <summary>
	///MES收料下推采购入库明细请求
	/// </summary>
	public class MesReceiveInStockDet
	{
        /// <summary>
        /// 收料订单明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 收料订单序号
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
		/// 交货数量
		/// </summary>
		public decimal ActReceiveQty { get; set; }

		/// <summary>
		/// 实收数量	
		/// </summary>
		public decimal RealQty { get; set; }

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
