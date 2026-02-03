using mymooo.weixinWork.SDK.Media.Model;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
    /// <summary>
    /// 发送文件消息
    /// </summary>
    public class SendFileMessageRequest : SendMessageRequest
	{
		/// <summary>
		/// 默认消息类型
		/// </summary>
		public SendFileMessageRequest()
		{
			this.MessageType = Enum.SendMessageType.file;
		}

		/// <summary>
		/// 文件下载路径
		/// </summary>
		public MediaInfo? MediaFile { get; set; }

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public FileMessage? File { get; set; }

		/// <summary>
		/// 文件类型
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeixinMediaType MediaType { get; internal set; }

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public class FileMessage
		{
			/// <summary>
			/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
			/// </summary>
			[JsonPropertyName("media_id")]
			public string MediaId { get; set; } = string.Empty;
		}
	}
}
