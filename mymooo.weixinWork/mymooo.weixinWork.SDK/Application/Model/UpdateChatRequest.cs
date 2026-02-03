using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
	/// <summary>
	/// 修改群聊
	/// </summary>
	public class UpdateChatRequest
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
		/// 添加群成员
		/// </summary>
		[JsonPropertyName("add_user_list")]
		public string[] AddUserCodes { get; set; } = [];

		/// <summary>
		/// 删除群成员
		/// </summary>
		[JsonPropertyName("del_user_list")]
		public string[] DeleteUserCodes { get; set; } = [];

		/// <summary>
		/// 群聊Id
		/// </summary>
		[JsonPropertyName("chatid")]
		public string ChatId { get; set; } = string.Empty;
	}
}
