namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批状态
	/// </summary>
	public enum ApprovalStatus
	{
		/// <summary>
		/// 审批中
		/// </summary>
		submit = 1,

		/// <summary>
		/// 审批
		/// </summary>
		approval = 2,

		/// <summary>
		/// 驳回
		/// </summary>
		reject = 3,

		/// <summary>
		/// 已撤销
		/// </summary>
		revocation = 4,

		/// <summary>
		/// 通过后撤销
		/// </summary>
		approvalRevocation = 5,

		/// <summary>
		/// 已删除
		/// </summary>
		delete = 6,

		/// <summary>
		/// 已支付
		/// </summary>
		pay = 7
	}
}
