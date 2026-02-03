using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK
{
    /// <summary>
    /// 企业微信响应消息
    /// </summary>
    public class WeiXinMessageResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string? ErrorMessage { get; set; }
    }
}
