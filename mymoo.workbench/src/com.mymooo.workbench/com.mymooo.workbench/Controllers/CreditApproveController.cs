using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.WeiXinWork.Approver;
using com.mymooo.workbench.core.WeiXinWork.Approver.Credit;
using com.mymooo.workbench.core.WeiXinWork.Approver.Crm;
using com.mymooo.workbench.core.WeiXinWork.Approver.Scm;
using com.mymooo.workbench.ef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 信用管理审批接口
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class CreditApproveController(WorkbenchDbContext workbenchDbContext, WorkbenchContext workbenchContext, ApprovalServiceClient approvalServiceClient, MediaServiceClient mediaServiceClient) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly ApprovalServiceClient _approvalServiceClient = approvalServiceClient;
        private readonly MediaServiceClient _mediaServiceClient = mediaServiceClient;

        /// <summary>
        /// 审批应用发送卡片信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendtextcardMessage([FromBody] SendTextCardMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendTextcardMessage<WorkbenchContext, User>(_workbenchContext, request));
        }

        /// <summary>
        /// 审批应用发送文本信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendTextMessage([FromBody] SendTextMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendTextMessage<WorkbenchContext, User>(_workbenchContext, request));
        }

        /// <summary>
        /// 审批应用发送MarkDown信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendMarkDownMessage([FromBody] SendMarkdownMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendMarkDownMessage<WorkbenchContext, User>(_workbenchContext, request));
        }

        private async Task<IActionResult> Approval<T>(T request) where T : ApprovalRequest, new()
        {
            request.EnvCode ??= "production";
            if (request.SummaryInfo.Count > 3)
            {
                request.SummaryInfo = request.SummaryInfo.Take(3).ToList();
            }
            if (request?.Notifyer != null && request.Notifyer.Any())
            {
                request.Notifyer = _workbenchDbContext.User.Where(it => request.Notifyer.Contains(it.UserId) && it.Status == 1 && !it.IsDelete).Select(it => it.UserId).ToList();

            }

            var response = await _approvalServiceClient.Applyevent<T, WorkbenchContext, User>(_workbenchContext, request);
            if (response.IsSuccess && response.Data.Errcode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = response.Data.ErrorMessage;
            }
            return Json(response);
        }

        /// <summary>
        /// 发货申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DeliveryApproval([FromBody] DeliveryRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 申请变更单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ChangeOrderApply([FromBody] ApplyChangeOrderRequest request)
        {
            if (request.SalesMans != null && request.SalesMans.Count != 0)
            {
                foreach (var item in request.SalesMans)
                {
                    var re = _workbenchDbContext.User.Where(it => it.UserId == item && !it.IsDelete && it.Status == 1);
                    if (re.Any())
                    {
                        request.CreatorUserCode = item;
                        break;
                    }
                }
            }

            return await Approval(request);
        }

        /// <summary>
        /// 取消销售订单的审批
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        // [IgnoreLog]
        public async Task<IActionResult> CancelSalesOrder([FromBody] CancelSalesOrderRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 样品单申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        //  [IgnoreLog]
        public async Task<IActionResult> SampleApply([FromBody] ApplySampleRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 客户转移
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CustomerTransfer([FromBody] CustomerTransferRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<List<string>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        [IgnoreLog(true)]
        public async Task<IActionResult> UploadFile([FromBody] List<MediaInfo> mediaInfos)
        {
            return Json(await _approvalServiceClient.UpLoadMediaAsync<WorkbenchContext, User>(_workbenchContext, mediaInfos));
        }

        /// <summary>
        /// 备库申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ReserveApply([FromBody] ReserveApplyRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 备货申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> StockApply([FromBody] StockApplyRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> VisitApply([FromBody] CustomerVisitApplyDto request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ApplyPaymentToDelivery([FromBody] ApplyPaymentToDeliveryRequest request)
        {
            return await Approval(request);
        }
        /// <summary>
        /// 产品直发申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ApplyDirectDelivery([FromBody] ApplyDirectdeliveryRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 分解单大金额订单批核
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ApplyResolveSecondaryAudit([FromBody] ResolveItemAuditRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 物流方式变更申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> K3cloudDnLogisticsChangesApproval([FromBody] K3cloudDnLogisticsChangesRequest request)
        {
            return await Approval(request);
        }

        /// <summary>
        /// 获取审批详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApproverDetailsResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetApprovalDetail([FromBody] GetApprovalDetailRequest request)
        {
            return Json(_approvalServiceClient.GetApprovalDetail<WorkbenchContext, User>(_workbenchContext, request).Result);
        }


    }
}
