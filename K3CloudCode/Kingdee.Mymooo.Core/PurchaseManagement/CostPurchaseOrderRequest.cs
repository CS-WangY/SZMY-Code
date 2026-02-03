using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
	/// <summary>
	/// 费用采购订单
	/// </summary>
	public class CostPurchaseOrderRequest
	{
		/// <summary>
		/// 单据编号
		/// </summary>
		public string BillNo { get; set; }

		/// <summary>
		/// 单据日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 组织编码
		/// </summary>
		public string PurchaseOrgCode { get; set; }

		/// <summary>
		/// 组织名称
		/// </summary>
		public string PurchaseOrgName { get; set; }

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
		/// 费用采购订单明细
		/// </summary>
		public List<CostPurchaseOrderDet> Details { get; set; }
	}

	/// <summary>
	/// 费用采购订单明细
	/// </summary>
	public class CostPurchaseOrderDet
	{
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
		/// 交货日期
		/// </summary>
		public DateTime DeliveryDate { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Note { get; set; }
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
		/// <summary>
		/// 生产车间编号
		/// </summary>
		public string WorkShopCode { get; set; }
        /// <summary>
        /// 费用承担部门编号
        /// </summary>
        public string CostWorkShopCode { get; set; }
    }
}
