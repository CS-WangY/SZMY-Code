using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Config;
using mymooo.weixinWork.SDK.Media;
using mymooo.weixinWork.SDK.Utils;
using mymooo.weixinWork.SDK.WeixinWorkMessage;
using System.ComponentModel.DataAnnotations;

namespace mymooo.weixinWork.SDK.Approval
{
    /// <summary>
    /// 审批应用
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class ApprovalServiceClient(WeixinWorkHttpService httpService, ApprovalUtils approvalUtils, MediaServiceClient mediaServiceClient, IOptions<WeiXinWorkConfig> weiXinWorkConfig, WorkbenchRedisCache workbenchRedisCache)
        : SendMessageService(httpService, mediaServiceClient, "weixinwork-Approval", "production", weiXinWorkConfig.Value.ApprovalAgentId)
    {
        private readonly ApprovalUtils _approvalUtils = approvalUtils;
        private readonly WorkbenchRedisCache _workbenchRedisCache = workbenchRedisCache;

        /// <summary>
        /// 获取审批详情
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <param name="isAlongGet">始终从企业微信获取</param>
        /// <returns></returns>
        public async Task<ResponseMessage<ApproverDetailsResponse>> GetApprovalDetail<C, U>(C mymoooContext, GetApprovalDetailRequest request, bool isAlongGet = false) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.SpNo);
            if (isAlongGet)
            {
                return await GetApprovalInfo<C, U>(mymoooContext, request);
            }
            var approverInfo = _workbenchRedisCache.HashGet<ApproverDetailsResponse.ApproverInfo>(new() { SpNo = request.SpNo });
            if (approverInfo == null)
            {
                return await GetApprovalInfo<C, U>(mymoooContext, request);
            }
            else
            {
                ResponseMessage<ApproverDetailsResponse> response = new()
                {
                    Data = new ApproverDetailsResponse() { Info = approverInfo }
                };
                return response;
            }
        }

        /// <summary>
        /// 提交审批信息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<ApplyeventResponse>> Applyevent<Q, C, U>(C mymoooContext, Q request) where Q : ApprovalRequest, new() where U : UserBase, new() where C : MymoooContext<U>
        {
            var weixinRequest = await _approvalUtils.CreateApprovalRequest(request);
            var response = await _httpService.InvokeWebServiceAsync<C, U, ApplyeventResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/oa/applyevent", JsonSerializerOptionsUtils.Serialize(weixinRequest));
            if (response.IsSuccess && response.Data != null && response.Data.Errcode == 0)
            {
                request.ApplyeventNo = response.Data.SpNo;
                _workbenchRedisCache.HashSet(request);
            }

            return response;
        }

        /// <summary>
        /// 获取审批应用模板
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<dynamic>> GetTemplateFields<C, U>(C mymoooContext, [Required] string templateId) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, dynamic>(mymoooContext, $"{_systemCode}/production/cgi-bin/oa/gettemplatedetail", JsonSerializerOptionsUtils.Serialize(new { template_id = templateId }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U">用户</typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request">请求数据</param>
        /// <returns></returns>
        public async Task<ResponseMessage<ApproverDetailsResponse>> GetApprovalInfo<C, U>(C mymoooContext, GetApprovalDetailRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            var response = await _httpService.InvokeWebServiceAsync<C, U, ApproverDetailsResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/oa/getapprovaldetail", JsonSerializerOptionsUtils.Serialize(request));
            if (response.IsSuccess && response.Data != null && response.Data.Errcode == 0)
            {
                _workbenchRedisCache.HashSet(response.Data.Info);
            }
            return response;
        }

        /// <summary>
        /// 下载媒体
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="mediaId">媒体Id</param>
        /// <returns></returns>
        public async Task<byte[]> DownLoadMediaAsync<C, U>(C mymoooContext, string mediaId) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _mediaServiceClient.DownLoadMediaAsync<C, U>(mymoooContext, _systemCode, _envCode, mediaId);
        }
    }
}
