using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model
{
    /// <summary>
    /// 发送MarkDown信息
    /// </summary>
    public class SendMarkdownMessageRequest : SendMessageRequest
    {
        /// <summary>
        /// 默认消息类型
        /// </summary>
        public SendMarkdownMessageRequest()
        {
            this.MessageType = Enum.SendMessageType.markdown;
        }

        /// <summary>
        /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        /// </summary>
        [JsonPropertyName("markdown")]
        public MarkDownMessage MarkDown { get; set; } = new();

        /// <summary>
        /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        /// </summary>
        public class MarkDownMessage
        {
            private string content = string.Empty;
            /// <summary>
            /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
            /// </summary>
            public string Content
            {
                get
                {
                    return content;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        content = string.Empty;
                    }
                    else
                    {
                        if (value.Length >= 2048)
                        {
                            content = value[..2048];
                        }
                        else
                        {
                            content = value;
                        }
                    }
                }
            }
        }
    }
}
