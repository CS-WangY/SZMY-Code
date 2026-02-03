using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Utils;
using mymooo.weixinWork.Models;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Net.Mime;

namespace mymooo.weixinWork.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApprovalController(WeixinWorkContext weixinWorkContext, ApprovalServiceClient approvalServiceClient, ApplicationServiceClient applicationServiceClient) : Controller
    {
        private readonly WeixinWorkContext _weixinWorkContext = weixinWorkContext;
        private readonly ApprovalServiceClient _approvalServiceClient = approvalServiceClient;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取审批详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApproverDetailsResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetApprovalDetail([FromBody] GetApprovalDetailRequest request)
        {
            var result = await this.ModelVerify(request);
            if (!result.IsSuccess)
            {
                return Json(result);
            }
            return Json(_approvalServiceClient.GetApprovalDetail<WeixinWorkContext, User>(_weixinWorkContext, request));
        }

        /// <summary>
        /// 发送审批测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreditApproval()
        {
            CreditApprovalRequest request = new()
            {
                Date = DateTime.Now,
                CustNumber = "E1111",
                CustName = "E1111",
                SettleType = "月结60天",
                CreatorUserCode = "moyifeng",
                ApprovalCredit = 50000,
                ApprovalReason = "safsfsd",
                NotifyType = NotifyType.submitNotify,
                EnvCode = "develop_mo",
                SendRabbitCode = "mymooo_weixin_Approval_SettlementChange_develop_mo",
                MediaInfos = [new() { FileUrl = "https://image.mymooo.com/HomeImage/30d0bc30f161482bae80d130aea92ef8.jpg", FileName = "test.jpg", MediaType = WeixinMediaType.image }]
            };

            return Json(await _approvalServiceClient.Applyevent<CreditApprovalRequest, WeixinWorkContext, User>(_weixinWorkContext, request));
        }

        /// <summary>
        /// 发送文本消息测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SendTestMessage()
        {
            SendTextMessageRequest sendTextMessage = new()
            {
                ToUser = "moyifeng",
                Text = new SendTextMessageRequest.TextMessage()
                {
                    Content = "测试发送消息"
                }
            };

            await _approvalServiceClient.SendTextMessage<WeixinWorkContext, User>(_weixinWorkContext, sendTextMessage);
            await _applicationServiceClient.SendTextMessage<WeixinWorkContext, User>(_weixinWorkContext, sendTextMessage);

            return Ok();
        }

        /// <summary>
        /// 发送 Markdown 消息测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SendMarkdownMessage()
        {
            SendMarkdownMessageRequest sendTextMessage = new()
            {
                ToUser = "moyifeng",
                MarkDown = new SendMarkdownMessageRequest.MarkDownMessage()
                {
                    Content = "测试发送消息"
                }
            };

            await _approvalServiceClient.SendMarkDownMessage<WeixinWorkContext, User>(_weixinWorkContext, sendTextMessage);
            await _applicationServiceClient.SendMarkDownMessage<WeixinWorkContext, User>(_weixinWorkContext, sendTextMessage);

            return Ok();
        }
    }
}
