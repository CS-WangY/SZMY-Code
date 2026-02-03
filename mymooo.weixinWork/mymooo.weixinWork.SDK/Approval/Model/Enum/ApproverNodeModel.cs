namespace mymooo.weixinWork.SDK.Approval.Model.Enum
{
	/// <summary>
	/// 审批流程信息，用于指定审批申请的审批流程，支持单人审批、多人会签、多人或签，可能有多个审批节点，仅use_template_approver为0时生效。
	/// </summary>
	public enum ApproverNodeModel
	{

		/// <summary>
		/// 空
		/// </summary>
		Null = 0,

		/// <summary>
		/// 或签
		/// </summary>
		Or = 1,

		/// <summary>
		/// 会签
		/// </summary>
		And = 2
	}
}
