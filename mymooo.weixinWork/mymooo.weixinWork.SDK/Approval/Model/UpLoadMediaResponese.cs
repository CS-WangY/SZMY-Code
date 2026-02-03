using System.Text.Json.Serialization;
using mymooo.weixinWork.SDK.Application.Model;
using mymooo.weixinWork.SDK.Media.Model;

namespace mymooo.weixinWork.SDK.Approval.Model
{
    /// <summary>
    /// 上传素材响应
    /// </summary>
    public class UpLoadMediaResponese : ApplicationResponse
    {
		/// <summary>
		/// 媒体文件类型，分别有图片（image）、语音（voice）、视频（video），普通文件(file)
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeixinMediaType Type { get; set; }

		/// <summary>
		/// 媒体文件上传后获取的唯一标识，3天内有效
		/// </summary>
		[JsonPropertyName("media_id")]
		public string MediaId { get; set; } = string.Empty;

		/// <summary>
		/// 媒体文件上传时间戳
		/// </summary>
		[JsonPropertyName("created_at")]
		public string Created { get; set; } = string.Empty;
	}
}
