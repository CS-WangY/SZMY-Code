using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// MES生产补料请求
    /// </summary>
    public class MesProductionFeedMtrlRequest
	{
        /// <summary>
        /// 补料单号
        /// </summary>
        public string FeedBillNo { get; set; }

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
		public List<MesProductionFeedMtrlMoDet> Details { get; set; }

    }

    /// <summary>
    /// MES生产补料的生产明细请求
    /// </summary>
    public class MesProductionFeedMtrlMoDet
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
        /// 生产订单明细物料
        /// </summary>
        public List<MesProductionFeedMtrlDet> SubDetails { get; set; }
    }

    /// <summary>
    /// MES生产补料请求明细
    /// </summary>
    public class MesProductionFeedMtrlDet
	{
        /// <summary>
        /// 物料编号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 实发数量
        /// </summary>
        public decimal ActualQty { get; set; }

        /// <summary>
        /// 补料仓库编码
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// 补料仓库名称
        /// </summary>
        public string StockName { get; set; }

        /// <summary>
        /// 补料原因
        /// </summary>
        public string FeedReasonId { get; set; } = "";
    }

	/// <summary>
	/// MES生产补料金蝶数据
	/// </summary>
	public class MesProductionFeedMtrlEntity
	{
		/// <summary>
		/// 组织ID
		/// </summary>
		public long FPRDORGID { get; set; }

		/// <summary>
		/// 订单ID
		/// </summary>
		public long FID { get; set; }

		/// <summary>
		/// 明细ID
		/// </summary>
		public long FENTRYID { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		public decimal FQTY { get; set; }

		/// <summary>
		/// 仓库编号
		/// </summary>
		public string FSTOCKCODE { get; set; }

		/// <summary>
		/// 补料原因
		/// </summary>
		public string FFeedReasonId { get; set; }
	}
}
