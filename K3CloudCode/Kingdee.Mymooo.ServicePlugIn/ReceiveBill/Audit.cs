using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.ServicePlugIn.SAL_RETURNSTOCK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.BOS.App.Core;
using System.Security.Cryptography;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using System.Data;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.Core.Metadata.EntityElement;

namespace Kingdee.Mymooo.ServicePlugIn.ReceiveBill
{
    [Description("收料通知单审核"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FISDIRECTDELIVERY");

        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var headEntity in e.DataEntitys)
            {
                long orgId = Convert.ToInt64(headEntity["STOCKORGID_Id"]);
                long fId = Convert.ToInt64(headEntity["Id"]);
                if (orgId.Equals(7401803))
                {
                    messages.Add(SenMes(this.Context, fId));
                }
                //全国一部或者华南二部，不含直发
                if ((orgId.Equals(7401780) || orgId.Equals(7401781)) && !Convert.ToBoolean(headEntity["FISDIRECTDELIVERY"]))
                {
                    List<ReceiveExemptionInStockEntity> selectedRows = new List<ReceiveExemptionInStockEntity>();
                    foreach (var item in headEntity["PUR_ReceiveEntry"] as DynamicObjectCollection)
                    {
                        if (!Convert.ToBoolean(item["CheckInComing"]))
                        {
                            selectedRows.Add(new ReceiveExemptionInStockEntity() { BillNo = Convert.ToString(headEntity["BillNo"]), PrimaryKeyValue = fId.ToString(), EntryPrimaryKeyValue = item["id"].ToString(), FormID = "PUR_ReceiveBill" });
                        }
                    }
                    if (selectedRows.Count() > 0)
                    {
                        //下推采购入库单,改MQ
                        messages.Add(new RabbitMQMessage
                        {
                            Exchange = "purchaseManagement",
                            Routingkey = "ReceiveExemptionInStock",
                            Keyword = Convert.ToString(headEntity["BillNo"]),
                            Message = JsonConvertUtils.SerializeObject(selectedRows)
                        });
                    }
                }
            }
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            kafkaProducer.AddMessage(this.Context, messages.ToArray());

        }

        /// <summary>
        /// 发送给MES
        /// </summary>
        public RabbitMQMessage SenMes(Context ctx, long fId)
        {
            ReceiveDeliveryToMesEntity entity = new ReceiveDeliveryToMesEntity();
            var sSql = $@"/*dialect*/select top 1 t1.FID ID,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,ORG.FNUMBER StockOrgCode,ORGL.FNAME StockOrgName,
									BUY.FNUMBER PurchaserCode,BUYL.FNAME PurchaserName,
									t1.FSUPPLIERID SupplierId,SUPP.FNUMBER  SupplierCode,SUPPL.FNAME SupplierName,
									CONT.FContact Contact,CONT.FMobile Mobile,CONT.FTel Tel,t1.FNote Note
									from T_PUR_RECEIVE  t1
									inner join T_ORG_ORGANIZATIONS ORG on t1.FSTOCKORGID=ORG.FORGID
									inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
									left join V_BD_BUYER BUY on t1.FPURCHASERID=BUY.fid
									left join V_BD_BUYER_L BUYL on BUYL.fid=BUY.fid AND BUYL.FLOCALEID=2052
									left join t_BD_Supplier SUPP on t1.FSUPPLIERID=SUPP.FSUPPLIERID
									left join T_BD_SUPPLIER_L SUPPL on SUPPL.FSUPPLIERID=SUPP.FSUPPLIERID and SUPPL.FLOCALEID=2052
									left join t_BD_SupplierContact  CONT on CONT.FContactId=t1.FPROVIDERCONTACTID
									where t1.FID=@fId";
            var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql, paramList: new SqlParam("@fId", KDDbType.Int64, fId));
            foreach (var item in datas)
            {
                entity.Id = Convert.ToInt64(item["Id"]);
                entity.BillNo = Convert.ToString(item["BillNo"]);
                entity.Date = Convert.ToDateTime(item["Date"]);
                entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
                entity.StockOrgCode = Convert.ToString(item["StockOrgCode"]);
                entity.StockOrgName = Convert.ToString(item["StockOrgName"]);
                entity.SupplierId = Convert.ToInt64(item["SupplierId"]);
                entity.SupplierCode = Convert.ToString(item["SupplierCode"]);
                entity.SupplierName = Convert.ToString(item["SupplierName"]);
                entity.PurchaserCode = Convert.ToString(item["PurchaserCode"]);
                entity.PurchaserName = Convert.ToString(item["PurchaserName"]);
                entity.Contact = Convert.ToString(item["Contact"]);
                entity.Tel = Convert.ToString(item["Tel"]);
                entity.Mobile = Convert.ToString(item["Mobile"]);
                entity.Note = Convert.ToString(item["Note"]);
            }

            sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FActReceiveQty ActReceiveQty,t1.FPreDeliveryDate PreDeliveryDate,sto.FNUMBER StockCode,stol.FNAME StockName,
                    t1.FPENYENTRYNOTE NoteEntry,
					t2.FSBILLID SrcId,t2.FSID SrcEntryId,t1.FSrcFormId SrcFormId,po.FBILLNO SrcBillNo,pod.FSEQ SrcSeq
                    from T_PUR_RECEIVEENTRY t1
                    left join T_PUR_RECEIVEENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
                    left join t_BD_Stock sto on sto.FStockId=t1.FSTOCKID
                    left join T_BD_STOCK_L stol on sto.FStockId=stol.FStockId and stol.FLOCALEID = 2052
					left join T_PUR_POORDER po on po.FID=t2.FSBILLID
					left join T_PUR_POORDERENTRY pod on pod.FID=t2.FSBILLID and pod.FENTRYID=t2.FSID
					where t1.FID=@fId order by t1.FSEQ ";
            datas = DBUtils.ExecuteDynamicObject(this.Context, sSql, paramList: new SqlParam("@fId", KDDbType.Int64, fId));
            entity.Details = new List<ReceiveDeliveryToMesDetEntity>();
            foreach (var item in datas)
            {
                entity.Details.Add(new ReceiveDeliveryToMesDetEntity
                {
                    EntryId = Convert.ToInt64(item["EntryId"]),
                    BillSeq = Convert.ToInt32(item["BillSeq"]),
                    MaterialCode = Convert.ToString(item["MaterialCode"]),
                    MaterialName = Convert.ToString(item["MaterialName"]),
                    ParentSmallId = Convert.ToInt32(item["ParentSmallId"]),
                    ParentSmallCode = Convert.ToString(item["ParentSmallCode"]),
                    ParentSmallName = Convert.ToString(item["ParentSmallName"]),
                    SmallId = Convert.ToInt32(item["SmallId"]),
                    SmallCode = Convert.ToString(item["SmallCode"]),
                    SmallName = Convert.ToString(item["SmallName"]),
                    ActReceiveQty = Convert.ToDecimal(item["ActReceiveQty"]),
                    PreDeliveryDate = Convert.ToDateTime(item["PreDeliveryDate"]),
                    StockCode = Convert.ToString(item["StockCode"]),
                    StockName = Convert.ToString(item["StockName"]),
                    NoteEntry = Convert.ToString(item["NoteEntry"]),
                    SrcId = Convert.ToInt64(item["SrcId"]),
                    SrcBillNo = Convert.ToString(item["SrcBillNo"]),
                    SrcEntryId = Convert.ToInt64(item["SrcEntryId"]),
                    SrcSeq = Convert.ToInt32(item["SrcSeq"]),
                    SrcFormId = Convert.ToString(item["SrcFormId"]),
                });
            }
            entity.OperationNumber = "Audit";
            entity.FormId = "PUR_ReceiveBill";
            return new RabbitMQMessage()
            {
                Exchange = "purchaseManagement",
                Routingkey = entity.FormId,
                Keyword = entity.BillNo,
                Message = JsonConvertUtils.SerializeObject(entity)
            };
        }

        /// <summary>
        /// 立即执行MQ
        /// </summary>
        /// <param name="e"></param>
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                //晚2个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(2000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }

    }
}
