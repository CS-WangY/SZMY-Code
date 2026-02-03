using mymooo.core.Attributes.Redis;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批请求基类
	/// </summary>
	[RedisKey("mymooo-weixin-Approval", 14)]
	public class ApprovalRequest
	{
		/// <summary>
		/// 发起人
		/// </summary>
		[JsonPropertyName("creator_userid")]
		public string CreatorUserCode { get; set; } = string.Empty;

		/// <summary>
		/// 抄送方式：1-提单时抄送（默认值）； 2-单据通过后抄送；3-提单和单据通过后抄送
		/// </summary>
		[JsonPropertyName("notify_type")]
		public NotifyType NotifyType { get; set; } = NotifyType.approvalNotify;

		/// <summary>
		/// 抄送人节点userid列表，仅use_template_approver为0时生效。
		/// </summary>
		public List<string> Notifyer { get; set; } = [];

		/// <summary>
		/// 备注信息
		/// </summary>
		public List<string> SummaryInfo { get; set; } = [];

		/// <summary>
		/// 审批单号
		/// </summary>
		[RedisPrimaryKey]
		public string? ApplyeventNo { get; set; }

		/// <summary>
		/// 审批回调发送的mq code
		/// </summary>
		[RedisValue]
		public string SendRabbitCode { get; set; } = string.Empty;

		/// <summary>
		/// 环境变量
		/// </summary>
		[RedisValue]
		public string EnvCode { get; set; } = string.Empty;

		/// <summary>
		/// 审批状态
		/// </summary>
		[RedisValue]
		[JsonIgnore]
		public string SpStatus { get; set; } = "1";
	}
}
