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
using Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System.Security.Cryptography;
using Kingdee.BOS.BusinessEntity.UserManager;
using System.Xml.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Warn.Message;

namespace Kingdee.Mymooo.ServicePlugIn.WarnService
{
	[Description("预警-监控方案解析发送企业微信"), HotUpdate]
	public class OutStockWarnMessageService : AbstractWarnServicePlugIn
	{
		public override void BeforeParseWarnMessage(BeforeParseWarnMessageEventArgs e)
		{
			//if (e.WarnSchedule.WarnObject.EqualsIgnoreCase("PENY_PLN_RESERVELINK_WarnObject"))
			//{
			//    e.KeyValueFieldNames.Add("FSUPPLYBILLNO");
			//}
			base.BeforeParseWarnMessage(e);
		}
		public override void AfterSendWarnMessage(AfterSendWarnMessageEventArgs e)
		{
			base.AfterSendWarnMessage(e);
			foreach (var item in e.WarnMessage.WarnObjectDataCollection.Rows)
			{
				string sendMessage = $@"#### 预警消息提醒
>出库单号:<font color=""info"">{item["FBILLNO"]}</font>
>客户编码:<font color=""comment"">{item["FNUMBER"]}</font>
>客户名称:<font color=""comment"">{item["FNAME"]}</font>
>最后回款日:<font color=""comment"">{item["FEXPECTEDPAYMENTDATE"]}</font>
>剩余回款金额:<font color=""comment"">{item["FAMOUNT"]}</font>
>异常情况: <font color=""warning"">超过回款日期，请及时处理！</font>";
				string sendGroupMessage = $@"#### 预警消息 原销售员离职，请关注：
>出库单号:<font color=""info"">{item["FBILLNO"]}</font>
>客户编码:<font color=""comment"">{item["FNUMBER"]}</font>
>客户名称:<font color=""comment"">{item["FNAME"]}</font>
>最后回款日:<font color=""comment"">{item["FEXPECTEDPAYMENTDATE"]}</font>
>剩余回款金额:<font color=""comment"">{item["FAMOUNT"]}</font>
>异常情况: <font color=""warning"">超过回款日期，请及时处理！</font>";


				var salorgid = Convert.ToInt64(item["FSALEORGID"]);
				var salemanid = Convert.ToInt64(item["FSALESMANID"]);
				var cUserWxCode = GetUserWxCodeByUserID(this.Context, salemanid);
				//获取缓存企业微信信息
				var apiinfo = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/AddressBook/GetLeveParent?usercode={cUserWxCode}&level=1");
				var puserinfo = JsonConvertUtils.DeserializeObject<ParentUserInfo>(apiinfo);

				var cCustID = Convert.ToInt64(item["FCUSTID"]);

				var cUsers = "";
				if (puserinfo is null || string.IsNullOrEmpty(puserinfo.parentUserCode))
				{
					if (string.IsNullOrWhiteSpace(cUserWxCode))
					{
						cUsers = GetSalGroupUserWxCode(this.Context, cCustID, salorgid);
						cUsers = cUsers + GetWarnUser(e.WarnMessage.UserWarnMessageCollection);
						SendTextMessageUtils.SendMarkdownMessage(cUsers, sendGroupMessage);
					}
					else
					{
						cUsers = cUserWxCode;
						cUsers = cUsers + GetWarnUser(e.WarnMessage.UserWarnMessageCollection);
						SendTextMessageUtils.SendMarkdownMessage(cUsers, sendMessage);
					}
				}
				else
				{
					var cUserParWxCode = GetUserWxCodeByCode(this.Context, puserinfo.parentUserCode);
					if (string.IsNullOrWhiteSpace(cUserWxCode) || string.IsNullOrWhiteSpace(cUserParWxCode))
					{
						cUsers = GetSalGroupUserWxCode(this.Context, cCustID, salorgid);
						cUsers = cUsers + GetWarnUser(e.WarnMessage.UserWarnMessageCollection);
						SendTextMessageUtils.SendMarkdownMessage(cUsers, sendGroupMessage);
					}
					else
					{
						cUsers = string.Join("|", cUserWxCode, cUserParWxCode);
						cUsers = cUsers + GetWarnUser(e.WarnMessage.UserWarnMessageCollection);
						SendTextMessageUtils.SendMarkdownMessage(cUsers, sendMessage);
					}
				}

			}
		}

		private string GetUserWxCode(Context ctx, long userId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
			string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";

			return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
		//获取创建人微信Code
		private string GetUserWxCodeByUserID(Context ctx, long userId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
			string sql = $@"SELECT t1.FWECHATCODE FROM V_BD_SALESMAN t1 INNER JOIN T_BD_STAFF t2 ON t1.FSTAFFID=t2.FSTAFFID
                            INNER JOIN dbo.T_HR_EMPINFO t3 ON t2.FPERSONID=t3.FPERSONID
                            WHERE t1.fid=@FuserID AND t3.FFORBIDSTATUS='A'";

			return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
		private string GetUserWxCodeByCode(Context ctx, string usercode)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserCode", KDDbType.String, usercode) };
			string sql = $@"SELECT t1.FWECHATCODE FROM T_HR_EMPINFO t1 LEFT JOIN T_HR_EMPINFO_L t2 ON t2.FID = t1.FID
							WHERE t1.FNUMBER=@FuserCode AND t1.FFORBIDSTATUS='A'";

			return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
		private string GetSalGroupUserWxCode(Context ctx, long custid, long salorgid)
		{
			List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@FCustid", KDDbType.Int64, custid),
				new SqlParam("@FSalorgid", KDDbType.Int64, salorgid),
			};
			string sql = $@"SELECT t5.FWECHATCODE FROM T_BD_OPERATORENTRY t1
INNER JOIN T_BD_OPERATORDETAILS t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_SCSALERCUST t3 ON t2.FOPERATORGROUPID=t3.FSALERGROUPID
INNER JOIN T_BD_STAFF t4 ON t1.FSTAFFID=t4.FSTAFFID
INNER JOIN dbo.T_HR_EMPINFO t5 ON t4.FPERSONID=t5.FPERSONID
WHERE t3.FCUSTOMERID=@FCustid AND t3.FSALEORGID=@FSalorgid AND t1.FForbiddenStatus=0";

			var userlist = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			return string.Join("|", userlist.Select(x => x["FWECHATCODE"]).ToArray());
		}
		private string GetWarnUser(UserWarnMessageCollection UserList)
		{
			List<string> users = new List<string>();
			foreach (var usermsg in UserList)
			{
				users.Add(GetUserWxCode(this.Context, usermsg.Key));
			}
			return string.Join("|", users.ToArray());
		}
	}
}
