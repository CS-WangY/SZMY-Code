using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.Mymooo.Core.Common;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.MatchRecordBill
{
    [Description("应收收款核销记录保存和删除通知消息"), HotUpdate]
    public class SaveOrDelNotice : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 删除调用
        /// </summary>
        /// <param name="e"></param>
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            try
            {
                List<long> fidList = new List<long>();

                //操作按钮
                if (this.FormOperation.Id.EqualsIgnoreCase("Delete"))
                {
                    foreach (var item in e.DataEntitys)
                    {
                        fidList.Add(Convert.ToInt64(item["Id"]));
                    }
                    var sql = $@"/*dialect*/select tt1.FSRCBILLNO,tt2.FCUSTOMERID,tt3.FNUMBER,tt4.FNAME,tt2.FALLAMOUNTFOR,tt1.FCURWRITTENOFFAMOUNTFOR,tt5.SUMAMOUNTFOR from (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) FCURWRITTENOFFAMOUNTFOR, t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                group by t2.FSRCBILLNO
                                ) tt1
                                inner join T_AR_RECEIVABLE tt2 on tt1.FSRCBILLNO=tt2.FBILLNO
                                inner join T_BD_CUSTOMER tt3 on tt2.FCUSTOMERID=tt3.FCUSTID
                                inner join T_BD_CUSTOMER_L tt4 on tt3.FCUSTID=tt4.FCUSTID and tt4.FLOCALEID=2052
                                inner join (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) SUMAMOUNTFOR, t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t2.FSRCBILLNO in (
                                select distinct t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                )
                                group by t2.FSRCBILLNO
                                ) tt5 on tt5.FSRCBILLNO=tt1.FSRCBILLNO ";
                    var data = DBUtils.ExecuteDynamicObject(this.Context, sql);
                    SendMarkdownMessageRequest WxEntity = new SendMarkdownMessageRequest();
                    foreach (var item in data)
                    {
                        if ((Convert.ToDecimal(item["SUMAMOUNTFOR"]) - Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"])) >= 0)
                        {
                            string cUserWxCode = GetWeChatCode(Convert.ToInt64(item["FCUSTOMERID"]));
                            string sendMessage = "";
                            sendMessage += $"客户编码：" + item["FNUMBER"] + "\r\n";
                            sendMessage += $"客户名称：" + item["FNAME"] + "\r\n";
                            sendMessage += $"应收单号：" + item["FSRCBILLNO"] + "\r\n";
                            sendMessage += $"本次收款金额：-" + Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"总金额：" + Convert.ToDecimal(item["FALLAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"累计总收款：" + (Convert.ToDecimal(item["SUMAMOUNTFOR"]) - Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"])).ToString("0.##") + "\r\n";
                            sendMessage += $"日期时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
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
                }


            }
            catch (Exception ex)
            {
                throw new Exception("操作失败：" + ex.Message);
            }
        }


        /// <summary>
        /// 保存调用
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                List<long> fidList = new List<long>();
                //操作按钮
                if (this.FormOperation.Id.EqualsIgnoreCase("Save"))
                {
                    foreach (var item in e.DataEntitys)
                    {
                        fidList.Add(Convert.ToInt64(item["Id"]));
                    }
                    var sql = $@"/*dialect*/select tt1.FSRCBILLNO,tt2.FCUSTOMERID,tt3.FNUMBER,tt4.FNAME,tt2.FALLAMOUNTFOR,tt1.FCURWRITTENOFFAMOUNTFOR,tt5.SUMAMOUNTFOR from (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) FCURWRITTENOFFAMOUNTFOR, t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                group by t2.FSRCBILLNO
                                ) tt1
                                inner join T_AR_RECEIVABLE tt2 on tt1.FSRCBILLNO=tt2.FBILLNO
                                inner join T_BD_CUSTOMER tt3 on tt2.FCUSTOMERID=tt3.FCUSTID
                                inner join T_BD_CUSTOMER_L tt4 on tt3.FCUSTID=tt4.FCUSTID and tt4.FLOCALEID=2052
                                inner join (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) SUMAMOUNTFOR, t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t2.FSRCBILLNO in (
                                select distinct t2.FSRCBILLNO from T_AR_RECMacthLog t1
                                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'--标准应收单
				                                inner join T_AR_RECMacthLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='36cf265bd8c3452194ed9c83ec5e73d2'--销售收款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                )
                                group by t2.FSRCBILLNO
                                ) tt5 on tt5.FSRCBILLNO=tt1.FSRCBILLNO ";
                    var data = DBUtils.ExecuteDynamicObject(this.Context, sql);
                    SendMarkdownMessageRequest WxEntity = new SendMarkdownMessageRequest();
                    foreach (var item in data)
                    {
                        if (Convert.ToDecimal(item["SUMAMOUNTFOR"]) >= 0)
                        {
                            string cUserWxCode = GetWeChatCode(Convert.ToInt64(item["FCUSTOMERID"]));
                            string sendMessage = "";
                            sendMessage += $"客户编码：" + item["FNUMBER"] + "\r\n";
                            sendMessage += $"客户名称：" + item["FNAME"] + "\r\n";
                            sendMessage += $"应收单号：" + item["FSRCBILLNO"] + "\r\n";
                            sendMessage += $"本次收款金额：" + Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"总金额：" + Convert.ToDecimal(item["FALLAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"累计总收款：" + Convert.ToDecimal(item["SUMAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"日期时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
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
                }
            }
            catch (Exception ex)
            {
                throw new Exception("操作失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 根据客户id获取对应的业务员和助理的微信Code
        /// </summary>
        private string GetWeChatCode(long custId)
        {

            List<string> list = new List<string>();
            string sql = $@"/*dialect*/SELECT distinct
                        emp.FWeChatCode
                        FROM T_SAL_SCSALERCUST sg
                        inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
                        inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY' and e.FISUSE='1'
                        inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID and staff.FFORBIDSTATUS='A'
						inner join T_HR_EMPINFO emp on emp.FID=staff.FEMPINFOID
						inner join T_HR_EMPINFO_L empl on emp.FID=empl.FID
                        where sg.FCUSTOMERID={custId}";
            var data = DBUtils.ExecuteDynamicObject(this.Context, sql);
            foreach (var item in data)
            {
                list.Add(Convert.ToString(item["FWeChatCode"]));
            }
            return string.Join("|", list);
        }
    }
}
