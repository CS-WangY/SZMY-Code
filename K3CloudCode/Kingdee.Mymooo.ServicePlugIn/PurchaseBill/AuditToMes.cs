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

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单审核到MES"), HotUpdate]
	public class AuditToMes : AbstractOperationServicePlugIn
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
					//华东五部，不包含费用采购
					if (!Convert.ToString(item["BillTypeID_Id"]).EqualsIgnoreCase("b1985f24f35841fdb418329af6ed7bd0"))
					{
						messages.Add(SenMes(this.Context, fid));
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
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, fId) };
			PurchaseToMesEntity entity = new PurchaseToMesEntity();
			var sSql = $@"/*dialect*/select top 1 t1.FID ID,t1.FBillTypeID BillTypeID,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,
									ORG.FNUMBER PurchaseOrgCode,ORGL.FNAME PurchaseOrgName,
									BUY.FNUMBER PurchaserCode,BUYL.FNAME PurchaserName,
									t1.FSUPPLIERID SupplierId,SUPP.FNUMBER  SupplierCode,SUPPL.FNAME SupplierName,
									CONT.FContact Contact,CONT.FMobile Mobile,CONT.FTel Tel,t1.FNote Note
									from T_PUR_POORDER  t1
									inner join T_ORG_ORGANIZATIONS ORG on t1.FPURCHASEORGID=ORG.FORGID
									inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
									left join V_BD_BUYER BUY on t1.FPURCHASERID=BUY.fid
									left join V_BD_BUYER_L BUYL on BUYL.fid=BUY.fid AND BUYL.FLOCALEID=2052
									left join t_BD_Supplier SUPP on t1.FSUPPLIERID=SUPP.FSUPPLIERID
									left join T_BD_SUPPLIER_L SUPPL on SUPPL.FSUPPLIERID=SUPP.FSUPPLIERID and SUPPL.FLOCALEID=2052
									left join t_BD_SupplierContact  CONT on CONT.FContactId=t1.FPROVIDERCONTACTID
									where t1.FID=@FID";
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
				entity.Note = Convert.ToString(item["Note"]);
			}

			sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FQty Qty,t3.FTaxPrice TaxPrice,t3.FAllAmount AllAmount,
					t2.FDeliveryDate DeliveryDate,t1.FNote NoteEntry,t1.FPENYMAPCODE CustItemNo,t1.FPENYMAPNAME CustItemName,t1.FCUSTMATERIALNO CustMaterialNo,
					t1.FSupplierProductCode SupplierProductCode,t1.FSONO SoNo,t1.FSOSEQ SoSeq,t1.FPENYDeliveryDate SoDeliveryDate,
					t5.FBILLNO SrcBillNo ,t4.FSBILLID SrcId,t4.FSID SrcEntryId,t6.FSEQ SrcSeq,t7.FSrcBillTypeId SrcFormId,
					t1.FSOUNITPRICE SoUnitPrice,t1.FPENYSALERS SoSalers,t1.FVatProfitRate VatProfitRate
                    from T_PUR_POORDERENTRY t1
					left join T_PUR_POORDERENTRY_D t2 on t1.FENTRYID=t2.FENTRYID
					left join T_PUR_POORDERENTRY_F t3 on t1.FENTRYID=t3.FENTRYID
					left join T_PUR_POORDERENTRY_LK t4 on t1.FENTRYID=t4.FENTRYID
					left join T_PUR_REQUISITION t5 on t4.FSBILLID=t5.FID
					left join T_PUR_REQENTRY t6 on t4.FSBILLID=t6.FID and t4.FSID=t6.FENTRYID
					left join T_PUR_POORDERENTRY_R t7 on t1.FENTRYID=t7.FENTRYID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
					where t1.FID=@FID and g.FIsSendMES=1  order by t1.FSEQ ";
			datas = DBUtils.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
			entity.Details = new List<PurchaseToMesDetEntity>();
			foreach (var item in datas)
			{
				entity.Details.Add(new PurchaseToMesDetEntity
				{
					EntryId = Convert.ToInt64(item["EntryId"]),
					BillSeq = Convert.ToInt32(item["BillSeq"]),
					MaterialCode = Convert.ToString(item["MaterialCode"]),
					MaterialName = Convert.ToString(item["MaterialName"]),
					CustItemNo = Convert.ToString(item["CustItemNo"]),
					CustItemName = Convert.ToString(item["CustItemName"]),
					SupplierProductCode = Convert.ToString(item["SupplierProductCode"]),
					CustMaterialNo = Convert.ToString(item["CustMaterialNo"]),
					ParentSmallId = Convert.ToInt32(item["ParentSmallId"]),
					ParentSmallCode = Convert.ToString(item["ParentSmallCode"]),
					ParentSmallName = Convert.ToString(item["ParentSmallName"]),
					SmallId = Convert.ToInt32(item["SmallId"]),
					SmallCode = Convert.ToString(item["SmallCode"]),
					SmallName = Convert.ToString(item["SmallName"]),
					TaxPrice = Convert.ToDecimal(item["TaxPrice"]),
					Qty = Convert.ToDecimal(item["Qty"]),
					AllAmount = Convert.ToDecimal(item["AllAmount"]),
					DeliveryDate = Convert.ToDateTime(item["DeliveryDate"]),
					SoNo = Convert.ToString(item["SoNo"]),
					SoSeq = Convert.ToInt32(item["SoSeq"]),
					SoDeliveryDate = (Convert.ToString(item["SoDeliveryDate"])) == "" ? "" :
					(Convert.ToDateTime(item["SoDeliveryDate"]).ToString("yyyy-MM-dd HH:mm:ss").Contains("0001-01-01") ? "" :
					(Convert.ToDateTime(item["SoDeliveryDate"]).ToString("yyyy-MM-dd HH:mm:ss"))),
					NoteEntry = Convert.ToString(item["NoteEntry"]),
					SrcId = Convert.ToInt64(item["SrcId"]),
					SrcBillNo = Convert.ToString(item["SrcBillNo"]),
					SrcEntryId = Convert.ToInt64(item["SrcEntryId"]),
					SrcSeq = Convert.ToInt32(item["SrcSeq"]),
					SrcFormId = Convert.ToString(item["SrcFormId"]),
					SoUnitPrice = Convert.ToDecimal(item["SoUnitPrice"]),
					VatProfitRate = Convert.ToDecimal(item["VatProfitRate"]),
					SoSalers = Convert.ToString(item["SoSalers"]),
				});
			}
			entity.OperationNumber = "Audit";
			entity.FormId = "PUR_PurchaseOrder_MES";
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
			string sql = $@"/*dialect*/select count(1) from T_PUR_POORDERENTRY t1
							inner join T_PUR_POORDER t2 on t1.FID=t2.FID
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
