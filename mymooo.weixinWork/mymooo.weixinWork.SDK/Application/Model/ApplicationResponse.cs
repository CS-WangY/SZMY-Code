using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Application.Model
{
    /// <summary>
    /// 微信应用响应
    /// </summary>
    public class ApplicationResponse
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
    }
}
