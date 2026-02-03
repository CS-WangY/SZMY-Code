using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Report;
using mymooo.k3cloud.business.Services.Purchase;
using mymooo.k3cloud.core.ReportModel;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 客户关系查询未出货数接口
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class ReportController(
        ReportNotDeliverService reportNotDeliverService,
        ReportNotCheckAccountService reportNotCheckAccountService,
        ReportNotOpenAmountService reportNotOpenAmountService,
        ReportNotReceiveService reportNotReceiveService
        ) : Controller
    {
        private readonly ReportNotDeliverService _reportNotDeliverService = reportNotDeliverService;
        private readonly ReportNotCheckAccountService _reportNotCheckAccountService = reportNotCheckAccountService;
        private readonly ReportNotOpenAmountService _reportNotOpenAmountService = reportNotOpenAmountService;
        private readonly ReportNotReceiveService _reportNotReceiveService = reportNotReceiveService;

        /// <summary>
        /// 获取未出货汇总
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<NotDeliverTotals>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotDeliverTotals([FromBody] CrmReportRequestModel request)
        {
            return Json(_reportNotDeliverService.GetNotDeliverTotals(request));
        }
        /// <summary>
        /// 获取未出货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<NotDeliverList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotDeliverList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotDeliverService.GetNotDeliverList(request));
        }
        /// <summary>
        /// 获取客户未出货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<CustNotDeliverList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetCustNotDeliverList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotDeliverService.GetCustNotDeliverList(request));
        }

        /// <summary>
        /// 获取未对账合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<NotCheckAccountTotals>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotCheckAccountTotals([FromBody] CrmReportRequestModel request)
        {
            return Json(_reportNotCheckAccountService.GetNotCheckAccountTotals(request));
        }
        /// <summary>
        /// 获取未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<NotCheckAccountList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotCheckAccountList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotCheckAccountService.GetNotCheckAccountList(request));
        }
        /// <summary>
        /// 获取客户未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<CustNotCheckAccountList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetCustNotCheckAccountList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotCheckAccountService.GetCustNotCheckAccountList(request));
        }

        /// <summary>
        /// 获取未对账合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<NotOpenAmountTotals>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotOpenAccountTotals([FromBody] CrmReportRequestModel request)
        {
            return Json(_reportNotOpenAmountService.GetNotOpenAccountTotals(request));
        }
        /// <summary>
        /// 获取未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<NotOpenAmountList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotOpenAccountList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotOpenAmountService.GetNotOpenAccountList(request));
        }
        /// <summary>
        /// 获取客户未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<CustNotOpenAmountList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetCustNotOpenAccountList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotOpenAmountService.GetCustNotOpenAccountList(request));
        }

        /// <summary>
        /// 获取未收款核销合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<NotReceiveTotals>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotReceiveTotals([FromBody] CrmReportRequestModel request)
        {
            return Json(_reportNotReceiveService.GetNotReceiveTotals(request));
        }
        /// <summary>
        /// 获取未收款核销明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<NotReceiveList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNotReceiveList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotReceiveService.GetNotReceiveList(request));
        }
        /// <summary>
        /// 获取客户未收款核销明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<PageResponse<CustNotReceiveList>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetCustNotReceiveList([FromBody] PageRequest<CrmReportRequestModel> request)
        {
            return Json(_reportNotReceiveService.GetCustNotReceiveList(request));
        }
    }
}
