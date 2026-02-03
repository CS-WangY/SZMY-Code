using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Config;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.AddressBook.Model;
using mymooo.weixinWork.SDK.Application.Model;
using mymooo.weixinWork.SDK.Application.Model.ClockIn;
using mymooo.weixinWork.SDK.Application.Model.ExternalContact;
using mymooo.weixinWork.SDK.Application.Model.KfAccount;
using mymooo.weixinWork.SDK.Config;
using mymooo.weixinWork.SDK.Media;
using mymooo.weixinWork.SDK.Utils;
using mymooo.weixinWork.SDK.WeixinWorkMessage;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;

namespace mymooo.weixinWork.SDK.Application
{
    /// <summary>
    /// 审批应用
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class ApplicationServiceClient(WeixinWorkHttpService httpService, IOptions<ApigatewayConfig> apigatewayConfig, MediaServiceClient mediaServiceClient, IOptions<WeiXinWorkConfig> weiXinWorkConfig)
        : SendMessageService(httpService, mediaServiceClient, "weixinwork-Application", apigatewayConfig.Value.EnvCode, weiXinWorkConfig.Value.ApplicationAgentId)
    {
        private readonly ApigatewayConfig _apigatewayConfig = apigatewayConfig.Value;

        /// <summary>
        /// 创建群聊
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<CreateChatResponse>> CreateChat<C, U>(C mymoooContext, CreateChatRequest? request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, CreateChatResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/create", JsonSerializerOptionsUtils.Serialize(request));

        }

        /// <summary>
        /// 更新群聊
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<CreateChatResponse>> UpdateChat<C, U>(C mymoooContext, UpdateChatRequest? request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, CreateChatResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/update", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取群聊
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="chatId">群聊Id</param>
        /// <returns></returns>
        public async Task<ResponseMessage<GetChatResponse>> GetChat<C, U>(C mymoooContext, string? chatId) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(chatId);
            return await _httpService.InvokeWebServiceAsync<C, U, GetChatResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/get?chatid={chatId}");
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendChatTextMessage<C, U>(C mymoooContext, SendTextMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送 MarkDown 消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendChatMarkDownMessage<C, U>(C mymoooContext, SendMarkdownMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送卡片消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendChatTextcardMessage<C, U>(C mymoooContext, SendTextCardMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送文件消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendChatFileMessageAsync<C, U>(C mymoooContext, SendFileMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.MediaFile);
            ResponseMessage<SendMessageResponse> response = new();
            var file = await _mediaServiceClient.UpLoadMediaAsync(request.MediaFile);
            if (file.ErrCode > 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = $"调用企业微信发生错误:{file.ErrorMessage} ";
                return response;
            }
            request.File = new SendFileMessageRequest.FileMessage() { MediaId = file.MediaId };
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/appchat/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取访问用户身份
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<UserIdResponse>> GetUserId<C, U>(C mymoooContext, string code) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, UserIdResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/user/getuserinfo?code={code}");
        }

        /// <summary>
        /// 获取访问用户身份
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<UserInfoResponse>> GetUserInfo<C, U>(C mymoooContext, string userid) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, UserInfoResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/user/get?userid={userid}");
        }

        /// <summary>
        /// openId 转 UserId
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<UserIdResponse>> OpenIdConvertUserId<C, U>(C mymoooContext, string openid) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, UserIdResponse>(mymoooContext, $"{_systemCode}/{_apigatewayConfig.EnvCode}/cgi-bin/user/convert_to_userid", JsonSerializerOptionsUtils.Serialize(new { openid }));
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

        /// <summary>
        /// 获取部门列表
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="departmentId">部门Id</param>
        /// <returns></returns>
        public async Task<ResponseMessage<DepartmentListResponse>> GetDepartmentList<C, U>(C mymoooContext, long departmentId = 0) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, DepartmentListResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/department/list?&id={departmentId}");
        }

        /// <summary>
        /// 获取部门成员
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="departmentId">部门Id</param>
        /// <param name="fetchChild">是否递归子部门</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SimpleDepartmentResponse>> GetSimpleDepartmentUsers<C, U>(C mymoooContext, long departmentId = 0, int fetchChild = 0) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, SimpleDepartmentResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/user/simplelist?&department_id={departmentId}&fetch_child={fetchChild}");
        }

        /// <summary>
        /// 获取部门下全部的用户
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="departmentId">部门ID  1 默认全部</param>
        /// <param name="fetchChild"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<DepartmentUsersResponse>> GetDepartmentUsers<C, U>(C mymoooContext, long departmentId = 1, int fetchChild = 1) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, DepartmentUsersResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/user/list?department_id={departmentId}&fetch_child={fetchChild}");
        }

        /// <summary>
        /// 获取打卡记录
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<ClockInDataResponse>> GetClockInDatas<C, U>(C mymoooContext, ClockInDataRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, ClockInDataResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/checkin/getcheckindata", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 可通过接口写入打卡记录，匹配打卡规则后可在企业微信打卡明细、统计中参与展示。
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<ApplicationResponse>> AddCheckinRecord<C, U>(C mymoooContext, AddCheckinRecordRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            foreach (var record in request.Records)
            {
                if (record.MediaInfo != null)
                {
                    //需要上传媒体
                    record.MediaInfo.SystemCode = _systemCode;
                    record.MediaInfo.EnvCode = "preview2";
                    var uploadResult = await _mediaServiceClient.UpLoadMediaAsync(record.MediaInfo);
                    if (uploadResult.ErrCode == 0)
                    {
                        record.MediaIds = [uploadResult.MediaId];
                        record.MediaInfo = null;
                    }
                    else
                    {
                        return new ResponseMessage<ApplicationResponse>() { Code = ResponseCode.WeiXinError, ErrorMessage = uploadResult.ErrorMessage };
                    }
                }
            }
            return await _httpService.InvokeWebServiceAsync<C, U, ApplicationResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/checkin/add_checkin_record", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取客服账号列表
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<KfAccountListResponse>> GetKfAccountList<C, U>(C mymoooContext, KfAccountListRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, KfAccountListResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/kf/account/list", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取企业已配置的「联系我」列表
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<ExternalContactListResponse>> GetExternalContactList<C, U>(C mymoooContext, ExternalContactListRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, ExternalContactListResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/externalcontact/list_contact_way", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 获取企业已配置的「联系我」方式
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<ExternalContactDetailResponse>> GetExternalContactDetail<C, U>(C mymoooContext, ExternalContactListResponse.ExternalContactId request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, ExternalContactDetailResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/externalcontact/get_contact_way", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 更新企业已配置的「联系我」方式
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<WeiXinMessageResponse>> UpdateExternalContactDetail<C, U>(C mymoooContext, ExternalContactDetailResponse.ExternalContactDetail request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, WeiXinMessageResponse>(mymoooContext, $"{_systemCode}/preview2/cgi-bin/externalcontact/update_contact_way", JsonSerializerOptionsUtils.Serialize(request));
        }
    }
}