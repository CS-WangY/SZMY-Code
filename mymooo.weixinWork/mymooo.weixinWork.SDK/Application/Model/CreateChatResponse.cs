using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
	/// <summary>
	/// 创建群聊响应
	/// </summary>
	public class CreateChatResponse
	{
		/// <summary>
		/// 0 为成功
		/// </summary>
		public int ErrCode { get; set; }

		/// <summary>
		/// 错误消息
		/// </summary>
		[JsonPropertyName("errmsg")]
		public string? ErrorMessage { get; set; }

		/// <summary>
		/// 群聊Id
		/// </summary>
		[JsonPropertyName("chatid")]
		public string ChatId { get; set; } = string.Empty;
	}
}
