using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// MES生产退料请求
    /// </summary>
    public class MesProductionReturnMtrlRequest
	{
        /// <summary>
        /// 退料单号
        /// </summary>
        public string ReturnBillNo { get; set; }

		/// <summary>
		/// 退料来源
		/// </summary>
		public string ReturnSource { get; set; }

		/// <summary>
		/// 退料来源单据编号
		/// </summary>
		public string ReturnSourceBillNo { get; set; }

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
		public List<MesProductionReturnMtrlMoDet> Details { get; set; }

    }

    /// <summary>
    /// MES生产退料的生产明细请求
    /// </summary>
    public class MesProductionReturnMtrlMoDet
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
        public List<MesProductionReturnMtrlDet> SubDetails { get; set; }
    }

    /// <summary>
    /// MES生产退料请求明细
    /// </summary>
    public class MesProductionReturnMtrlDet
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
		/// 实退数量
		/// </summary>
		public decimal Qty { get; set; }

        /// <summary>
        /// 退料仓库编码
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// 退料仓库名称
        /// </summary>
        public string StockName { get; set; }

		/// <summary>
		/// 退料类型
		/// </summary>
		public string ReturnType { get; set; } = "";

		/// <summary>
		/// 退料原因
		/// </summary>
		public string ReturnReason { get; set; } = "";
    }

	/// <summary>
	/// MES生产退料金蝶数据
	/// </summary>
	public class MesProductionReturnMtrlEntity
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
		/// 退料类型
		/// </summary>
		public string FReturnType { get; set; }

		/// <summary>
		/// 退料原因
		/// </summary>
		public string FReturnReason { get; set; }
	}
}
