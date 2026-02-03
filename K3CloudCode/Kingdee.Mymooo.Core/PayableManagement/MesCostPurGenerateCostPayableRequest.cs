using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PayableManagement
{
	public class MesCostPurGenerateCostPayableRequest
	{
		/// <summary>
		/// 费用应付单据编号
		/// </summary>
		public string PayBillNo { get; set; }

		/// <summary>
		/// 单据日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 整单折扣金额
		/// </summary>
		public decimal OrderDiscountAmountFor { get; set; } = 0;

		public List<MesCostPurGenerateCostPayableDetails> Details { get; set; }
	}

	public class MesCostPurGenerateCostPayableDetails
	{
		/// <summary>
		/// 费用采购单据编号
		/// </summary>
		public string BillNo { get; set; }

		/// <summary>
		/// 订单ID
		/// </summary>
		public long FID { get; set; }

		public List<MesCostPurGenerateCostPayableSubDetails> SubDetails { get; set; }
	}

	/// <summary>
	/// 订单明细的子明细
	/// </summary>
	public class MesCostPurGenerateCostPayableSubDetails
	{

		/// <summary>
		/// 明细ID
		/// </summary>
		public long FENTRYID { get; set; }
		/// <summary>
		/// MES明细ID
		/// </summary>
		public string MesEntryId { get; set; }
        /// <summary>
        /// 费用编码
        /// </summary>
        public string MaterialCode { get; set; }
		/// <summary>
		/// 费用名称
		/// </summary>
		public string MaterialName { get; set; }
		/// <summary>
		/// 含税单价
		/// </summary>
		public decimal TaxPrice { get; set; }
		/// <summary>
		/// 数量
		/// </summary>
		public decimal Qty { get; set; }

		/// <summary>
		/// 折扣率(%)
		/// </summary>
		public decimal EntryDiscountRate { get; set; } = 0;

		/// <summary>
		/// 折扣额
		/// </summary>
		public decimal DiscountAmountfor { get; set; } = 0;

		/// <summary>
		/// 费用项目编码
		/// </summary>
		public string ChargeProjectCode { get; set; }
		/// <summary>
		/// 费用项目名称
		/// </summary>
		public string ChargeProjectName { get; set; }
		/// <summary>
		/// 生产订单编号
		/// </summary>
		public string MoNumber { get; set; }
		/// <summary>
		/// 生产订单行号
		/// </summary>
		public int MoEntrySeq { get; set; }

	}
	/// <summary>
	/// 费用应付删除
	/// </summary>
    public class CostPayableOrderDeleteRequest
    {
        /// <summary>
        /// 费用应付单据编号
        /// </summary>
        public string BillNo { get; set; }

    }
}
