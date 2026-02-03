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
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;

namespace Kingdee.Mymooo.ServicePlugIn.SAL_RETURNSTOCK
{
	[Description("销售退货单审核发MES"), HotUpdate]
	public class AuditToMes : AbstractOperationServicePlugIn
	{
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);

			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			foreach (var item in e.DataEntitys)
			{
				var fid = Convert.ToInt64(item["Id"]);
				//华东五部的，并且过滤大小类，需要传给MES
				if (IsHDFI(this.Context, fid))
				{
					messages.Add(SenMes(this.Context, fid));
				}
				//发送mq给平台发货信息
				var request = new List<SyncOrderDeliveryRequest>();
				foreach (var detail in item["SAL_RETURNSTOCKENTRY"] as DynamicObjectCollection)
				{
					var newrows = new SyncOrderDeliveryRequest();
					newrows.BillNo = Convert.ToString(item["BillNo"]);

					newrows.DeliveryDate = System.DateTime.Now;
					string sSql = $@"SELECT t2.FORDERDETAILID,t2.FQTY-t3.FCANOUTQTY AS FCANOUTQTY FROM dbo.T_SAL_OUTSTOCKENTRY_R t1
                    LEFT JOIN T_SAL_ORDERENTRY t2 ON t1.FSOENTRYID=t2.FENTRYID
					LEFT JOIN T_SAL_ORDERENTRY_R t3 ON t2.FENTRYID=t3.FENTRYID
                    WHERE t1.FENTRYID={detail["SOEntryId"]}";
					var orderid = DBUtils.ExecuteDynamicObject(this.Context, sSql);
					foreach (var salide in orderid)
					{
						newrows.isCancel = 1;
						newrows.ActualQuantity = Convert.ToInt64(salide["FCANOUTQTY"]);
						newrows.DetailId = Convert.ToInt64(salide["FORDERDETAILID"]);
					}

					request.Add(newrows);
				}
				messages.Add(
				new RabbitMQMessage()
				{
					Exchange = "mall-salesOrder",
					Routingkey = "SyncOrderDelivery",
					Keyword = Convert.ToString(item["BillNo"]),
					Message = JsonConvertUtils.SerializeObject(request)
				}
								);
			}
			KafkaProducerService kafkaProducer = new KafkaProducerService();
			kafkaProducer.AddMessage(this.Context, messages.ToArray());
		}

		/// <summary>
		/// 发送给MES
		/// </summary>
		public RabbitMQMessage SenMes(Context ctx, long fId)
		{
			ReturnBillToMesEntity entity = new ReturnBillToMesEntity();
			var sSql = $@"/*dialect*/select top 1 t1.FID ID,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,ORG.FNUMBER RetOrgCode,ORGL.FNAME RetOrgName,
                            t1.FRETCUSTID CustomerId,CUS.FNUMBER  CustomerCode,CUSL.FNAME CustomerName,
                            SALM.FWECHATCODE SalesManCode ,SALML.FNAME SalesManName,
                            t1.FLinkMan LinkMan,t1.FLinkPhone LinkPhone,t1.FReceiveAddress ReceiveAddress,
                            rrl.FDATAVALUE ReturnReason
                            from T_SAL_RETURNSTOCK t1
                            inner join T_ORG_ORGANIZATIONS ORG on t1.FStockOrgId=ORG.FORGID
                            inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
                            left join T_BD_CUSTOMER CUS on t1.FRETCUSTID=CUS.FCUSTID
                            left join T_BD_CUSTOMER_L CUSL on CUSL.FCUSTID=CUS.FCUSTID AND CUSL.FLOCALEID=2052
                            left join V_BD_SALESMAN SALM on t1.FSalesManID=SALM.fid
                            left join V_BD_SALESMAN_L SALML on SALM.fid=SALML.fid and SALML.FLOCALEID=2052
                            left join T_BAS_ASSISTANTDATAENTRY_L rrl  on rrl.FENTRYID=t1.FRETURNREASON and rrl.FLOCALEID=2052
                             where t1.FID={fId}";
			var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			foreach (var item in datas)
			{
				entity.Id = Convert.ToInt64(item["Id"]);
				entity.BillNo = Convert.ToString(item["BillNo"]);
				entity.Date = Convert.ToDateTime(item["Date"]);
				entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
				entity.RetOrgCode = Convert.ToString(item["RetOrgCode"]);
				entity.RetOrgName = Convert.ToString(item["RetOrgName"]);
				entity.CustomerId = Convert.ToInt64(item["CustomerId"]);
				entity.CustomerCode = Convert.ToString(item["CustomerCode"]);
				entity.CustomerName = Convert.ToString(item["CustomerName"]);
				entity.SalesManCode = Convert.ToString(item["SalesManCode"]);
				entity.SalesManName = Convert.ToString(item["SalesManName"]);
				entity.LinkMan = Convert.ToString(item["LinkMan"]);
				entity.LinkPhone = Convert.ToString(item["LinkPhone"]);
				entity.ReceiveAddress = Convert.ToString(item["ReceiveAddress"]);
				entity.ReturnReason = Convert.ToString(item["ReturnReason"]);
			}

			sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,
					t1.FCustItemNo CustItemNo,t1.FCustItemName CustItemName,t1.FCUSTMATERIALNO CustMaterialNo,t1.FCUSTPURCHASENO CustPurchaseNo,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FRealQty Qty,t1.FDeliveryDate DeliveryDate,sto.FNUMBER StockCode,stol.FNAME StockName,ORG.FNUMBER SupplyTargetOrgCode,orgl.FNAME SupplyTargetOrgName,
                    t1.FInsideRemark InsideRemark,t1.FProjectNo ProjectNo,t1.FStockFeatures StockFeatures,t1.FLocFactory LocFactory,t1.FNote NoteEntry,
                    t10.FID SaleOrderId,t10.FBILLNO SaleOrderNo,t9.FENTRYID OrderEntryId,t9.FSEQ OrderEntrySeq,rr.FNUMBER RmTypeCode,rrl.FDATAVALUE RmTypeName
                    from T_SAL_RETURNSTOCKENTRY t1
					inner join T_SAL_RETURNSTOCKENTRY_LK t2 on  t1.FENTRYID=t2.FENTRYID
					inner join T_SAL_RETURNNOTICEENTRY t3 on  t2.FSID=t3.FENTRYID and t2.FSBILLID=t3.FID
                    left join T_SAL_RETURNNOTICEENTRY_LK t4 on t3.FENTRYID=t4.FENTRYID
					left join T_SAL_OUTSTOCKENTRY  t5 on t5.FENTRYID=t4.FSID and t5.FID=t4.FSBILLID 
					left join T_SAL_OUTSTOCKENTRY_LK  t6 on t5.FENTRYID=t6.FENTRYID  
					left join T_SAL_DELIVERYNOTICEENTRY t7 on t7.FENTRYID=t6.FSID and t7.FID=t6.FSBILLID 
					left join T_SAL_DELIVERYNOTICEENTRY_LK  t8 on t7.FENTRYID=t8.FENTRYID 
                    left join T_SAL_ORDERENTRY t9 on t9.FENTRYID=t8.FSID and t9.FID=t8.FSBILLID 
                    left join T_SAL_ORDER t10 on t9.FID=t10.FID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
                    left join t_BD_Stock sto on sto.FStockId=t1.FSTOCKID
                    left join T_BD_STOCK_L stol on sto.FStockId=stol.FStockId and stol.FLOCALEID = 2052
                    inner join T_ORG_ORGANIZATIONS ORG on t1.FSupplyTargetOrgId=ORG.FORGID
                    inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
					left join T_BAS_ASSISTANTDATAENTRY rr  on rr.FENTRYID=t1.FReturnType 
					left join T_BAS_ASSISTANTDATAENTRY_L rrl  on rr.FENTRYID=rrl.FENTRYID and rrl.FLOCALEID=2052
					where t1.FID={fId} and t1.FSupplyTargetOrgId=7401803 and g.FIsSendMES=1 order by t1.FSEQ";
			datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			entity.Details = new List<ReturnBillToMesDetEntity>();
			foreach (var item in datas)
			{
				entity.Details.Add(new ReturnBillToMesDetEntity
				{
					EntryId = Convert.ToInt64(item["EntryId"]),
					BillSeq = Convert.ToInt32(item["BillSeq"]),
					MaterialCode = Convert.ToString(item["MaterialCode"]),
					MaterialName = Convert.ToString(item["MaterialName"]),
					CustItemNo = Convert.ToString(item["CustItemNo"]),
					CustItemName = Convert.ToString(item["CustItemName"]),
					CustMaterialNo = Convert.ToString(item["CustMaterialNo"]),
					CustPurchaseNo = Convert.ToString(item["CustPurchaseNo"]),
					ParentSmallId = Convert.ToInt32(item["ParentSmallId"]),
					ParentSmallCode = Convert.ToString(item["ParentSmallCode"]),
					ParentSmallName = Convert.ToString(item["ParentSmallName"]),
					SmallId = Convert.ToInt32(item["SmallId"]),
					SmallCode = Convert.ToString(item["SmallCode"]),
					SmallName = Convert.ToString(item["SmallName"]),
					Qty = Convert.ToDecimal(item["Qty"]),
					DeliveryDate = Convert.ToDateTime(item["DeliveryDate"]),
					StockCode = Convert.ToString(item["StockCode"]),
					StockName = Convert.ToString(item["StockName"]),
					SupplyTargetOrgCode = Convert.ToString(item["SupplyTargetOrgCode"]),
					SupplyTargetOrgName = Convert.ToString(item["SupplyTargetOrgName"]),
					InsideRemark = Convert.ToString(item["InsideRemark"]),
					ProjectNo = Convert.ToString(item["ProjectNo"]),
					StockFeatures = Convert.ToString(item["StockFeatures"]),
					LocFactory = Convert.ToString(item["LocFactory"]),
					NoteEntry = Convert.ToString(item["NoteEntry"]),
					SaleOrderId = Convert.ToInt64(item["SaleOrderId"]),
					SaleOrderNo = Convert.ToString(item["SaleOrderNo"]),
					OrderEntryId = Convert.ToInt64(item["OrderEntryId"]),
					OrderEntrySeq = Convert.ToInt32(item["OrderEntrySeq"]),
					RmTypeCode = Convert.ToString(item["RmTypeCode"]),
					RmTypeName = Convert.ToString(item["RmTypeName"]),
				});
			}
			entity.OperationNumber = "Audit";
			entity.FormId = "SAL_RETURNSTOCK_MES";
			return new RabbitMQMessage()
			{
				Exchange = "salesManagement",
				Routingkey = entity.FormId,
				Keyword = entity.BillNo,
				Message = JsonConvertUtils.SerializeObject(entity)
			};
		}


		private bool IsHDFI(Context ctx, long fId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, fId) };
			string sql = $@"select count(1) from T_SAL_RETURNSTOCKENTRY t1
							inner join T_BD_MATERIALGROUP t2 on t1.FSMALLID=t2.FID
							where t1.FID=@FID and t1.FSupplyTargetOrgId=7401803 and t2.FIsSendMES=1 ";
			return DBUtils.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
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
