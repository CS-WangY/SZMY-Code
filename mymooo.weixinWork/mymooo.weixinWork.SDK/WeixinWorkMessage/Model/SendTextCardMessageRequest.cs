using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
	/// <summary>
	/// 发送卡片消息
	/// </summary>
	public class SendTextCardMessageRequest : SendMessageRequest
	{
		/// <summary>
		/// 默认消息类型
		/// </summary>
		public SendTextCardMessageRequest()
		{
			this.MessageType = Enum.SendMessageType.textcard;
		}

		/// <summary>
		/// 文本卡片
		/// </summary>
        [JsonPropertyName("textcard")]
		public TextCardMessage TextCard { get; set; } = new();

		/// <summary>
		/// 文本卡片类
		/// </summary>
		public class TextCardMessage
		{
			/// <summary>
			/// 标题，不超过128个字节，超过会自动截断（支持id转译）
			/// </summary>
			public string Title { get; set; } = string.Empty;

			/// <summary>
			/// 描述，不超过512个字节，超过会自动截断（支持id转译）
			/// </summary>
			public string Description { get; set; } = string.Empty;

			/// <summary>
			/// 点击后跳转的链接。
			/// </summary>
			public string Url { get; set; } = string.Empty;

			/// <summary>
			/// 按钮文字。 默认为“详情”， 不超过4个文字，超过自动截断。
			/// </summary>
			public string Btntxt { get; set; } = string.Empty;
		}
	}
}
