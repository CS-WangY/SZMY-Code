namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批回调发送MQ
	/// </summary>
	public class ApprovalMessageRequest
	{
		/// <summary>
		/// 审批单号
		/// </summary>
		public string ApplyeventNo { get; set; } = string.Empty;

		/// <summary>
		/// 环境变量
		/// </summary>
		public string EnvCode { get; set; } = string.Empty;

		/// <summary>
		/// 审批状态
		/// </summary>
		public string SpStatus { get; set; } = "1";

		/// <summary>
		/// 发起审批的请求数据
		/// </summary>
		public DateTime ApprovalDate { get; set; }

		/// <summary>
		/// 审批用户
		/// </summary>
		public string AduitUserName { get; set; } = string.Empty;

		/// <summary>
		/// 审批用户编码
		/// </summary>
		public string AduitUserCode { get; set; } = string.Empty;

		/// <summary>
		/// 最后一个人的审批意见
		/// </summary>
		public string? Speech { get; set; }
	}
}
