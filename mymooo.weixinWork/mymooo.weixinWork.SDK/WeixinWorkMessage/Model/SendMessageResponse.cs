using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
	/// <summary>
	/// 发送消息响应
	/// </summary>
	public class SendMessageResponse
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
		/// 无效用户
		/// </summary>
		public string? InvalidUser { get; set; }

		/// <summary>
		/// 无效部门
		/// </summary>
		public string? InvalidParty { get; set; }

		/// <summary>
		/// 无效标记
		/// </summary>
		public string? InvalidTag { get; set; }
	}
}
