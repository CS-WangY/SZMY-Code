using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Utils;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.Approver;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 审批应用
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApprovalController(ApprovalServiceClient approvalServiceClient, KingdeeContent kingdeeContent, SaleOrderService saleOrderService) : Controller
    {
        private readonly ApprovalServiceClient _approvalServiceClient = approvalServiceClient;
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        private readonly SaleOrderService _saleOrderService = saleOrderService;
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
            return Json(await _approvalServiceClient.GetApprovalDetail<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendTextMessage([FromBody] SendTextMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendTextMessage<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 发送 Markdown 消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendMarkdownMessage([FromBody] SendMarkdownMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendMarkDownMessage<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 发送 卡片 消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendtextcardMessage([FromBody] SendTextCardMessageRequest request)
        {
            return Json(await _approvalServiceClient.SendTextcardMessage<KingdeeContent, User>(_kingdeeContent, request));
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
            var response = await _approvalServiceClient.Applyevent<K3cloudDnLogisticsChangesRequest, KingdeeContent, User>(_kingdeeContent, request);
            if (response.IsSuccess && response.Data?.Errcode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = response.Data.ErrorMessage;
            }
            return Json(response);
        }

        /// <summary>
        /// 加急发货申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> K3cloudDnUrgentShipmentApproval([FromBody] K3cloudDnUrgentShipmentRequest request)
        {
            var response = await _approvalServiceClient.Applyevent<K3cloudDnUrgentShipmentRequest, KingdeeContent, User>(_kingdeeContent, request);
            if (response.IsSuccess && response.Data?.Errcode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = response.Data.ErrorMessage;
            }
            return Json(response);
        }

        /// <summary>
        /// 获取审批流信息
        /// </summary>
        /// <param name="applyeventno"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<K3CloudClosedSalOrderRequest>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult K3CloudGetApproval(string applyeventno)
        {
            ResponseMessage<K3CloudClosedSalOrderRequest> responseMessage = new()
            {
                Data = _kingdeeContent.WorkbenchRedisCache.HashGet(new K3CloudClosedSalOrderRequest { ApplyeventNo = applyeventno })
            };
            return Json(responseMessage);
        }
        /// <summary>
        /// 获取关闭预测单审批信息
        /// </summary>
        /// <param name="applyeventno"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<K3CloudClosedForecastOrderRequest>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult K3CloudGetForecastApproval(string applyeventno)
        {
            ResponseMessage<K3CloudClosedForecastOrderRequest> responseMessage = new()
            {
                Data = _kingdeeContent.WorkbenchRedisCache.HashGet(new K3CloudClosedForecastOrderRequest { ApplyeventNo = applyeventno })
            };
            return Json(responseMessage);
        }

        /// <summary>
        /// 金蝶审批关闭销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> K3CloudClosedSalBillApproval([FromBody] K3CloudClosedSalOrderRequest request)
        {
            //查询数据获取附件
            request.AttachmentMaterials.Add(
                new MediaInfo
                {
                    FileName = "取消销售订单附件.xlsx",
                    MediaType = WeixinMediaType.file,
                    File = _saleOrderService.GetSaleOrderExcel(request.SaleOrderEntrys)
                });
            var response = await _approvalServiceClient.Applyevent<K3CloudClosedSalOrderRequest, KingdeeContent, User>(_kingdeeContent, request);
            if (response.IsSuccess && response.Data?.Errcode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = response.Data.ErrorMessage;
            }
            return Json(response);
        }

        /// <summary>
        /// 金蝶审批关闭预测单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ApplyeventResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> K3CloudClosedForecastBillApproval([FromBody] K3CloudClosedForecastOrderRequest request)
        {
            var response = await _approvalServiceClient.Applyevent<K3CloudClosedForecastOrderRequest, KingdeeContent, User>(_kingdeeContent, request);
            if (response.IsSuccess && response.Data?.Errcode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = response.Data.ErrorMessage;
            }
            return Json(response);
        }
    }
}
