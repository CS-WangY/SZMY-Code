using mymooo.weixinWork.SDK.Approval.Model.Enum;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批节点
	/// </summary>
	public class ApproverNode
	{
		/// <summary>
		/// 节点审批方式：1-或签；2-会签，仅在节点为多人审批时有效
		/// </summary>
		public ApproverNodeModel Attr { get; set; }

		/// <summary>
		/// 审批节点审批人userid列表，若为多人会签、多人或签，需填写每个人的userid
		/// </summary>
		public List<string> Userid { get; set; } = [];
	}
}
