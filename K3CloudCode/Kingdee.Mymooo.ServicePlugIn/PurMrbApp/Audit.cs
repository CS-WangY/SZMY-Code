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
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.App.Core.KafkaProducer;

namespace Kingdee.Mymooo.ServicePlugIn.PurMrbApp
{
	[Description("采购退料申请单审核"), HotUpdate]
	public class Audit : AbstractOperationServicePlugIn
	{
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			foreach (var item in e.DataEntitys)
			{
				if (Convert.ToInt64(item["APPORGID_Id"]).Equals(7401803))
				{
					messages.Add(SenMes(this.Context, Convert.ToInt64(item["Id"])));
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
			PurMrAppToMesEntity entity = new PurMrAppToMesEntity();
			var sSql = $@"/*dialect*/select top 1 t1.FID ID,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,ORG.FNUMBER AppOrgCode,ORGL.FNAME AppOrgName,
									t1.FSUPPLIERID SupplierId,SUPP.FNUMBER  SupplierCode,SUPPL.FNAME SupplierName,
									t1.FDESCRIPTION Note
									from T_PUR_MRAPP  t1
									inner join T_ORG_ORGANIZATIONS ORG on t1.FAPPORGID=ORG.FORGID
									inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
									left join t_BD_Supplier SUPP on t1.FSUPPLIERID=SUPP.FSUPPLIERID
									left join T_BD_SUPPLIER_L SUPPL on SUPPL.FSUPPLIERID=SUPP.FSUPPLIERID and SUPPL.FLOCALEID=2052
									where t1.FID={fId}";
			var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			foreach (var item in datas)
			{
				entity.Id = Convert.ToInt64(item["Id"]);
				entity.BillNo = Convert.ToString(item["BillNo"]);
				entity.Date = Convert.ToDateTime(item["Date"]);
				entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
				entity.AppOrgCode = Convert.ToString(item["AppOrgCode"]);
				entity.AppOrgName = Convert.ToString(item["AppOrgName"]);
				entity.SupplierId = Convert.ToInt64(item["SupplierId"]);
				entity.SupplierCode = Convert.ToString(item["SupplierCode"]);
				entity.SupplierName = Convert.ToString(item["SupplierName"]);
				entity.Note = Convert.ToString(item["Note"]);
			}

			sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FMrAppQty MrAppQty,sto.FNUMBER StockCode,stol.FNAME StockName,
                    t1.FNOTE NoteEntry,
					t2.FSBILLID SrcId,t2.FSID SrcEntryId,t1.FSRCBILLTYPEID SrcFormId,t3.FBILLNO SrcBillNo,t4.FSEQ SrcSeq
                    from T_PUR_MRAPPENTRY t1
                    left join T_PUR_MRAPPENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
                    left join t_BD_Stock sto on sto.FStockId=t1.FSTOCKID
                    left join T_BD_STOCK_L stol on sto.FStockId=stol.FStockId and stol.FLOCALEID = 2052
					left join T_STK_INSTOCK t3 on t3.FID=t2.FSBILLID
					left join T_STK_INSTOCKENTRY t4 on t4.FID=t2.FSBILLID and t4.FENTRYID=t2.FSID
					where t1.FID={fId} order by t1.FSEQ ";
			datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			entity.Details = new List<PurMrAppToMesDetEntity>();
			foreach (var item in datas)
			{
				entity.Details.Add(new PurMrAppToMesDetEntity
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
					MrAppQty = Convert.ToDecimal(item["MrAppQty"]),
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
			entity.FormId = "PUR_MRAPP";
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
