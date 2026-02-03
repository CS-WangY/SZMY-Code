using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Media;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.Utils;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;

namespace mymooo.weixinWork.SDK.WeixinWorkMessage
{
    /// <summary>
    /// 发送消息服务
    /// </summary>
    /// <param name="httpService"></param>
    /// <param name="mediaServiceClient"></param>
    /// <param name="systemCode">系统编码</param>
    /// <param name="envCode"></param>
    /// <param name="agentId"></param>
    public class SendMessageService(WeixinWorkHttpService httpService, MediaServiceClient mediaServiceClient, string systemCode, string envCode, long agentId)
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly WeixinWorkHttpService _httpService = httpService;

        /// <summary>
        /// 系统编码
        /// </summary>
        protected readonly string _systemCode = systemCode;

        /// <summary>
        /// 环境变量
        /// </summary>
        protected readonly string _envCode = envCode;

        /// <summary>
        /// 应用Id
        /// </summary>
        protected readonly long _agentId = agentId;

        /// <summary>
        /// 企业微信媒体服务
        /// </summary>
        protected readonly MediaServiceClient _mediaServiceClient = mediaServiceClient;

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendTextMessage<C, U>(C mymoooContext, SendTextMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            request.AgentId = _agentId;
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_envCode}/cgi-bin/message/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送 MarkDown 消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendMarkDownMessage<C, U>(C mymoooContext, SendMarkdownMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            request.AgentId = _agentId;
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_envCode}/cgi-bin/message/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送卡片消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendTextcardMessage<C, U>(C mymoooContext, SendTextCardMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            request.AgentId = _agentId;
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_envCode}/cgi-bin/message/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送文件消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendFileMessageAsync<C, U>(C mymoooContext, SendFileMessageRequest request) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.MediaFile);
            ResponseMessage<SendMessageResponse> response = new();
            request.MediaFile.SystemCode = _systemCode;
            request.MediaFile.EnvCode = _envCode;
            var file = await _mediaServiceClient.UpLoadMediaAsync(request.MediaFile);
            if (file.ErrCode != 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = $"调用企业微信发生错误:{file.ErrorMessage} ";
                return response;
            }
            request.AgentId = _agentId;
            request.MediaType = request.MediaFile.MediaType;
            request.File = new SendFileMessageRequest.FileMessage() { MediaId = file.MediaId };
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"{_systemCode}/{_envCode}/cgi-bin/message/send", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 上传媒体文件
        /// </summary>
        /// <param name="mediaInfo"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<string>> UpLoadMediaAsync(MediaInfo mediaInfo)
        {
            ResponseMessage<string> response = new();
            mediaInfo.SystemCode = _systemCode;
            var result = await _mediaServiceClient.UpLoadMediaAsync(mediaInfo);

            if (result.ErrCode != 0)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.ErrorMessage = result.ErrorMessage;
                response.Code = ResponseCode.Exception;
            }
            else
            {
                response.Data = result.MediaId;
            }
            return response;
        }

        /// <summary>
        /// 上传媒体文件
        /// </summary>
        /// <param name="mediaInfos"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<string>>> UpLoadMediaAsync(List<MediaInfo> mediaInfos)
        {
            ResponseMessage<List<string>> response = new()
            {
                Data = []
            };
            foreach (var mediaInfo in mediaInfos)
            {
                mediaInfo.SystemCode = _systemCode;
                mediaInfo.EnvCode = _envCode;
                var result = await _mediaServiceClient.UpLoadMediaAsync(mediaInfo);

                if (result.ErrCode != 0)
                {
                    response.Code = ResponseCode.ThirdpartyError;
                    response.ErrorMessage = result.ErrorMessage;
                    response.Code = ResponseCode.Exception;
                    break;
                }
                else
                {
                    response.Data.Add(result.MediaId);
                }
            }
            return response;
        }
    }
}
