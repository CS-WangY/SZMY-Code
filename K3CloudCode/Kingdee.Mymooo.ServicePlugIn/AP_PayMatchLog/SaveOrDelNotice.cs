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
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.Mymooo.Core.Common;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.AP_PayMatchLog
{
    [Description("应付付款核销记录保存和删除通知消息"), HotUpdate]
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
                    var sql = $@"/*dialect*/select tt2.FSETTLEORGID,tt1.FSRCBILLNO,tt3.FNUMBER,tt4.FNAME,tt2.FALLAMOUNTFOR,tt1.FCURWRITTENOFFAMOUNTFOR,tt5.SUMAMOUNTFOR from (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) FCURWRITTENOFFAMOUNTFOR, t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                group by t2.FSRCBILLNO
                                ) tt1
                                inner join T_AP_PAYABLE tt2 on tt1.FSRCBILLNO=tt2.FBILLNO
                                inner join t_BD_Supplier tt3 on tt2.FSUPPLIERID=tt3.FSupplierId
                                inner join t_BD_Supplier_L tt4 on tt3.FSupplierId=tt4.FSupplierId and tt4.FLOCALEID=2052
                                inner join (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) SUMAMOUNTFOR, t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t2.FSRCBILLNO in (
                                select distinct t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
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
                            string cUserWxCode = GetWeChatCode(Convert.ToInt64(item["FSETTLEORGID"]));
                            string sendMessage = "";
                            sendMessage += $"供应商编码：" + item["FNUMBER"] + "\r\n";
                            sendMessage += $"供应商名称：" + item["FNAME"] + "\r\n";
                            sendMessage += $"应付单号：" + item["FSRCBILLNO"] + "\r\n";
                            sendMessage += $"本次付款金额：-" + Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"总金额：" + Convert.ToDecimal(item["FALLAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"累计总付款：" + (Convert.ToDecimal(item["SUMAMOUNTFOR"]) - Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"])).ToString("0.##") + "\r\n";
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
                    var sql = $@"/*dialect*/select tt2.FSETTLEORGID,tt1.FSRCBILLNO,tt3.FNUMBER,tt4.FNAME,tt2.FALLAMOUNTFOR,tt1.FCURWRITTENOFFAMOUNTFOR,tt5.SUMAMOUNTFOR from (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) FCURWRITTENOFFAMOUNTFOR, t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t1.FID in ({string.Join(",", fidList)}) 
                                group by t2.FSRCBILLNO
                                ) tt1
                                inner join T_AP_PAYABLE tt2 on tt1.FSRCBILLNO=tt2.FBILLNO
                                inner join t_BD_Supplier tt3 on tt2.FSUPPLIERID=tt3.FSupplierId
                                inner join t_BD_Supplier_L tt4 on tt3.FSupplierId=tt4.FSupplierId and tt4.FLOCALEID=2052
                                inner join (
                                select SUM(t2.FCURWRITTENOFFAMOUNTFOR) SUMAMOUNTFOR, t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
                                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t2.FSRCBILLNO in (
                                select distinct t2.FSRCBILLNO from T_AP_PAYMatchLog t1
                                                inner join T_AP_PAYMatchLogENTRY t2 on  t1.FID=t2.FID and t2.FSOURCETYPE ='a83c007f22414b399b0ee9b9aafc75f9'--标准应付单
				                                inner join T_AP_PAYMatchLogENTRY t3 on  t1.FID=t3.FID and t3.FSOURCETYPE ='D9652BC4-1420-4D3B-A214-2509BC9BF925'--采购业务付款单
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
                            string cUserWxCode = GetWeChatCode(Convert.ToInt64(item["FSETTLEORGID"]));
                            string sendMessage = "";
                            sendMessage += $"供应商编码：" + item["FNUMBER"] + "\r\n";
                            sendMessage += $"供应商名称：" + item["FNAME"] + "\r\n";
                            sendMessage += $"应付单号：" + item["FSRCBILLNO"] + "\r\n";
                            sendMessage += $"本次付款金额：" + Convert.ToDecimal(item["FCURWRITTENOFFAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"总金额：" + Convert.ToDecimal(item["FALLAMOUNTFOR"]).ToString("0.##") + "\r\n";
                            sendMessage += $"累计总付款：" + Convert.ToDecimal(item["SUMAMOUNTFOR"]).ToString("0.##") + "\r\n";
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
        /// 根据组织获取负责人的企业微信Code
        /// </summary>
        private string GetWeChatCode(long orgId)
        {
            List<string> list = new List<string>();
            // 读取系统参数包
            var parameterData = SystemParameterServiceHelper.Load(this.Context, orgId, 0, "AP_SystemParameter");
            foreach (DynamicObject userItem in (DynamicObjectCollection)parameterData["FPayNoticeUser"])
            {
                var userCode = Convert.ToString(((DynamicObject)userItem["FPayNoticeUser"])["FStaffNumber"]);
                list.Add(userCode);
            }
            return string.Join("|", list);
        }
    }
}
