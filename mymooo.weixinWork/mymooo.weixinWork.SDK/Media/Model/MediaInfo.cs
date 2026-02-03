using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Media.Model
{
    /// <summary>
    /// 素材信息
    /// </summary>
    public class MediaInfo
    {
        /// <summary>
        /// 系统编码
        /// </summary>
        internal string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 环境变量
        /// </summary>
        internal string EnvCode { get; set; } = string.Empty;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; } = string.Empty;

		/// <summary>
		/// 媒体文件类型，分别有图片（image）、语音（voice）、视频（video），普通文件（file）
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
        public WeixinMediaType MediaType { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string? FileStream { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// 文件Id
        /// </summary>
        public string? MediaId { get; set; }

        /// <summary>
        /// 文件字节流
        /// </summary>
        public byte[]? File { get; set; }
    }
}
