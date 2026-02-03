using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using mymooo.k3cloud.core.ReportModel;

namespace mymooo.k3cloud.SDK.Report
{
    /// <summary>
    /// 报表服务获取金蝶统计数据
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="httpService"></param>
    [AutoInject(InJectType.Single)]
    public class ReportServiceClient<C, U>(HttpService httpService) where U : UserBase, new() where C : MymoooContext<U>
    {
        private readonly HttpService _httpService = httpService;
        /// <summary>
        /// 获取未对账合计
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<NotCheckAccountTotals>> GetNotCheckAccountTotals(C mymoooContext, CrmReportRequestModel? request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, NotCheckAccountTotals>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotCheckAccountTotals", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未对账明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<NotCheckAccountList>>> GetNotCheckAccountList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<NotCheckAccountList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotCheckAccountList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取客户未对账明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<CustNotCheckAccountList>>> GetCustNotCheckAccountList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<CustNotCheckAccountList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetCustNotCheckAccountList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未出货汇总
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<NotDeliverTotals>> GetNotDeliverTotals(C mymoooContext, CrmReportRequestModel? request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, NotDeliverTotals>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotDeliverTotals", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未出货记录
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<NotDeliverList>>> GetNotDeliverList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<NotDeliverList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotDeliverList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取客户未出货记录
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<CustNotDeliverList>>> GetCustNotDeliverList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<CustNotDeliverList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetCustNotDeliverList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未开票合计
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<NotOpenAmountTotals>> GetNotOpenAccountTotals(C mymoooContext, CrmReportRequestModel? request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, NotOpenAmountTotals>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotOpenAccountTotals", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未开票明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<NotOpenAmountList>>> GetNotOpenAccountList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<NotOpenAmountList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotOpenAccountList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取客户未开票明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<CustNotOpenAmountList>>> GetCustNotOpenAccountList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<CustNotOpenAmountList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetCustNotOpenAccountList", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取未收款核销合计
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<NotReceiveTotals>> GetNotReceiveTotals(C mymoooContext, CrmReportRequestModel? request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, NotReceiveTotals>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotReceiveTotals", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取未收款核销明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<NotReceiveList>>> GetNotReceiveList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<NotReceiveList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetNotReceiveList", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 获取客户未收款核销明细
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ResponseMessage<PageResponse<CustNotReceiveList>>> GetCustNotReceiveList(C mymoooContext, PageRequest<CrmReportRequestModel> request)
        {
            if (request == null)
            {
                throw new ArgumentException($"“{nameof(request)}”不能为 null 或空白。", nameof(request));
            }
            return await _httpService.InvokeWebServiceAsync<C, U, PageResponse<CustNotReceiveList>>(mymoooContext, $"k3cloudapi/{mymoooContext.ApigatewayConfig.EnvCode}/Report/GetCustNotReceiveList", JsonSerializerOptionsUtils.Serialize(request));
        }
    }
}
