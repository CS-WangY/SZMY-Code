using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.PurInStock
{
	[Description("采购入库审核到MES"), HotUpdate]
	public class Audit : AbstractOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FBillTypeID");
			e.FieldKeys.Add("FPurchaserId");

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
			}
			KafkaProducerService kafkaProducer = new KafkaProducerService();
			kafkaProducer.AddMessage(this.Context, messages.ToArray());
		}

		/// <summary>
		/// 发送给MES
		/// </summary>
		public RabbitMQMessage SenMes(Context ctx, long fId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, fId) };
			PurInStockToMesEntity entity = new PurInStockToMesEntity();
			var sSql = $@"/*dialect*/select top 1 t1.FID ID,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,
									ORG.FNUMBER PurchaseOrgCode,ORGL.FNAME PurchaseOrgName,
									BUY.FNUMBER PurchaserCode,BUYL.FNAME PurchaserName,
									t1.FSUPPLIERID SupplierId,SUPP.FNUMBER  SupplierCode,SUPPL.FNAME SupplierName,
									CONT.FContact Contact,CONT.FMobile Mobile,CONT.FTel Tel
									from T_STK_INSTOCK  t1
									inner join T_ORG_ORGANIZATIONS ORG on t1.FPURCHASEORGID=ORG.FORGID
									inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
									left join V_BD_BUYER BUY on t1.FPURCHASERID=BUY.fid
									left join V_BD_BUYER_L BUYL on BUYL.fid=BUY.fid AND BUYL.FLOCALEID=2052
									left join t_BD_Supplier SUPP on t1.FSUPPLIERID=SUPP.FSUPPLIERID
									left join T_BD_SUPPLIER_L SUPPL on SUPPL.FSUPPLIERID=SUPP.FSUPPLIERID and SUPPL.FLOCALEID=2052
									left join t_BD_SupplierContact  CONT on CONT.FContactId=t1.FPROVIDERCONTACTID
									where t1.FID=@FID ";
			var datas = DBUtils.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
			foreach (var item in datas)
			{
				entity.Id = Convert.ToInt64(item["Id"]);
				entity.BillNo = Convert.ToString(item["BillNo"]);
				entity.Date = Convert.ToDateTime(item["Date"]);
				entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
				entity.PurchaseOrgCode = Convert.ToString(item["PurchaseOrgCode"]);
				entity.PurchaseOrgName = Convert.ToString(item["PurchaseOrgName"]);
				entity.SupplierId = Convert.ToInt64(item["SupplierId"]);
				entity.SupplierCode = Convert.ToString(item["SupplierCode"]);
				entity.SupplierName = Convert.ToString(item["SupplierName"]);
				entity.PurchaserCode = Convert.ToString(item["PurchaserCode"]);
				entity.PurchaserName = Convert.ToString(item["PurchaserName"]);
				entity.Contact = Convert.ToString(item["Contact"]);
				entity.Tel = Convert.ToString(item["Tel"]);
				entity.Mobile = Convert.ToString(item["Mobile"]);
			}

			sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FREALQTY Qty,t2.FTaxPrice TaxPrice,t2.FAllAmount AllAmount,
					t1.FNote NoteEntry,t7.FBILLNO SrcBillNo ,t5.FSBILLID SrcId,t5.FSID SrcEntryId,t8.FSEQ SrcSeq
                    from T_STK_INSTOCKENTRY t1
					left join T_STK_INSTOCKENTRY_F t2 on t1.FENTRYID=t2.FENTRYID
					left join T_STK_INSTOCKENTRY_LK t3 on t1.FENTRYID=t3.FENTRYID
					left join T_PUR_RECEIVEENTRY t4 on t3.FSBILLID=t4.FID and t3.FSID=t4.FENTRYID
					left join T_PUR_RECEIVEENTRY_LK t5 on t4.FENTRYID=t5.FENTRYID
					left join T_PUR_POORDERENTRY_R t6 on t5.FSBILLID=t6.FID and t5.FSID=t6.FENTRYID
					left join T_PUR_POORDER t7 on t6.FID=t7.FID
					left join T_PUR_POORDERENTRY t8 on t7.FID=t8.FID and t6.FENTRYID=t8.FENTRYID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
					where t1.FID=@FID and g.FIsSendMES=1 order by t1.FSEQ ";
			datas = DBUtils.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
			entity.Details = new List<PurInStockToMesDetEntity>();
			foreach (var item in datas)
			{
				entity.Details.Add(new PurInStockToMesDetEntity
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
					TaxPrice = Convert.ToDecimal(item["TaxPrice"]),
					Qty = Convert.ToDecimal(item["Qty"]),
					AllAmount = Convert.ToDecimal(item["AllAmount"]),
					NoteEntry = Convert.ToString(item["NoteEntry"]),
					SrcId = Convert.ToInt64(item["SrcId"]),
					SrcBillNo = Convert.ToString(item["SrcBillNo"]),
					SrcEntryId = Convert.ToInt64(item["SrcEntryId"]),
					SrcSeq = Convert.ToInt32(item["SrcSeq"])
				});
			}
			entity.OperationNumber = "Audit";
			entity.FormId = "STK_InStock_MES";
			return new RabbitMQMessage()
			{
				Exchange = "purchaseManagement",
				Routingkey = entity.FormId,
				Keyword = entity.BillNo,
				Message = JsonConvertUtils.SerializeObject(entity)
			};
		}

		private bool IsHDFI(Context ctx, long fId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, fId) };
			string sql = $@"/*dialect*/select count(1) from T_STK_INSTOCKENTRY t1
							inner join T_STK_INSTOCK t2 on t1.FID=t2.FID
							inner join T_BD_MATERIALGROUP t3 on t1.FSMALLID=t3.FID
							where t1.FID=@FID and t2.FPURCHASEORGID=7401803 and t3.FIsSendMES=1 ";
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
