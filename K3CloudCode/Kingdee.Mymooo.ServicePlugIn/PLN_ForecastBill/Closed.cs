using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.Utils;
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Common;
using System.Xml.Linq;
using Kingdee.BOS.Orm.DataEntity;
using static Kingdee.K3.Core.HR.HRConsts;

namespace Kingdee.Mymooo.ServicePlugIn.PLN_ForecastBill
{
    [Description("预测订单关闭-同时关闭组织间需求单"), HotUpdate]
    public class Closed : AbstractOperationServicePlugIn
    {
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
            //为了在BeginOperationTransaction里检查数据抛出异常时
            //只回滚当前单据的事务，这里设置为不支持批量事务
            //这样BOS会循环为每一张单据创建事务调用操作
            e.SupportTransaction = true;
            e.SurportBatchTransaction = false;
        }
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FIsPayOnline");
            e.FieldKeys.Add("FBillAllAmount");
            e.FieldKeys.Add("FTaxNetPrice");
            e.FieldKeys.Add("FStockOutQty");
            e.FieldKeys.Add("FCUSTID");
            e.FieldKeys.Add("FEntryNote");
            e.FieldKeys.Add("FWRITEOFFQTY");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            if (this.FormOperation.OperationId == 38)
            {
                SetStatusService setStatusService = new SetStatusService();
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                foreach (var dataEntity in e.DataEntitys)
                {
                    var billid = dataEntity["Id"];
                    var billNo = dataEntity["BillNo"];

                    List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();

                    var entryids = ((BeginSetStatusTransactionArgs)e).PkEntryIds;
                    if (entryids == null)
                    {
                        //预测单不存在整单关闭
                    }
                    else
                    {
                        foreach (var item in entryids)
                        {
                            var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_REQUIREMENTORDER");
                            //如果是行关闭，行相关联的需求单都关闭
                            string sSql = $@"SELECT FID FROM T_PLN_REQUIREMENTORDER WHERE FSALEORDERID='{item.Key}' AND FSALEORDERENTRYID='{item.Value}'";
                            var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            foreach (var plnitem in datas)
                            {
                                keyValuePairs.Add(new KeyValuePair<object, object>(plnitem["FID"], ""));
                            }
                            if (keyValuePairs.Count > 0)
                            {
                                setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
                            }
                            keyValuePairs.Clear();
                            //计划订单
                            view = FormMetadataUtils.CreateBillView(this.Context, "PLN_PLANORDER");
                            sSql = $@"SELECT t2.FID FROM T_PLN_PLANORDER_B t1 INNER JOIN T_PLN_PLANORDER t2 ON t1.FID=t2.FID
                            WHERE t1.FSALEORDERID={item.Key} AND t1.FSALEORDERENTRYID={item.Value} AND t2.FReleaseStatus IN (0,1,2)";
                            datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            foreach (var plnitem in datas)
                            {
                                keyValuePairs.Add(new KeyValuePair<object, object>(plnitem["FID"], ""));
                            }
                            if (keyValuePairs.Count > 0)
                            {
                                setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
                            }
                            var rowdo = ((DynamicObjectCollection)dataEntity["PLN_FORECASTENTRY"]).Where(x => Convert.ToInt64(x["Id"]) == Convert.ToInt64(item.Value)).FirstOrDefault();
                            string sendMessage = $@"#### 预测单关闭消息提醒请知悉-{ApigatewayUtils.ApigatewayConfig.EnvCode}]
>预测单号:<font color=""info"">{billNo}</font>
>预测单行号: <font color=""warning"">{rowdo["Seq"]}</font>
>蚂蚁型号:<font color=""comment"">{((DynamicObject)rowdo["MaterialID"])["Number"]}</font>
>剩余执行数量:<font color=""comment"">{Convert.ToDecimal(rowdo["Qty"]) - Convert.ToDecimal(rowdo["WRITEOFFQTY"])}</font>
>操作关闭人: <font color=""warning"">{this.Context.UserId}-{this.Context.UserName}</font>";

                            var createChat = new
                            {
                                ChatId = ApigatewayUtils.ApigatewayConfig.EnvCode + "AppGroupChatsFOClosed",
                                Name = ApigatewayUtils.ApigatewayConfig.EnvCode + "预测单关闭消息群",
                                Owner = "wangyang",
                                userlist = new string[] { "wangyang", "dengbingqiu" }
                            };
                            //创建群聊
                            ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/CreateChat", JsonConvertUtils.SerializeObject(createChat));

                            var WxEntity = new
                            {
                                //chatid = "productionAppGroupChatsFOClosed",
                                chatid = ApigatewayUtils.ApigatewayConfig.EnvCode + "AppGroupChatsFOClosed",
                                msgtype = "markdown",
                                markdown = new
                                {
                                    content = sendMessage
                                }
                            };
                            ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendChatMarkDownMessage", JsonConvertUtils.SerializeObject(WxEntity));
                            //ApigatewayUtils.InvokePostWebService($"weixinwork-Application/production/cgi-bin/appchat/send", JsonConvertUtils.SerializeObject(WxEntity));
                        }
                    }

                }
            }
        }


    }
}
