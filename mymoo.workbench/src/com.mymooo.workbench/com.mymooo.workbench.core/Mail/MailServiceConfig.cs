using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Mail
{
    public class MailServiceConfig
    {
        /// <summary>
        /// 发送人
        /// </summary>
        public string SenderEmail { get; set; }
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
    }
}
