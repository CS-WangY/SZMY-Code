using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mymooo.core.Attributes;
using System;
using System.Linq;

namespace com.mymooo.workbench.business.QuartzTask.WeiXinWork
{
    [AutoInject(InJectType.Scope)]
	public class WeiXinWorkMessage(ILogger<WeiXinWorkMessage> logger, WorkbenchDbContext workbenchDbContext, MessageScheduled messageScheduled, WorkbenchContext workbenchContext)
	{
        private readonly ILogger<WeiXinWorkMessage> _logger = logger;
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly MessageScheduled _messageScheduled = messageScheduled;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

		public string AddMessage(string appId, string appCode, string msg_signature, string timestamp, string nonce, string echostr, string data)
        {
            var app = _workbenchDbContext.ThirdpartyApplicationDetail.Include(c => c.ThirdpartyApplicationConfig).FirstOrDefault(a => a.DetailCode == appCode && a.AppId == appId);
            int ret = 0;
			WXBizMsgCrypt wxcpt = new(app.Token, app.EncodingAESKey, app.ThirdpartyApplicationConfig.Token);
            if (string.IsNullOrWhiteSpace(data))
            {
                string sEchoStr = "";
                ret = wxcpt.VerifyURL(msg_signature, timestamp, nonce, echostr, ref sEchoStr);
                return sEchoStr;
            }
            else
            {
                string sMsg = "";  // 解析之后的明文
                ret = wxcpt.DecryptMsg(msg_signature, timestamp, nonce, data, ref sMsg);
                if (ret != 0)
                {
                    System.Console.WriteLine("ERR: Decrypt Fail, ret: " + ret);
                    _logger.LogError(new Exception(), "请求的参数:{0} 结果 {1}", data, "ERR: Decrypt Fail, ret: " + ret);
                    return "";
                }
                var message = new WeiXinMessage()
                {
                    ApplicationDetailId = app.Id,
                    CreateDate = DateTime.Now,
                    Message = sMsg
                };

				_workbenchDbContext.WeiXinMessage.Add(message);
                _workbenchDbContext.SaveChanges();
				_ = _messageScheduled.ExecWeiXinMessage(message, _workbenchContext);

				return "";
            }
        }
    }
}
