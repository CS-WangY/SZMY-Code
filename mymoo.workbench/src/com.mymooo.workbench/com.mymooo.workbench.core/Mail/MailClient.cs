using com.mymooo.workbench.core.Utils;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using mymooo.core.Attributes;

namespace com.mymooo.workbench.core.Mail
{
    [AutoInject(InJectType.Scope)]
    public class MailClient
    {
        private readonly MailServiceConfig _mailServiceConfig;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailClient(IOptions<MailServiceConfig> mailServiceConfig, IWebHostEnvironment webHostEnvironment)
        {
            _mailServiceConfig = mailServiceConfig.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailMessageModel"></param>
        /// <returns></returns>
        public string SendEmail(MailMessageModel mailMessageModel)
        {
            // 发件人配置如果为空，则使用配置文件的默认配置
            if (!string.IsNullOrWhiteSpace(mailMessageModel.SenderEmail) && !string.IsNullOrWhiteSpace(mailMessageModel.Code))
            {
                //截取发件人邮箱地址从而判断Smtp的值
                string[] sArray = mailMessageModel.SenderEmail.Split(['@', '.']);
                if (sArray[1] == "qq")
                {
                    mailMessageModel.Host = "smtp.qq.com";
                }
                else if (sArray[1] == "163")
                {
                    mailMessageModel.Host = "smtp.163.com";
                }
            }
            else
            {
                mailMessageModel.SenderEmail = _mailServiceConfig.SenderEmail;
                mailMessageModel.Host = _mailServiceConfig.Host;
                mailMessageModel.Code = _mailServiceConfig.Code;
                mailMessageModel.Port = _mailServiceConfig.Port;
            }

            // 发件人
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mailMessageModel.SenderEmail, mailMessageModel.SenderEmail));
            //收件人
            List<InternetAddress> AcceptEmailAddressList = new List<InternetAddress>();

            //判断收件人数组中是否有数据
            if (mailMessageModel.RecipientArry != null && mailMessageModel.RecipientArry.Any())
            {
                //循环添加收件人地址
                foreach (var item in mailMessageModel.RecipientArry)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        AcceptEmailAddressList.Add(new MailboxAddress(item.ToString(), item.ToString()));
                    }
                }
            }

            //判断抄送地址数组是否有数据
            if (mailMessageModel.MailCcArray != null && mailMessageModel.MailCcArray.Any())
            {
                //循环添加抄送地址
                foreach (var item in mailMessageModel.MailCcArray)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        AcceptEmailAddressList.Add(new MailboxAddress(item.ToString(), item.ToString()));
                    }
                }
            }
            message.To.AddRange(AcceptEmailAddressList);
            // 主题
            message.Subject = mailMessageModel.Subject;
            var builder = new BodyBuilder();

            // 正文
            builder.TextBody = mailMessageModel.MailBody;

            // 附件
            if (mailMessageModel.AttachmentsPathList != null)
            {
                foreach (var item in mailMessageModel.AttachmentsPathList)
                {
                    builder.Attachments.Add(item);
                }
            }
            
            //合并消息
            message.Body = builder.ToMessageBody();

            // 发送邮件
            string sendReturn = "";
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(mailMessageModel.Host, mailMessageModel.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                smtpClient.Authenticate(mailMessageModel.SenderEmail, mailMessageModel.Code);
                sendReturn = smtpClient.Send(message);
                smtpClient.DisconnectAsync(true);
            }
            return sendReturn;
        }

        /// <summary>
        /// 下载附件到本地
        /// </summary>
        /// <returns></returns>
        public string DownloadAttachment(AttachmentData attachmentData)
        {
            byte[] fileData = HttpUtils.DownloadFile(attachmentData.Url);
            MemoryStream memoryStream = new MemoryStream(fileData);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath == null ? _webHostEnvironment.ContentRootPath : _webHostEnvironment.WebRootPath, "EmailAttachments");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string fullFilePath = Path.Combine(filePath, attachmentData.AttachmentName);
            using (FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                int size = 2048;
                byte[] by = new byte[2048];
                while (true)
                {
                    size = memoryStream.Read(by, 0, by.Length);
                    if (size > 0)
                    {
                        fs.Write(by, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            memoryStream.Close();
            return fullFilePath;
        }
    }
}
