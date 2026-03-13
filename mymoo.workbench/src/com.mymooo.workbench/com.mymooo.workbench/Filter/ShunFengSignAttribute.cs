using com.mymooo.workbench.core;
using com.mymooo.workbench.core.Expressage.ShunFeng;
using com.mymooo.workbench.core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Filter
{
    [AutoInject(InJectType.Scope)]
    public class ShunFengSignAttribute : AbstractSignAttribute
    {
        private readonly ILogger<ShunFengSignAttribute> _logger;
        private readonly ShunFengClient _shunFengClient;
        private readonly ShunFengExpressageInfo _expressageInfo;
        public ShunFengSignAttribute(ILogger<ShunFengSignAttribute> logger, ShunFengClient shunFengClient, IOptions<ShunFengExpressageInfo> expressageInfo)
        {
            _logger = logger;
            _shunFengClient = shunFengClient;
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
                    keyValues[split.Substring(0, index)] = split.Substring(index + 1);
                }
            }
            if (!keyValues.ContainsKey("msgData") || !keyValues.ContainsKey("timestamp") || !keyValues.ContainsKey("msgDigest"))
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
                if (_shunFengClient.Md5($"{keyValues["msgData"]}{keyValues["timestamp"]}{_expressageInfo.AppSecret}") != System.Web.HttpUtility.UrlDecode(keyValues["msgDigest"]))
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
