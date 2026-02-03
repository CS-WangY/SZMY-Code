using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// MES生产入库请求
    /// </summary>
    public class MesProductionInStockRequest
    {
        /// <summary>
        /// 生产入库单号
        /// </summary>
        public string InStockBillNo { get; set; }

		/// <summary>
		/// 生产订单ID
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 生产订单编号
		/// </summary>
		public string BillNo { get; set; }

		/// <summary>
		/// 生产明细
		/// </summary>
		public List<MesProductionInStockMoDet> Details { get; set; }
       
    }

    /// <summary>
    ///MES生产入库的生产明细请求
    /// </summary>
    public class MesProductionInStockMoDet
    {


        /// <summary>
        /// 生产订单明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 生产订单序号
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
        /// 数量
        /// </summary>
        public decimal MustQty { get; set; }

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
