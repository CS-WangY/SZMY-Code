using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单关闭-同时关闭组织间需求单"), HotUpdate]
    public class Closed : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FIsPayOnline");
            e.FieldKeys.Add("FBillAllAmount");
            e.FieldKeys.Add("FTaxNetPrice");
            e.FieldKeys.Add("FStockOutQty");
            e.FieldKeys.Add("FCUSTID");
            e.FieldKeys.Add("FEntryNote");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            if (this.FormOperation.OperationId == 38)
            {
                SetStatusService setStatusService = new SetStatusService();
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
                foreach (var dataEntity in e.DataEntitys)
                {
                    var billid = dataEntity["Id"];

                    int CancelType = 1;
                    List<long> longs = new List<long>();
                    List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();

                    var entryids = ((BeginSetStatusTransactionArgs)e).PkEntryIds;
                    if (entryids == null)
                    {
                        longs.Add(Convert.ToInt64(billid));
                        //组织间需求单
                        var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_REQUIREMENTORDER");
                        //如果是整单关闭，整单关联的需求单都关闭
                        string sSql = $@"SELECT FID FROM T_PLN_REQUIREMENTORDER WHERE FSALEORDERID='{billid}'";
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
                        WHERE t1.FSALEORDERID={billid} AND t2.FReleaseStatus IN (0,1,2)";
                        datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                        foreach (var plnitem in datas)
                        {
                            keyValuePairs.Add(new KeyValuePair<object, object>(plnitem["FID"], ""));
                        }
                        if (keyValuePairs.Count > 0)
                        {
                            setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
                        }

                    }
                    else
                    {
                        CancelType = 0;
                        foreach (var item in entryids)
                        {
                            longs.Add(Convert.ToInt64(item.Value));
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
                        }
                    }
                    messages.Add(SaleSendMQ(CancelType, longs));
                }
                KafkaProducerService kafkaProducer = new KafkaProducerService();
                kafkaProducer.AddMessage(this.Context, messages.ToArray());
            }
        }

        private RabbitMQMessage SaleSendMQ(int CancelType, List<long> longs)
        {
            var cancelRequest = new SalesOrderCancelRequest();
            cancelRequest.CancelType = CancelType;
            switch (CancelType)
            {
                case 0:
                    var sSqls = $"SELECT FORDERDETAILID FROM T_SAL_ORDERENTRY WHERE FENTRYID IN ({string.Join(",", longs)})";
                    var datas = DBUtils.ExecuteDynamicObject(this.Context, sSqls);
                    cancelRequest.orderDetails = datas.Select(x => Convert.ToInt64(x["FORDERDETAILID"])).ToList();
                    break;
                case 1:
                    sSqls = $"SELECT FBILLNO FROM T_SAL_ORDER WHERE FID IN ({string.Join(",", longs)})";
                    datas = DBUtils.ExecuteDynamicObject(this.Context, sSqls);
                    cancelRequest.OrderNumber = datas.Select(x => Convert.ToString(x["FBILLNO"])).First();
                    break;
            }
            return new RabbitMQMessage()
            {
                Exchange = "mall-salesOrder",
                Routingkey = "CancelOrderDetails",
                Keyword = "",
                Message = JsonConvertUtils.SerializeObject(cancelRequest)
            };
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                //晚5个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(5000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }
    }


}
