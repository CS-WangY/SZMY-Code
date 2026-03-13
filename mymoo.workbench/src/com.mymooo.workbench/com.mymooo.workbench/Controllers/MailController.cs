using com.mymooo.workbench.core.Expressage.DeBang;
using com.mymooo.workbench.core.Mail;
using com.mymooo.workbench.ef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Utils.JsonConverter;
using System.Collections.Generic;

namespace com.mymooo.workbench.Controllers
{
    public class MailController(ILogger<MailController> logger, MailClient mailClient) : Controller
    {
        private readonly ILogger<MailController> _logger = logger;
        private readonly MailClient _mailClient = mailClient;

        [AllowAnonymous]
        public IActionResult SendEmail([FromBody] MailMessageModel request)
        {
            if (request == null)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Code = ResponseCode.ModelError,
                    Message = "参数不能为空"
                });
            }
            
            if (request.AttachmentDatas != null)
            {
                List<string> attachmentsPathList = new List<string>();
                // 下载附件到本地
                foreach (var item in request.AttachmentDatas)
                {
                    if (string.IsNullOrWhiteSpace(item.AttachmentName))
                    {
                        return Json(new
                        {
                            IsSuccess = false,
                            Code = ResponseCode.ModelError,
                            Message = "发送失败，请给每个附件地址设置一个附件文件名(含后缀)"
                        });
                    }
                    string attachmentsUrl = _mailClient.DownloadAttachment(item);
                    attachmentsPathList.Add(attachmentsUrl);
                }
                request.AttachmentsPathList = attachmentsPathList;
            }
            
            // 发送邮件
            string sendReturn = _mailClient.SendEmail(request);
            if (sendReturn != null && sendReturn.Contains("Ok")) 
            {
                return Json(new
                {
                    IsSuccess = true,
                    Code = ResponseCode.Success,
                    Message = "发送成功"
                });
            }
            else
            {
                return Json(new
                {
                    IsSuccess = false,
                    Code = ResponseCode.ThirdpartyError,
                    Message = $@"发送失败{sendReturn}"
                });
            }
            
        }
    }
}
