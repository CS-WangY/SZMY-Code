using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Config;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using System.Net;

namespace mymooo.weixinWork.SDK.Utils
{
    /// <summary>
    /// 微信请求
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class WeixinWorkHttpService(SignatureAuthorizeService signatureAuthorizeService, IOptions<ApigatewayConfig> apigatewayConfig) : HttpService(signatureAuthorizeService, apigatewayConfig)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override async Task<ResponseMessage<R>> GetWebResponse<R>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializerOptionsUtils.Deserialize<R>(data);
                    if (result == null)
                    {
                        return new ResponseMessage<R>() { Code = ResponseCode.DeserializeError, ErrorMessage = "服务已经成功响应,但是数据无法序列化" };
                    }
                    else
                    {
                        return new ResponseMessage<R>() { Data = result };
                    }
                }
                catch
                {
                    return new ResponseMessage<R>() { Code = ResponseCode.DeserializeError, ErrorMessage = "服务已经成功响应,但是数据无法序列化" };
                }
            }
            else
            {
                return await base.GetWebResponse<R>(response);
            }
        }
    }
}
