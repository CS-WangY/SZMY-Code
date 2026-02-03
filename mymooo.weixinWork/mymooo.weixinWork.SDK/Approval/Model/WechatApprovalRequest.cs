using System.Text.Json.Serialization;
using mymooo.weixinWork.SDK.Approval.Model.Enum;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 提交到企业微信的审批请求信息
	/// </summary>
	public class WechatApprovalRequest : ApprovalRequest
	{
		/// <summary>
		/// 模板id。可在“获取审批申请详情”、“审批状态变化回调通知”中获得，也可在审批模板的模板编辑页面链接中获得。暂不支持通过接口提交[打卡补卡][调班]模板审批单。
		/// </summary>
		[JsonPropertyName("template_id")]
		public string TemplateId { get; set; } = string.Empty;

		/// <summary>
		/// 审批人模式：0-通过接口指定审批人、抄送人（此时approver、notifyer等参数可用）; 1-使用此模板在管理后台设置的审批流程，支持条件审批。
		/// </summary>
		[JsonPropertyName("use_template_approver")]
		public TemplateApproverModel UseTemplateApprover { get; set; } = TemplateApproverModel.interfaceType;

		/// <summary>
		/// 审批流程信息，用于指定审批申请的审批流程，支持单人审批、多人会签、多人或签，可能有多个审批节点，仅use_template_approver为0时生效。
		/// </summary>
		public List<ApproverNode> Approver { get; set; } = [];

		/// <summary>
		/// 审批申请数据，可定义审批申请中各个控件的值，其中必填项必须有值，选填项可为空，数据结构同“获取审批申请详情”接口返回值中同名参数“apply_data”
		/// </summary>
		[JsonPropertyName("apply_data")]
		public ApprovalDetails ApplyData { get; set; } = new();

		/// <summary>
		/// 摘要信息，用于显示在审批通知卡片、审批列表的摘要信息，最多3行
		/// </summary>
		[JsonPropertyName("summary_list")]
		public List<SummaryInfo> SummaryList
		{
			get
			{
				return SummaryInfo?.Select(p => new SummaryInfo() { SummaryInfos = [new ControlTitle { Text = p }] }).ToList() ?? [];
			}
		}
	}
}
