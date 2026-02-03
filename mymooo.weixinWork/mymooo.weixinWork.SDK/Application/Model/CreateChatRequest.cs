using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
	/// <summary>
	/// 创建群聊请求
	/// </summary>
	public class CreateChatRequest
	{
		/// <summary>
		/// 群聊名称
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 群主
		/// </summary>
		public string Owner { get; set; } = string.Empty;

		/// <summary>
		/// 群成员
		/// </summary>
		[JsonPropertyName("userlist")]
		public string[] UserCodes { get; set; } = [];

		/// <summary>
		/// 群聊Id
		/// </summary>
		[JsonPropertyName("chatid")]
		public string ChatId { get; set; } = string.Empty;
	}

}
