using mymooo.weixinWork.SDK.WeixinWorkMessage.Model.Enum;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
	/// <summary>
	/// 发送消息请求
	/// </summary>
	public class SendMessageRequest
	{
		/// <summary>
		/// 指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。
		/// 特殊情况：指定为”@all”，则向该企业应用的全部成员发送
		/// </summary>
		[JsonPropertyName("touser")]
		public string? ToUser { get; set; }

		/// <summary>
		/// 群聊会话Id
		/// </summary>
		[JsonPropertyName("chatid")]
		public string? ChatId { get; set; }

		/// <summary>
		/// 指定接收消息的部门，部门ID列表，多个接收者用‘|’分隔，最多支持100个。
		/// 当touser为”@all”时忽略本参数
		/// </summary>
		[JsonPropertyName("toparty")]
		public string? ToParty { get; set; }

		/// <summary>
		/// 指定接收消息的标签，标签ID列表，多个接收者用‘|’分隔，最多支持100个。
		/// 当touser为”@all”时忽略本参数
		/// </summary>
		[JsonPropertyName("totag")]
		public string? ToTag { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        [JsonPropertyName("msgtype")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SendMessageType MessageType { get; internal set; }

        /// <summary>
        ///  	企业应用的id，整型。企业内部开发，可在应用的设置页面查看；第三方服务商，可通过接口 获取企业授权信息 获取该参数值
        /// </summary>
        [JsonPropertyName("agentid")]
        public long? AgentId { get; internal set; }

        /// <summary>
        /// 表示是否是保密消息，0表示否，1表示是，默认0
        /// </summary>
        public int Safe { get; set; }

        /// <summary>
        /// 表示是否开启id转译，0表示否，1表示是，默认0
        /// </summary>
        [JsonPropertyName("enable_id_trans")]
        public int EnableIdTrans { get; set; }

        /// <summary>
        ///  	表示是否开启重复消息检查，0表示否，1表示是，默认0
        /// </summary>
        [JsonPropertyName("enable_duplicate_check")]
        public int EnableDuplicateCheck { get; set; }

        /// <summary>
        /// 表示是否重复消息检查的时间间隔，默认1800s，最大不超过4小时
        /// </summary>
        [JsonPropertyName("duplicate_check_interval")]
        public int DuplicateCheckInterval { get; set; }
    }
}
