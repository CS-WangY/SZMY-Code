using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Mail
{
    public class MailMessageModel
    {
        /// <summary>
        /// 发件邮箱
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// 收件人地址(多人)
        /// </summary>
        public string[] RecipientArry { get; set; }

        /// <summary>
        /// 抄送地址(多人)
        /// </summary>
        public string[] MailCcArray { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string MailBody { get; set; }

        /// <summary>
        /// 客户端授权码(可存在配置文件中)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// SMTP邮件服务器
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 正文是否是html格式
        /// </summary>
        public bool IsbodyHtml { get; set; }
        /// <summary>
        /// 附件存储路径
        /// </summary>
        public List<string> AttachmentsPathList { get; set; }

        /// <summary>
        /// 附件url地址
        /// </summary>
        public List<AttachmentData> AttachmentDatas { get; set; }
    }

    /// <summary>
    /// 附件地址
    /// </summary>
    public class AttachmentData
    {
        // 文件名
        public string AttachmentName { get; set; }
        /// <summary>
        /// 文件下载地址
        /// </summary>
        public string Url { get; set; }
    }
}
