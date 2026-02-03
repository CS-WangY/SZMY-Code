using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.App.Data;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FSaleOrgId");//销售组织
            e.FieldKeys.Add("FDate");//发货通知单时间
            e.FieldKeys.Add("FCustomerID");//客户
            e.FieldKeys.Add("FReceiverContactID");//收货方联系人
            e.FieldKeys.Add("FReceiveAddress");//收货方地址
            e.FieldKeys.Add("FLinkMan");//收货人姓名
            e.FieldKeys.Add("FLinkPhone");//联系电话
            e.FieldKeys.Add("FCustMatID");//客户物料编码
            e.FieldKeys.Add("FCustMatName");//客户物料名称
            e.FieldKeys.Add("FCustItemNo");//客户物料编码(新)
            e.FieldKeys.Add("FCustItemName");//客户物料名称(新)
            e.FieldKeys.Add("FMateriaModel");//规格型号
            e.FieldKeys.Add("FSrcBillNo");//销售单号
            e.FieldKeys.Add("FSOEntryId");//销售订单EntryId
            e.FieldKeys.Add("FPROJECTNO");//项目号
            e.FieldKeys.Add("FCustMaterialNo");//客户料号
            e.FieldKeys.Add("FStockFeatures");//库存管理特征
            e.FieldKeys.Add("FBUSINESSDIVISIONID");//事业部
            e.FieldKeys.Add("FDeliveryOrgID");//发货组织
            e.FieldKeys.Add("FStockID");//出货仓库
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FCUSTPURCHASENO");//客户采购单号
            e.FieldKeys.Add("FPENYNOTE");//表头备注(用于云存储送货单备注)
            e.FieldKeys.Add("FNoteEntry");//明细销售备注(用于云存储销售备注)
            e.FieldKeys.Add("FSalesManID");//销售员
            e.FieldKeys.Add("FTaxPrice");//含税单价
            e.FieldKeys.Add("FCreatorId");//创建人
            e.FieldKeys.Add("FSpecialDelivery");//特殊发货
            e.FieldKeys.Add("FPackagingReq");//包装要求

        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            AuditValidator isPoValidator = new AuditValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            //foreach (var item in e.DataEntitys)
            //{
            //    var fid = Convert.ToInt64(item["Id"]);
            //    //var billtype = item["BillTypeID_Id"].ToString();
            //    var billno = item["BillNo"].ToString();
            //    if (Convert.ToInt64(item["SaleOrgId_Id"]) == 7207688)//华南三部
            //    {
            //        return;
            //    }
            //    messages.Add(SaleSendMQ(fid, billno));
            //}
            //if (messages.Count > 0)
            //{
            //    KafkaProducerService kafkaProducer = new KafkaProducerService();
            //    kafkaProducer.AddMessage(this.Context, messages.ToArray());
            //}
        }

        private RabbitMQMessage SaleSendMQ(long SalBillID, string SalBillNo)
        {
            var planentrySql = @"/*dialect*/SELECT t3.FBILLNO,t2.FQTY,t3.FAPPROVEDATE,sal.FORDERDETAILID FROM dbo.T_SAL_DELIVERYNOTICEENTRY_LK t1
INNER JOIN dbo.T_SAL_ORDERENTRY sal ON t1.FSID=sal.FENTRYID
INNER JOIN T_SAL_DELIVERYNOTICEENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_DELIVERYNOTICE t3 ON t2.FID=t3.FID
WHERE sal.FORDERDETAILID<>0 AND t3.FID=@FID";
            SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, SalBillID);
            var planentrys = DBUtils.ExecuteDynamicObject(this.Context, planentrySql, paramList: new SqlParam[] { sqlParam });
            var request = new List<SyncOrderDeliveryRequest>();
            foreach (var item in planentrys)
            {
                var newrows = new SyncOrderDeliveryRequest();
                newrows.BillNo = item.GetValue<string>("FBILLNO", string.Empty);
                newrows.isCancel = 0;
                newrows.ActualQuantity = item.GetValue<decimal>("FQTY", 0);
                newrows.DeliveryDate = item.GetValue<DateTime>("FAPPROVEDATE");
                newrows.DetailId = item.GetValue<long>("FORDERDETAILID", 0);
                request.Add(newrows);
            }

            return new RabbitMQMessage()
            {
                Exchange = "mall-salesOrder",
                Routingkey = "SyncOrderDelivery",
                Keyword = SalBillNo,
                Message = JsonConvertUtils.SerializeObject(request)
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
