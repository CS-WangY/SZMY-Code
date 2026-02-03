using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled.ChangeOrderTaskRequest;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单--关闭,反关闭,审核,变更"), HotUpdate]
    public class CreditOrderInfo : AbstractOperationServicePlugIn
    {
        public readonly Dictionary<string, string> rabbitCode = new Dictionary<string, string>();
        public readonly Dictionary<string, string> billIdKey = new Dictionary<string, string>();

        public CreditOrderInfo()
        {
            rabbitCode.Add("YLBillClose", "CloseSalesOrder");
            rabbitCode.Add("YLUnBillClose", "UnCloseSalesOrder");
            rabbitCode.Add("Audit", "AuditSalesOrder");
            rabbitCode.Add("TakeEffect", "ChangeSalesOrder");

            billIdKey.Add("YLBillClose", "Id");
            billIdKey.Add("YLUnBillClose", "Id");
            billIdKey.Add("Audit", "Id");
            billIdKey.Add("TakeEffect", "PKIDX");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            var headSql = @"select o.FBILLNO SalesOrderNo,o.FSalesOrderDate SalesOrderDate,o.FAuditTime AuditTime,c.FNUMBER CustomerNumber,cl.FNAME CustomerName,org.FNUMBER OrganizationNumber,orgl.FNAME OrganizationName
,o.FCustPurchaseNo CustPurchaseNo,f.FALLDISCOUNT Discount,dept.FNUMBER DepartmentNumber,deptl.FNAME DepartmentName,sales.FWECHATCODE SalesNumber,salesl.FNAME SalesName,t.FNUMBER SettleTypeNumber,tl.FNAME SettleTypeName
,o.FCloseStatus CloseStatus,o.FCancelStatus CancelStatus,o.FPlatCreatorType PlatCreatorType,o.FPlatCreatorId PlatCreatorName,case o.FBillTypeId when '63719cf361775e' then 'FB' else 'FA' end BillType,f.FBILLALLAMOUNT OriginalPaidAmount
from T_SAL_ORDER o
	inner join T_SAL_ORDERFIN f on o.FID = f.FID
	inner join T_BD_CUSTOMER c on o.FCUSTID = c.FCUSTID
	inner join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
	inner join T_ORG_ORGANIZATIONS org on o.FSALEORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID and orgl.FLOCALEID = 2052
	left join T_BD_DEPARTMENT dept on o.FSALEDEPTID = dept.FDEPTID
	left join T_BD_DEPARTMENT_L deptl on dept.FDEPTID = deptl.FDEPTID and deptl.FLOCALEID = 2052
	left join V_BD_SALESMAN sales on o.FSALERID = sales.fid
	left join V_BD_SALESMAN_L salesl on sales.fid = salesl.fid
	left join T_BD_RecCondition t on f.FRECCONDITIONID = t.FID
	left join T_BD_RecCondition_L tl on t.FID = tl.FID and tl.FLOCALEID = 2052
where o.FID = @FID ";
            var entrySql = @"select e.FENTRYID EntryId,e.FSEQ Seq,m.FNUMBER MaterialNumber,ml.FNAME MaterialName,e.FCustItemNo CustomerMaterialNumber,e.FCustItemName CustomerMaterialName,e.FCUSTMATERIALNO CustomerMaterialNo,e.FStockFeatures StockFeatures,e.FLocFactory LocFactory
,bu.FNUMBER BusinessDivisionNumber,bul.FDATAVALUE BusinessDivisionName,msg.FID SmallId,msg.FNUMBER SmallNumber,msgl.FNAME SmallName,pmsg.FNUMBER ParentSmallNumber,pmsgl.FNAME ParentSmallName
,e.FMrpTerminateStatus MrpTerminateStatus,e.FMrpCloseStatus MrpCloseStatus,ed.FDeliveryDate DeliveryDate
,case when e.FMrpTerminateStatus = 'B' or o.FCancelStatus = 'B' or o.FCloseStatus = 'B' or e.FMrpCloseStatus = 'B' then 0 else e.FQTY - er.FStockOutQty end NoOutQty,e.FQTY Qty,ef.FTaxNetPrice Price
,case when e.FMrpTerminateStatus = 'B' or o.FCancelStatus = 'B' then 0 when o.FCloseStatus = 'B' or e.FMrpCloseStatus = 'B' then round(er.FStockOutQty * ef.FTaxNetPrice,2) else ef.FALLAMOUNT end Amount
from T_SAL_ORDER o
	inner join T_SAL_ORDERENTRY e on o.FID = e.FID
	inner join T_SAL_ORDERENTRY_R er on e.FENTRYID = er.FENTRYID
	inner join T_SAL_ORDERENTRY_D ed on e.FENTRYID = ed.FENTRYID
	inner join T_SAL_ORDERENTRY_F ef on e.FENTRYID = ef.FENTRYID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID	
	inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	left join T_BAS_ASSISTANTDATAENTRY bu on e.FBusinessDivisionId = bu.FENTRYID
	left join T_BAS_ASSISTANTDATAENTRY_L bul on bu.FENTRYID = bul.FENTRYID and bul.FLOCALEID = 2052
	inner join T_BD_MATERIALSALE ms on m.FMATERIALID = ms.FMATERIALID
	left join T_BD_MATERIALSALGROUP msg on ms.FSALGROUP = msg.FID
	left join T_BD_MATERIALSALGROUP_L msgl on msg.FID = msgl.FID and msgl.FLOCALEID = 2052
	left join T_BD_MATERIALSALGROUP pmsg on msg.FPARENTID = pmsg.FID
	left join T_BD_MATERIALSALGROUP_L pmsgl on pmsg.FID = pmsgl.FID and pmsgl.FLOCALEID = 2052
where o.FID = @FID ";
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var dataEntity in e.DataEntitys)
            {
                SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, dataEntity[billIdKey[this.FormOperation.Operation]]);
                var head = DBUtils.ExecuteDynamicObject(this.Context, headSql, paramList: new SqlParam[] { sqlParam }).FirstOrDefault();
                if (head != null)
                {
                    ChangeOrderTaskRequest request = new ChangeOrderTaskRequest()
                    {
                        SalesOrderNo = head.GetValue<string>("SalesOrderNo", string.Empty),
                        Operation = this.FormOperation.Operation,
                        SalesName = head.GetValue<string>("SalesName", string.Empty),
                        SalesNumber = head.GetValue<string>("SalesNumber", string.Empty),
                        SettleTypeNumber = head.GetValue<string>("SettleTypeNumber", string.Empty),
                        SettleTypeName = head.GetValue<string>("SettleTypeName", string.Empty),
                        SalesOrderDate = head.GetValue<DateTime>("SalesOrderDate"),
                        CancelStatus = head.GetValue<string>("CancelStatus", string.Empty),
                        CloseStatus = head.GetValue<string>("CloseStatus", string.Empty),
                        CustomerName = head.GetValue<string>("CustomerName", string.Empty),
                        CustomerNumber = head.GetValue<string>("CustomerNumber", string.Empty),
                        CustPurchaseNo = head.GetValue<string>("CustPurchaseNo", string.Empty),
                        PlatCreatorName = head.GetValue<string>("PlatCreatorName", string.Empty),
                        PlatCreatorType = head.GetValue<string>("PlatCreatorType", string.Empty),
                        AuditTime = head.GetValue<DateTime?>("AuditTime", null),
                        BillType = head.GetValue<string>("BillType", string.Empty),
                        DepartmentName = head.GetValue<string>("DepartmentName", string.Empty),
                        DepartmentNumber = head.GetValue<string>("DepartmentNumber", string.Empty),
                        Discount = head.GetValue<decimal>("Discount", 0),
                        OrganizationName = head.GetValue<string>("OrganizationName", string.Empty),
                        OrganizationNumber = head.GetValue<string>("OrganizationNumber", string.Empty),
                        OriginalPaidAmount = head.GetValue<decimal>("OriginalPaidAmount", 0),
                        Details = new List<ChangeOrderTaskRequest.ChangeOrderTaskRequestDetails>()
                    };
                    var details = DBUtils.ExecuteDynamicObject(this.Context, entrySql, paramList: new SqlParam[] { sqlParam });
                    foreach (var detail in details)
                    {
                        request.Details.Add(new ChangeOrderTaskRequest.ChangeOrderTaskRequestDetails()
                        {
                            EntryId = detail.GetValue<long>("EntryId", 0),
                            Seq = detail.GetValue<int>("Seq", 0),
                            Amount = detail.GetValue<decimal>("Amount", 0),
                            BusinessDivisionName = detail.GetValue<string>("BusinessDivisionName", string.Empty),
                            BusinessDivisionNumber = detail.GetValue<string>("BusinessDivisionNumber", string.Empty),
                            CustomerMaterialName = detail.GetValue<string>("CustomerMaterialName", string.Empty),
                            CustomerMaterialNo = detail.GetValue<string>("CustomerMaterialNo", string.Empty),
                            CustomerMaterialNumber = detail.GetValue<string>("CustomerMaterialNumber", string.Empty),
                            DeliveryDate = detail.GetValue<DateTime>("DeliveryDate"),
                            LocFactory = detail.GetValue<string>("LocFactory", string.Empty),
                            MaterialName = detail.GetValue<string>("MaterialName", string.Empty),
                            MaterialNumber = detail.GetValue<string>("MaterialNumber", string.Empty),
                            MrpCloseStatus = detail.GetValue<string>("MrpCloseStatus", string.Empty),
                            MrpTerminateStatus = detail.GetValue<string>("MrpTerminateStatus", string.Empty),
                            NoOutQty = detail.GetValue<decimal>("NoOutQty", 0),
                            ParentSmallName = detail.GetValue<string>("ParentSmallName", string.Empty),
                            ParentSmallNumber = detail.GetValue<string>("ParentSmallNumber", string.Empty),
                            Price = detail.GetValue<decimal>("Price", 0),
                            Qty = detail.GetValue<decimal>("Qty", 0),
                            SmallId = detail.GetValue<long>("SmallId", 0),
                            SmallName = detail.GetValue<string>("SmallName", string.Empty),
                            SmallNumber = detail.GetValue<string>("SmallNumber", string.Empty),
                            StockFeatures = detail.GetValue<string>("StockFeatures", string.Empty)
                        });
                    }
                    request.PaidAmount = request.Details.Sum(p => p.Amount);
                    request.OrderPayment = GetPayInfo(Convert.ToInt64(dataEntity[billIdKey[this.FormOperation.Operation]]), request.SalesOrderNo);
                    RabbitMQMessage message = new RabbitMQMessage()
                    {
                        Exchange = "salesManagement",
                        Routingkey = rabbitCode[this.FormOperation.Operation],
                        Keyword = request.SalesOrderNo,
                        Message = JsonConvertUtils.SerializeObject(request)
                    };
                    messages.Add(message);
                }

            }
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            kafkaProducer.AddMessage(this.Context, messages.ToArray());
        }

        private SyncOrderPaymentRequest GetPayInfo(long SalBillID, string SalBillNo)
        {
            var planentrySql = @"SELECT 
CASE MAX(t4.FNUMBER)
WHEN 'WeChat' THEN 0
WHEN 'Alipay' THEN 1
ELSE 2 END PayType
,MAX(t5.FAPPROVEDATE) PaidDateTime
,SUM(t1.FAMOUNT) PaidAmount,tb.FBILLNO
FROM dbo.T_SAL_ORDERPLANENTRY t1
INNER JOIN T_SAL_ORDERPLAN t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_ORDER tb ON t2.FID=tb.FID
INNER JOIN T_AR_RECEIVEBILLENTRY t3 ON t1.FADVANCEENTRYID=t3.FENTRYID
INNER JOIN T_BD_SETTLETYPE t4 ON t3.FSETTLETYPEID=t4.FID
INNER JOIN T_AR_RECEIVEBILL t5 ON t3.FID=t5.FID
WHERE t2.FID=@FID
GROUP BY tb.FBILLNO";
            SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, SalBillID);
            var planentrys = DBUtils.ExecuteDynamicObject(this.Context, planentrySql, paramList: new SqlParam[] { sqlParam }).FirstOrDefault();
            var request = new SyncOrderPaymentRequest();
            request.OrderNumber = SalBillNo;
            if (planentrys != null)
            {
                request.PayType = planentrys.GetValue<int>("PayType", 0);
                request.PaidDateTime = planentrys.GetValue<DateTime?>("PaidDateTime", null);
                request.PaidAmount = planentrys.GetValue<decimal>("PaidAmount", 0);
                request.OrderNumber = planentrys.GetValue<string>("FBILLNO", string.Empty);
            }
            return request;
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
