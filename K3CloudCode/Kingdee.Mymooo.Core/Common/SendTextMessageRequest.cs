using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kingdee.Mymooo.Core.Common
{
    public class SendMessageRequest
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string ChatId { get; set; }
        //
        // 摘要:
        //     指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。 特殊情况：指定为”@all”，则向该企业应用的全部成员发送
        public string ToUser { get; set; }

        //
        // 摘要:
        //     指定接收消息的部门，部门ID列表，多个接收者用‘|’分隔，最多支持100个。 当touser为”@all”时忽略本参数
        public string ToParty { get; set; }

        //
        // 摘要:
        //     指定接收消息的标签，标签ID列表，多个接收者用‘|’分隔，最多支持100个。 当touser为”@all”时忽略本参数
        public string ToTag { get; set; }

        //
        // 摘要:
        //     消息类型
        public SendMessageType MessageType { get; set; }

        //
        // 摘要:
        //     企业应用的id，整型。企业内部开发，可在应用的设置页面查看；第三方服务商，可通过接口 获取企业授权信息 获取该参数值
        public long AgentId { get; set; }

        //
        // 摘要:
        //     表示是否是保密消息，0表示否，1表示是，默认0
        public int Safe { get; set; }

        //
        // 摘要:
        //     表示是否开启id转译，0表示否，1表示是，默认0
        public int EnableIdTrans { get; set; }

        //
        // 摘要:
        //     表示是否开启重复消息检查，0表示否，1表示是，默认0
        public int EnableDuplicateCheck { get; set; }

        //
        // 摘要:
        //     表示是否重复消息检查的时间间隔，默认1800s，最大不超过4小时
        public int DuplicateCheckInterval { get; set; }
    }
    /// <summary>
    /// 发送企业微信消息
    /// </summary>
    public class SendTextMessageRequest
    {
        /// <summary>
        /// 默认消息类型
        /// </summary>
        /// <summary>
        /// 消息类型，此时固定为：text
        /// </summary>
        public string msgtype = "text";

        /// <summary>
        /// 指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。
        /// 特殊情况：指定为”@all”，则向该企业应用的全部成员发送
        /// </summary>
        public string touser { get; set; }
        /// <summary>
        /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        /// </summary>
        public Text text { get; set; }

        /// <summary>
        /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        /// </summary>
        public class Text
        {
            /// <summary>
            /// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
            /// </summary>
            public string content { get; set; }
        }
    }

    public class SendMarkdownMessageRequest : SendMessageRequest
    {
        //
        // 摘要:
        //     消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        public class MarkDownMessage
        {
            //
            // 摘要:
            //     消息内容，最长不超过2048个字节，超过将截断（支持id转译）
            public string Content { get; set; }
        }

        //
        // 摘要:
        //     消息内容，最长不超过2048个字节，超过将截断（支持id转译）
        public MarkDownMessage MarkDown { get; set; }

        //
        // 摘要:
        //     默认消息类型
        public SendMarkdownMessageRequest()
        {
            base.MessageType = SendMessageType.markdown;
        }
    }


    public enum SendMessageType
    {
        //
        // 摘要:
        //     文本
        text,
        //
        // 摘要:
        //     卡片
        textcard,
        //
        // 摘要:
        //     图片
        image,
        //
        // 摘要:
        //     语音
        voice,
        //
        // 摘要:
        //     视频
        video,
        //
        // 摘要:
        //     文件
        file,
        //
        // 摘要:
        //     图文
        news,
        //
        // 摘要:
        //     mpnews类型的图文消息，跟普通的图文消息一致，唯一的差异是图文内容存储在企业微信。 多次发送mpnews，会被认为是不同的图文，阅读、点赞的统计会被分开计算。
        mpnews,
        //
        // 摘要:
        //     markdown消息 标题 （支持1至6级标题，注意#与文字中间要有空格） # 标题一 ## 标题二 ### 标题三 #### 标题四 ##### 标题五
        //     ###### 标题六 加粗 **bold** 链接 [这是一个链接](http://work.weixin.qq.com/api/doc) 行内代码段（暂不支持跨行）
        //     `code` 引用 > 引用文字 字体颜色(只支持3种内置颜色) 绿色 灰色 橙红色
        markdown
    }
}
