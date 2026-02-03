using mymooo.weixinWork.SDK.WeixinWorkMessage.Model.Enum;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
	/// <summary>
	/// 发送文本消息
	/// </summary>
	public class SendTextMessageRequest : SendMessageRequest
	{
		/// <summary>
		/// 默认消息类型
		/// </summary>
		public SendTextMessageRequest()
		{
			this.MessageType = SendMessageType.text;
		}

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public TextMessage Text { get; set; } = new();

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public class TextMessage
		{
			/// <summary>
			/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
			/// </summary>
			public string Content { get; set; } = string.Empty;
		}
	}
}
