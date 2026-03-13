using com.mymooo.workbench.core.Expressage.Kuayue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using System.IO;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Filter
{
    /// <summary>
    /// 跨越路由回调验证
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class KuayueSignAttribute(ILogger<KuayueSignAttribute> logger, KuayueClient kuayueClient, IOptions<KuayueExpressageInfo> expressageInfo) : AbstractSignAttribute
    {
        private readonly ILogger<KuayueSignAttribute> _logger = logger;
        private readonly KuayueClient _kuayueClient = kuayueClient;
        private readonly KuayueExpressageInfo _expressageInfo = expressageInfo.Value;

		public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("跨越回调开始");
            var timestamp = context.HttpContext.Request.Headers["X-KYE-TIMESTAMP"];
            var sign = context.HttpContext.Request.Headers["X-KYE-SIGN"];
            Task<string> body = null;
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(context.HttpContext.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                body = reader.ReadToEndAsync();
            }
            if (KuayueClient.Md5(_expressageInfo.PlatformFlag + timestamp + body.Result) != sign)
            {
                ResponseMessage<dynamic> message = new ResponseMessage<dynamic>
                {
                    Code = ResponseCode.TokenInvalid,
                    ErrorMessage = "签名验证不通过！"
                };
                context.Result = new JsonResult(message);
            }
        }
    }
}
