using com.mymooo.workbench.core.Expressage.DeBang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Filter
{
    [AutoInject(InJectType.Scope)]
    public class DeBangSignAttribute : AbstractSignAttribute
    {
        private readonly ILogger<DeBangSignAttribute> _logger;
        private readonly DeBangClient _debangClient;
        private readonly DeBangExpressageInfo _expressageInfo;
        public DeBangSignAttribute(ILogger<DeBangSignAttribute> logger, DeBangClient deBangClient, IOptions<DeBangExpressageInfo> expressageInfo)
        {
            _logger = logger;
            _debangClient = deBangClient;
            _expressageInfo = expressageInfo.Value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Task<string> body = null;
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(context.HttpContext.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                body = reader.ReadToEndAsync();
            }
            var splits = body.Result.Split('&', StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> keyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var split in splits)
            {
                int index = split.IndexOf("=");
                if (index >= 0)
                {
                    keyValues[split.Substring(0, index)] = System.Web.HttpUtility.UrlDecode(split.Substring(index + 1));
                }
            }
            if (!keyValues.ContainsKey("params") || !keyValues.ContainsKey("timestamp") || !keyValues.ContainsKey("digest"))
            {

                ResponseMessage<dynamic> message = new ResponseMessage<dynamic>
                {
                    Code = ResponseCode.TokenInvalid,
                    ErrorMessage = "签名验证不通过！"
                };
                context.Result = new JsonResult(message);
            }
            else
            {
                if (_debangClient.Md5(keyValues["params"] + _expressageInfo.AppKey + keyValues["timestamp"]) != keyValues["digest"])
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
}
