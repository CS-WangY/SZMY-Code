using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.Utils;

namespace mymooo.weixinWork.SDK.Media
{
    /// <summary>
    /// 企业微信媒体服务
    /// </summary>
    /// <param name="httpService"></param>
    /// <param name="httpClientFactory"></param>
    [AutoInject(InJectType.Single)]
    public class MediaServiceClient(WeixinWorkHttpService httpService, IHttpClientFactory httpClientFactory)
    {
        private readonly WeixinWorkHttpService _httpService = httpService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="mediaInfo"></param>
        /// <returns></returns>
        public async Task<UpLoadMediaResponese> UpLoadMediaAsync(MediaInfo mediaInfo)
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
            var result = await _httpService.InvokePostFileToWeixinWorkAsync($"{mediaInfo.SystemCode}/{mediaInfo.EnvCode}/cgi-bin/media/upload?type={mediaInfo.MediaType}", mediaInfo.File, mediaInfo.FileName);

            return JsonSerializerOptionsUtils.DeserializeObject<UpLoadMediaResponese>(result);
        }

        /// <summary>
        /// 下载媒体
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="systemCode">系统</param>
        /// <param name="envCode">环境变量</param>
        /// <param name="mediaId">媒体Id</param>
        /// <returns></returns>
        public async Task<byte[]> DownLoadMediaAsync<C, U>(C mymoooContext, string systemCode, string envCode, string mediaId) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.DownloadFileAsync<C, U>(mymoooContext, $"{systemCode}/{envCode}/cgi-bin/media/get?media_id={mediaId}");
        }
    }
}
