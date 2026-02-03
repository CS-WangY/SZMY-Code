using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批详情数据
	/// </summary>
	public class ApproverDetailsResponse
	{
		/// <summary>
		/// 为0表示正确
		/// </summary>
		public int Errcode { get; set; }

		/// <summary>
		/// 错误消息
		/// </summary>
		[JsonPropertyName("errmsg")]
		public string? ErrorMessage { get; set; }

		/// <summary>
		/// 审批信息
		/// </summary>
		public ApproverInfo? Info { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[RedisKey("mymooo-weixin-Approval", 14)]
		public class ApproverInfo
		{
			/// <summary>
			/// 审批编号
			/// </summary>
			[JsonPropertyName("sp_no")]
			[RedisPrimaryKey]
			public string SpNo { get; set; } = string.Empty;

			/// <summary>
			/// redis 标识
			/// </summary>
			[RedisMainField]
			public string RedisMain { get; set; } = "context";

			/// <summary>
			/// 审批申请类型名称（审批模板名称）
			/// </summary>
			[JsonPropertyName("sp_name")]
			public string SpName { get; set; } = string.Empty;

			/// <summary>
			///  	申请单状态：1-审批中；2-已通过；3-已驳回；4-已撤销；6-通过后撤销；7-已删除；10-已支付
			/// </summary>
			[JsonPropertyName("sp_status")]
			public int SpStatus { get; set; }

			/// <summary>
			/// 审批模板id。可在“获取审批申请详情”、“审批状态变化回调通知”中获得，也可在审批模板的模板编辑页面链接中获得。
			/// </summary>
			[JsonPropertyName("template_id")]
			public string TemplateId { get; set; } = string.Empty;

			/// <summary>
			/// 审批申请提交时间,Unix时间戳
			/// </summary>
			[JsonPropertyName("spply_time")]
			public int ApplyTime { get; set; }

			/// <summary>
			/// 申请人信息
			/// </summary>
			public Applyer Applyer { get; set; } = new();

			/// <summary>
			///  	审批流程信息，可能有多个审批节点。
			/// </summary>
			[JsonPropertyName("sp_record")]
			public SpRecord[] SpRecords { get; set; } = [];

			/// <summary>
			/// 抄送信息，可能有多个抄送节点
			/// </summary>
			public Notifyer[] Notifyer { get; set; } = [];

			/// <summary>
			/// 审批申请数据
			/// </summary>
			[JsonPropertyName("apply_data")]
			public Apply_Data ApplyData { get; set; } = new();

			/// <summary>
			/// 审批申请备注信息，可能有多个备注节点
			/// </summary>
			public Comment[] Comments { get; set; } = [];
		}

		/// <summary>
		/// 申请人信息
		/// </summary>
		public class Applyer
		{
			/// <summary>
			/// 申请人userid
			/// </summary>
			[JsonPropertyName("userid")]
			public string UserId { get; set; } = string.Empty ;

			/// <summary>
			/// 申请人所在部门id
			/// </summary>
			[JsonPropertyName("partyid")]
			public string PartyId { get; set; } = string.Empty;
		}

		/// <summary>
		/// 审批申请数据
		/// </summary>
		public class Apply_Data
		{
			/// <summary>
			/// 审批申请详情，由多个表单控件及其内容组成
			/// </summary>
			public Content[] Contents { get; set; } = [];
		}

		/// <summary>
		/// 审批申请详情，由多个表单控件及其内容组成
		/// </summary>
		public class Content
		{
			/// <summary>
			/// 控件类型：Text-文本；Textarea-多行文本；Number-数字；Money-金额；Date-日期/日期+时间；
			/// Selector-单选/多选；；Contact-成员/部门；Tips-说明文字；File-附件；Table-明细；Attendance-假勤；
			/// Vacation-请假；PunchCorrection-补卡;DateRange-时长
			/// </summary>
			public string Control { get; set; } = string.Empty;

			/// <summary>
			/// 控件id
			/// </summary>
			public string Id { get; set; } = string.Empty;

			/// <summary>
			/// 控件名称 ，若配置了多语言则会包含中英文的控件名称
			/// </summary>
			public Title[] Title { get; set; } = [];

			/// <summary>
			/// 控件值 ，包含了申请人在各种类型控件中输入的值，不同控件有不同的值，具体说明详见附录
			/// </summary>
			public ControlValue? Value { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		public class ControlValue
		{
			/// <summary>
			/// 文本值
			/// </summary>
			public string Text { get; set; } = string.Empty;

			/// <summary>
			/// 整数
			/// </summary>
			[JsonPropertyName("new_number")]
			public long Number { get; set; }

			/// <summary>
			/// 金额
			/// </summary>
			[JsonPropertyName("new_money")]
			public decimal Money { get; set; }

			/// <summary>
			/// 日期值
			/// </summary>
			public DateControlValue? Date { get; set; }

			/// <summary>
			/// 下拉框选项值
			/// </summary>
			public SelectorControlValue? Selector { get; set; }

			/// <summary>
			/// 成员列表
			/// </summary>
			public MemberControlValue[] Members { get; set; } = [];
		}

		/// <summary>
		/// 
		/// </summary>
		public class MemberControlValue
		{
			/// <summary>
			/// 
			/// </summary>
			[JsonPropertyName("userid")]
			public string UserId { get; set; } = string.Empty;

			/// <summary>
			/// 
			/// </summary>
			public string Name { get; set; } = string.Empty;
		}

		/// <summary>
		/// 下拉框选项值
		/// </summary>
		public class SelectorControlValue
		{
			/// <summary>
			/// 控件类型
			/// </summary>
			public string Type { get; set; } = string.Empty;

			/// <summary>
			/// 选项值
			/// </summary>
			public Option[] Options { get; set; } = [];

			/// <summary>
			/// 
			/// </summary>
			public class Option
			{
				/// <summary>
				/// 
				/// </summary>
				public string Key { get; set; } = string.Empty;

				/// <summary>
				/// 
				/// </summary>
				public OptionValue[] Value { get; set; } = [];
			}

			/// <summary>
			/// 
			/// </summary>
			public class OptionValue
			{
				/// <summary>
				/// 
				/// </summary>
				public string Text { get; set; } = string.Empty;

				/// <summary>
				/// 
				/// </summary>
				public string Lang { get; set; } = string.Empty;
			}

		}

		/// <summary>
		/// 日期控件值
		/// </summary>
		public class DateControlValue
		{
			/// <summary>
			/// 控件类型
			/// </summary>
			public string Type { get; set; } = string.Empty;

			/// <summary>
			/// 时间戳
			/// </summary>
			[JsonPropertyName("s_timestamp")]
			public long Timestamp { get; set; }

			/// <summary>
			/// 时区
			/// </summary>
			[JsonPropertyName("timezone_info")]
			public TimezoneInfo? Timezone { get; set; }

			/// <summary>
			/// 时区
			/// </summary>
			public class TimezoneInfo
			{
				/// <summary>
				/// 
				/// </summary>
				[JsonPropertyName("zone_offset")]
				public string ZoneOffset { get; set; } = string.Empty;

				/// <summary>
				/// 
				/// </summary>
				[JsonPropertyName("zone_desc")]
				public string Description { get; set; } = string.Empty;
			}
		}

		/// <summary>
		/// 控件名称 ，若配置了多语言则会包含中英文的控件名称
		/// </summary>
		public class Title
		{
			/// <summary>
			/// 控件名称
			/// </summary>
			public string Text { get; set; } = string.Empty;

			/// <summary>
			/// 语言
			/// </summary>
			public string Lang { get; set; } = string.Empty;
		}

		/// <summary>
		/// 审批流程信息，可能有多个审批节点。
		/// </summary>
		public class SpRecord
		{
			/// <summary>
			/// 审批节点状态：1-审批中；2-已同意；3-已驳回；4-已转审
			/// </summary>
			[JsonPropertyName("sp_status")]
			public int SpStatus { get; set; }

			/// <summary>
			/// 节点审批方式：1-或签；2-会签
			/// </summary>
			public int Approverattr { get; set; }

			/// <summary>
			/// 审批节点详情,一个审批节点有多个审批人
			/// </summary>
			public Detail[] Details { get; set; } = [];
		}

		/// <summary>
		/// 审批节点详情,一个审批节点有多个审批人
		/// </summary>
		public class Detail
		{
			/// <summary>
			/// 分支审批人
			/// </summary>
			public Approver? Approver { get; set; }

			/// <summary>
			/// 审批意见
			/// </summary>
			public string? Speech { get; set; }

			/// <summary>
			/// 分支审批人审批状态：1-审批中；2-已同意；3-已驳回；4-已转审
			/// </summary>
			[JsonPropertyName("Sp_status")]
			public int SpStatus { get; set; }

			/// <summary>
			/// 节点分支审批人审批操作时间戳，0表示未操作
			/// </summary>
			[JsonPropertyName("sptime")]
			public int SpTime { get; set; }

			/// <summary>
			/// 节点分支审批人审批意见附件，media_id具体使用请参考：文档-获取临时素材
			/// </summary>
			[JsonPropertyName("media_id")]
			public object[] MediaId { get; set; } = [];
		}

		/// <summary>
		///  	操作者
		/// </summary>
		public class Approver
		{
			/// <summary>
			/// 用户
			/// </summary>
			[JsonPropertyName("userid")]
			public string UserId { get; set; } = string.Empty;
		}

		/// <summary>
		/// 抄送信息，可能有多个抄送节点
		/// </summary>
		public class Notifyer
		{
			/// <summary>
			/// 节点抄送人userid
			/// </summary>
			[JsonPropertyName("userid")]
			public string UserId { get; set; } = string.Empty;
		}

		/// <summary>
		/// 审批申请备注信息，可能有多个备注节点
		/// </summary>
		public class Comment
		{
			/// <summary>
			/// 备注人信息
			/// </summary>
			public Commentuserinfo? CommentUserInfo { get; set; }

			/// <summary>
			/// 备注提交时间戳，Unix时间戳
			/// </summary>
			[JsonPropertyName("commenttime")]
			public int CommentTime { get; set; }

			/// <summary>
			/// 备注文本内容
			/// </summary>
			[JsonPropertyName("commentcontent")]
			public string? CommentContent { get; set; }

			/// <summary>
			/// 备注id
			/// </summary>
			[JsonPropertyName("commentid")]
			public string? CommentId { get; set; }

			/// <summary>
			/// 备注附件id，可能有多个，media_id具体使用请参考：文档-获取临时素材
			/// </summary>
			[JsonPropertyName("media_id")]
			public string[] MediaId { get; set; } = [];
		}

		/// <summary>
		/// 备注人信息
		/// </summary>
		public class Commentuserinfo
		{
			/// <summary>
			/// 备注人userid
			/// </summary>
			[JsonPropertyName("userid")]
			public string UserId { get; set; } = string.Empty;
		}
	}
}
