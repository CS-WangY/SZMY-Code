using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
	/// <summary>
	/// 获取群聊内容
	/// </summary>
	public class GetChatResponse : ApplicationResponse
    {
		/// <summary>
		/// 群聊信息
		/// </summary>
		[JsonPropertyName("chat_info")]
		public CreateChatRequest? ChatInfo { get; set; }
	}
}
