using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.Utils;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;

namespace mymooo.weixinWork.SDK.Robot
{
    /// <summary>
    /// 机器人
    /// </summary>
    /// <remarks>
    /// 企业微信媒体服务
    /// </remarks>
    [AutoInject(InJectType.Single)]
    public class RobotServiceClient(WeixinWorkHttpService httpService, IHttpClientFactory httpClientFactory)
    {
        private readonly WeixinWorkHttpService _httpService = httpService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <param name="robotKey">机器人key</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendTextMessage<C, U>(C mymoooContext, SendTextMessageRequest request, string robotKey) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"Robot/production/cgi-bin/webhook/send?key={robotKey}", JsonSerializerOptionsUtils.Serialize(request));
        }
        /// <summary>
        /// 发送 MarkDown 消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <param name="robotKey">机器人key</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendMarkDownMessage<C, U>(C mymoooContext, SendMarkdownMessageRequest request, string robotKey) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"Robot/production/cgi-bin/webhook/send?key={robotKey}", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送卡片消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <param name="robotKey">机器人key</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendTextcardMessage<C, U>(C mymoooContext, SendTextCardMessageRequest request, string robotKey) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"Robot/production/cgi-bin/webhook/send?key={robotKey}", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 发送文件消息
        /// </summary>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="request"></param>
        /// <param name="robotKey">机器人key</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SendMessageResponse>> SendFileMessageAsync<C, U>(C mymoooContext, SendFileMessageRequest request, string robotKey) where U : UserBase, new() where C : MymoooContext<U>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.MediaFile);
            ResponseMessage<SendMessageResponse> response = new();
            var file = await UpLoadMediaAsync(request.MediaFile, robotKey);
            if (file.ErrCode != 0)
            {
                response.Code = ResponseCode.WeiXinError;
                response.ErrorMessage = $"调用企业微信发生错误:{file.ErrorMessage} ";
                return response;
            }
            request.MediaType = request.MediaFile.MediaType;
            request.File = new SendFileMessageRequest.FileMessage() { MediaId = file.MediaId };
            return await _httpService.InvokeWebServiceAsync<C, U, SendMessageResponse>(mymoooContext, $"Robot/production/cgi-bin/webhook/send?key={robotKey}", JsonSerializerOptionsUtils.Serialize(request));
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="mediaInfo"></param>
        /// <param name="robotKey">机器人key</param>
        /// <returns></returns>
        private async Task<UpLoadMediaResponese> UpLoadMediaAsync(MediaInfo mediaInfo, string robotKey)
        {
            if (mediaInfo.File == null || mediaInfo.File.Length == 0)
            {
                using HttpClient client = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await client.GetAsync(string.IsNullOrWhiteSpace(mediaInfo.FileUrl) ? mediaInfo.FileStream : mediaInfo.FileUrl);
                using Stream stream = await response.Content.ReadAsStreamAsync();
                //创建本地文件写入流
                byte[] bArr = new byte[1024];
                int size = stream.Read(bArr, 0, (int)bArr.Length);
                List<byte> bytes = [];
                while (size > 0)
                {
                    bytes.AddRange(bArr.Take(size));
                    size = stream.Read(bArr, 0, (int)bArr.Length);
                }
                mediaInfo.File = [.. bytes];
            }
            var result = await _httpService.InvokePostFileToWeixinWorkAsync($"Robot/production/cgi-bin/webhook/upload_media?key={robotKey}&type=file", mediaInfo.File, mediaInfo.FileName);

            return JsonSerializerOptionsUtils.DeserializeObject<UpLoadMediaResponese>(result);
        }
    }
}
