using com.mymooo.api.gateway.SDK;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.business.WeixinWork.Approval
{
    /// <summary>
    /// 审批服务
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class ApprovalService(WorkbenchContext mymoooContext, RabbitMQServiceClient rabbitMQServiceClient, ApprovalServiceClient approvalServiceClient, WorkbenchDbContext workbenchDbContext, WorkbenchContext workbenchContext)
    {
        private readonly WorkbenchContext _mymoooContext = mymoooContext;
        private readonly RabbitMQServiceClient _rabbitMQServiceClient = rabbitMQServiceClient;
        private readonly ApprovalServiceClient _approvalServiceClient = approvalServiceClient;
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

        /// <summary>
        /// 企业微信审批回调
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WeiXinCallBack(dynamic message)
        {
            ApprovalRequest approvalRequest = new() { ApplyeventNo = message.ApprovalInfo.SpNo };
            var detials = await _approvalServiceClient.GetApprovalDetail<WorkbenchContext, User>(_workbenchContext, new GetApprovalDetailRequest() { SpNo = approvalRequest.ApplyeventNo }, true);
            if (Convert.ToInt32(message.ApprovalInfo.SpStatus) != 1)
            {
                var sendRabbitCode = _mymoooContext.WorkbenchRedisCache.HashGet(approvalRequest, p => p.SendRabbitCode);
                if (!string.IsNullOrWhiteSpace(sendRabbitCode))
                {
                    ApprovalMessageRequest request = new()
                    {
                        ApplyeventNo = message.ApprovalInfo.SpNo,
                        SpStatus = message.ApprovalInfo.SpStatus,
                        EnvCode = _mymoooContext.WorkbenchRedisCache.HashGet(approvalRequest, p => p.EnvCode),
                    };
                    if (request.SpStatus == "2")
                    {
                        var sprecord = detials.Data.Info.SpRecords.LastOrDefault().Details.LastOrDefault(it => it.SpStatus == 2);
                        request.ApprovalDate = DateTimeUtility.ConvertToDateTime(sprecord.SpTime);
                        request.AduitUserCode = sprecord.Approver.UserId;
                        request.Speech = sprecord.Speech;

                    }
                    else if (request.SpStatus == "3")
                    {
                        var sprecord = detials.Data.Info.SpRecords.FirstOrDefault(it => it.SpStatus == 3).Details.FirstOrDefault(it => it.SpStatus == 3);
                        request.ApprovalDate = DateTimeUtility.ConvertToDateTime(sprecord.SpTime);
                        request.AduitUserCode = sprecord.Approver.UserId;
                        request.Speech = sprecord.Speech;
                    }
                    else if (request.SpStatus == "4")
                    {
                        request.ApprovalDate = DateTime.Now;
                        request.AduitUserCode = detials.Data.Info.Applyer.UserId;
                    }

                    if (!string.IsNullOrWhiteSpace(request.AduitUserCode))
                    {
                        request.AduitUserName = _workbenchDbContext.User.FirstOrDefault(it => it.UserId == request.AduitUserCode)?.Name;
                    }
                    _mymoooContext.WorkbenchRedisCache.HashSet(approvalRequest, p => p.SpStatus);

                    ResponseMessage<dynamic> result = await _rabbitMQServiceClient.SendMessage<WorkbenchContext, User>(_mymoooContext, sendRabbitCode, JsonSerializerOptionsUtils.Serialize(request));
                    return result.IsSuccess;
                }
            }
            return true;
        }

        public async Task<bool> WeiXinCallBack(WeiXinMessage message)
        {
            dynamic messageInfo = XmlUtils.GetAnonymousType(message.Message);
            message.Spno = messageInfo.ApprovalInfo.SpNo;
            message.Status = Convert.ToInt32(messageInfo.ApprovalInfo.SpStatus);
            var result = await WeiXinCallBack(messageInfo);
            if (result)
            {
                message.CompleteDate = DateTime.Now;
            }
            return result;
        }
    }
}
