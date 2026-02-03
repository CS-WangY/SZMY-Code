using Kingdee.BOS;
using Kingdee.BOS.App.Core.Warn.Data;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Kingdee.Mymooo.ServicePlugIn.MrpModel.MrpCalingValidator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalOutstock
{
	[Description("销售出库单审核时解锁上游发货通知单锁库"), HotUpdate]
	public class Audit : AbstractOperationServicePlugIn
	{
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);

		}
		//审核增加运算校验
		public override void OnAddValidators(AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);
			e.Validators.Add(new IsMrpCalingValidator() { AlwaysValidate = true, EntityKey = "FBillHead" });
		}

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
				foreach (var detail in item["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection)
				{
					var newrows = new SyncOrderDeliveryRequest();
					newrows.BillNo = Convert.ToString(item["BillNo"]);

					newrows.DeliveryDate = System.DateTime.Now;
					string sSql = $@"SELECT t2.FORDERDETAILID,t2.FQTY-t3.FCANOUTQTY AS FCANOUTQTY FROM dbo.T_SAL_OUTSTOCKENTRY_R t1
                    LEFT JOIN T_SAL_ORDERENTRY t2 ON t1.FSOENTRYID=t2.FENTRYID
					LEFT JOIN T_SAL_ORDERENTRY_R t3 ON t2.FENTRYID=t3.FENTRYID
                    WHERE t1.FENTRYID={detail["Id"]}";
					var orderid = DBUtils.ExecuteDynamicObject(this.Context, sSql);
					foreach (var salide in orderid)
					{
						newrows.isCancel = 0;
						newrows.ActualQuantity = Convert.ToDecimal(salide["FCANOUTQTY"]);
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
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			SalOutStockToMesEntity entity = new SalOutStockToMesEntity();
			var sSql = $@"/*dialect*/select top 1 t1.FID Id,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,ORG.FNUMBER DeliveryOrgCode,ORGL.FNAME DeliveryOrgName,
                                t1.FCustomerID CustomerId,CUS.FNUMBER  CustomerCode,CUSL.FNAME CustomerName,
                                SALM.FWECHATCODE SalesManCode ,SALML.FNAME SalesManName,
                                t1.FNote PenyNote,t1.FLinkMan LinkMan,t1.FLinkPhone LinkPhone,t1.FReceiveAddress ReceiveAddress from T_SAL_OUTSTOCK t1
                                inner join T_ORG_ORGANIZATIONS ORG on t1.FSTOCKORGID=ORG.FORGID
                                inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
                                left join T_BD_CUSTOMER CUS on t1.FCustomerID=CUS.FCUSTID
                                left join T_BD_CUSTOMER_L CUSL on CUSL.FCUSTID=CUS.FCUSTID AND CUSL.FLOCALEID=2052
                                left join V_BD_SALESMAN SALM on t1.FSalesManID=SALM.fid
                                left join V_BD_SALESMAN_L SALML on SALM.fid=SALML.fid and SALML.FLOCALEID=2052
                                where t1.FID={fId}";
			var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
			foreach (var item in datas)
			{
				entity.Id = Convert.ToInt64(item["Id"]);
				entity.BillNo = Convert.ToString(item["BillNo"]);
				entity.Date = Convert.ToDateTime(item["Date"]);
				entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
				entity.DeliveryOrgCode = Convert.ToString(item["DeliveryOrgCode"]);
				entity.DeliveryOrgName = Convert.ToString(item["DeliveryOrgName"]);
				entity.CustomerId = Convert.ToInt64(item["CustomerId"]);
				entity.CustomerCode = Convert.ToString(item["CustomerCode"]);
				entity.CustomerName = Convert.ToString(item["CustomerName"]);
				entity.SalesManCode = Convert.ToString(item["SalesManCode"]);
				entity.SalesManName = Convert.ToString(item["SalesManName"]);
				entity.PenyNote = Convert.ToString(item["PenyNote"]);
				entity.LinkMan = Convert.ToString(item["LinkMan"]);
				entity.LinkPhone = Convert.ToString(item["LinkPhone"]);
				entity.ReceiveAddress = Convert.ToString(item["ReceiveAddress"]);
			}

			sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,t1.FCustItemNo CustItemNo,t1.FCustItemName CustItemName,t1.FCUSTMATERIALNO CustMaterialNo,t1.FCUSTPURCHASENO CustPurchaseNo,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FRealQty Qty,t3.FDeliveryDate DeliveryDate,sto.FNUMBER StockCode,stol.FNAME StockName,ORG.FNUMBER SupplyTargetOrgCode,orgl.FNAME SupplyTargetOrgName,
                    t1.FInsideRemark InsideRemark,t1.FProjectNo ProjectNo,t1.FStockFeatures StockFeatures,t1.FLocFactory LocFactory,t1.FNOTE NoteEntry,
                    t6.FID SaleOrderId,t6.FBILLNO SaleOrderNo,t5.FENTRYID OrderEntryId,t5.FSEQ OrderEntrySeq
                    from T_SAL_OUTSTOCKENTRY t1
					inner join T_SAL_OUTSTOCKENTRY_LK t2 on  t1.FENTRYID=t2.FENTRYID
					inner join T_SAL_DELIVERYNOTICEENTRY t3 on t2.FSID=t3.FENTRYID and t2.FSBILLID=t3.FID
                    left join T_SAL_DELIVERYNOTICEENTRY_LK t4 on t3.FENTRYID=t4.FENTRYID
                    left join T_SAL_ORDERENTRY t5 on t4.FSID=t5.FENTRYID
                    left join T_SAL_ORDER t6 on t5.FID=t6.FID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
                    left join t_BD_Stock sto on sto.FStockId=t1.FStockID
                    left join T_BD_STOCK_L stol on sto.FStockId=stol.FStockId and stol.FLOCALEID = 2052
                    inner join T_ORG_ORGANIZATIONS ORG on t1.FSupplyTargetOrgId=ORG.FORGID
                    inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
                    where t1.FID={fId} and t1.FSupplyTargetOrgId=7401803 and g.FIsSendMES=1  order by t1.FSEQ";
			datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
			entity.Details = new List<SalOutStockToMesDetEntity>();
			foreach (var item in datas)
			{
				entity.Details.Add(new SalOutStockToMesDetEntity
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
				});
			}

			entity.OperationNumber = "Audit";
			entity.FormId = "SAL_OUTSTOCK_MES";
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
			string sql = $@"select count(1) from T_SAL_OUTSTOCKENTRY t1
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
