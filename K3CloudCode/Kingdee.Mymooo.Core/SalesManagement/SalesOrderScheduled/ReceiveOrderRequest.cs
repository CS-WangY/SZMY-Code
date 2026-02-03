using System;

namespace Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled
{
	public class ReceiveOrderRequest
	{
		/// <summary>
		/// 收退款单号
		/// </summary>
		public string ReceiveBillNo { get; set; }

		/// <summary>
		/// 操作
		/// </summary>
		public string Operation { get; set; }

		/// <summary>
		/// 业务员名称
		/// </summary>
		public string SalesName { get; set; }

		/// <summary>
		/// 业务员编码
		/// </summary>
		public string SalesNumber { get; set; }

		/// <summary>
		/// 结算方式编码
		/// </summary>
		public string SettleTypeNumber { get; set; }

		/// <summary>
		/// 结算方式名称
		/// </summary>
		public string SettleTypeName { get; set; }

		/// <summary>
		/// 收退款日期
		/// </summary>
		public DateTime ReceiveDate { get; set; }

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustomerName { get; set; }

		/// <summary>
		/// 客户编码
		/// </summary>
		public string CustomerNumber { get; set; }

		/// <summary>
		/// 收款方式编码
		/// </summary>
		public string ReceiveNumber { get; set; }

		/// <summary>
		/// 收款方式名称
		/// </summary>
		public string ReceiveName { get; set; }

		/// <summary>
		/// 部门名称
		/// </summary>
		public string DepartmentName { get; set; }

		/// <summary>
		/// 部门编码
		/// </summary>
		public string DepartmentNumber { get; set; }

		/// <summary>
		/// 销售组织
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// 销售组织编码
		/// </summary>
		public string OrganizationNumber { get; set; }

		/// <summary>
		/// 收退款金额
		/// </summary>
		public decimal ReceiveAmount { get; set; }
	}
}
