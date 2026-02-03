using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;

namespace mymooo.k3cloud.core.Approver
{
	/// <summary>
	/// 物流方式变更申请
	/// </summary>
	[ApprovalTemplate("Bs2hnGNuTbVWtzKM5Wcte2voK4bbkPuFxPoo1BSyp")]
	public class K3cloudDnLogisticsChangesRequest : ApprovalRequest
	{
		/// <summary>
		/// DN单号
		/// </summary>
		public string DnNo { get; set; } = string.Empty;

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; } = string.Empty;

		/// <summary>
		/// 物流发货仓库
		/// </summary>
		public string Warehouse { get; set; } = string.Empty;

		/// <summary>
		/// 发往省份
		/// </summary>
		public string Province { get; set; } = string.Empty;

		/// <summary>
		/// 订单金额
		/// </summary>
		public decimal OrderAmount { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Remarks { get; set; } = string.Empty;

		/// <summary>
		/// 申请日期
		/// </summary>
		public DateTime ApplyDate { get; set; } = DateTime.Now;

		/// <summary>
		/// 物流方式
		/// </summary>
		public string LogisticsType { get; set; } = "";

		/// <summary>
		/// 付费方式
		/// </summary>
		public string PaymentType { get; set; } = "";

		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime? CompleteTime { get; set; }

		/// <summary>
		/// 审批单号
		/// </summary>
		public string ApprovalNo { get; set; } = string.Empty;

		/// <summary>
		/// 审核人
		/// </summary>
		public string AduitUserName { get; set; } = string.Empty;

	}
}
