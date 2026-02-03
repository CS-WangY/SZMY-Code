using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Kingdee.BOS;
using Kingdee.BOS.Core.MessageCenter;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.Utils;

namespace Kingdee.Mymooo.ServicePlugIn.WarnService
{
    [Description("预警-监控方案解析发送企业微信"), HotUpdate]
    public class WarnMessageService : AbstractWarnServicePlugIn
    {
        public override void BeforeParseWarnMessage(BeforeParseWarnMessageEventArgs e)
        {
            if (e.WarnSchedule.WarnObject.EqualsIgnoreCase("PENY_PLN_RESERVELINK_WarnObject"))
            {
                e.KeyValueFieldNames.Add("FSUPPLYBILLNO");
            }
            base.BeforeParseWarnMessage(e);
        }
        public override void AfterSendWarnMessage(AfterSendWarnMessageEventArgs e)
        {
            base.AfterSendWarnMessage(e);
            foreach (var usermsg in e.WarnMessage.UserWarnMessageCollection)
            {
                string yjbillno = "";
                string yjname = "";
                foreach (var item in e.WarnMessage.SendWarnMessageCollection)
                {
                    yjname = item["FName"].ToString();
                    yjbillno = item["FBillNo"].ToString();
                }
                //                > 方案编码:< font color = ""info"" >{ yjbillno}</ font >
                //> 方案名称:< font color = ""info"" >{ yjbillno}</ font >
                //> 异常总数:< font color = ""comment"" >{ e.WarnMessage.WarnResulCount}</ font > ";
                string sendMessage = $@"#### 预警消息提醒
>方案编码:<font color=""info"">{yjbillno}</font> 方案名称:<font color=""comment"">{yjname}</font>
>异常总数: <font color=""warning"">{e.WarnMessage.WarnResulCount}</font> 条,请即时关注并处理";
                foreach (var messageItem in usermsg.Value.GetMessageItems())
                {
                    sendMessage = sendMessage + "\r\n" + @"<font color=""warning"">" + messageItem.MessageEntity.GetPropertyValue("Content").ToString() + "</font>";
                }
                //创建人微信Code
                var cUserWxCode = GetUserWxCode(this.Context, usermsg.Key);

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
                    //SendTextMessageUtils.SendTextMessage(cUserWxCode, sendMessage);
                }
            }
        }

        //获取创建人微信Code
        private string GetUserWxCode(Context ctx, long userId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
            string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";

            return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }
    }
}
