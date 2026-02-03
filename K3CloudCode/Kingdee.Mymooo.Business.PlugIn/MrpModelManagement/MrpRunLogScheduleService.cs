using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.UserManager;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Cvp;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;

namespace Kingdee.Mymooo.Business.PlugIn.MrpModelManagement
{
	[Description("定时获取运算日志发送给指定人"), HotUpdate]
	public class MrpRunLogScheduleService : IScheduleService
	{
		public void Run(Context ctx, Schedule schedule)
		{
			var context = LoginServiceUtils.BackgroundLogin(ctx);
			//获取所有未发送异常的日志
			string sSql = $@"/*dialect*/SELECT t1.FSCHEMEID,t4.FNUMBER as FMRPName,t1.FNUMBER,tpm.FENTRYID,
       t3.FLOGCONTENT
FROM T_PLN_MRPLOG t1
    INNER JOIN
    (
        SELECT FID,
               MAX(FENTRYID) FENTRYID
        FROM T_PLN_MRPLOGENTRY
        --WHERE FPENYChkSend = 0
        GROUP BY FID
    ) t2
        ON t1.FID = t2.FID
		INNER JOIN T_PLN_MRPLOGENTRY tpm ON t2.FENTRYID=tpm.FENTRYID
    INNER JOIN T_PLN_MRPLOGENTRY_L t3
        ON t2.FENTRYID = t3.FENTRYID
    LEFT JOIN T_PLN_MRPOPTION t4 ON t1.FSCHEMEID=t4.FID
WHERE t1.FSTATUS = 2 AND tpm.FPENYChkSend=0";
			var reader = DBServiceHelper.ExecuteDynamicObject(context, sSql);
			foreach (var item in reader)
			{
				long entryid = Convert.ToInt64(item["FENTRYID"]);
				string mrpnumber = Convert.ToString(item["FNUMBER"]);
				string mrplog = Convert.ToString(item["FLOGCONTENT"]);
				string mrpname = Convert.ToString(item["FMRPName"]);
				sSql = $"SELECT FPENYMULLOGUSER FROM PENY_t_MulLogUser WHERE FID={item["FSCHEMEID"]}";
				var users = DBServiceHelper.ExecuteDynamicObject(context, sSql);
				foreach (var user in users)
				{
					var createUser = Convert.ToInt64(user["FPENYMULLOGUSER"]);
					//var cUserWxCode = GetUserWxCode(context, createUser);
					//if (!string.IsNullOrWhiteSpace(cUserWxCode))
					//{
					//	SendTextMessageUtils.SendTextMessage(cUserWxCode, "运算日志[" + mrpname + "][" + mrpnumber + "]:" + mrplog);
					//}

					string sendMessage = $@"#### 预警消息提醒
>运算编码:<font color=""info"">{mrpnumber}</font> 运算名称:<font color=""comment"">{mrpname}</font>";
					sendMessage = sendMessage + "\r\n" + @"<font color=""warning"">" + mrplog + "</font>";
					//创建人微信Code
					var cUserWxCode = GetUserWxCode(context, createUser);

					if (!string.IsNullOrWhiteSpace(cUserWxCode))
					{
						SendMarkdownMessageRequest WxEntity = new SendMarkdownMessageRequest();
						WxEntity = new SendMarkdownMessageRequest()
						{
							ToUser = cUserWxCode,
							MarkDown = new SendMarkdownMessageRequest.MarkDownMessage()
							{
								Content = sendMessage
							}
						};
						var response = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendMarkdownMessage", JsonConvertUtils.SerializeObject(WxEntity));
					}
				}
				sSql = $"/*dialect*/UPDATE T_PLN_MRPLOGENTRY SET FPENYChkSend=1 WHERE FENTRYID={entryid}";
				DBServiceHelper.Execute(context, sSql);
			}
		}

		private string GetUserWxCode(Context ctx, long userId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
			string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
	}
}
