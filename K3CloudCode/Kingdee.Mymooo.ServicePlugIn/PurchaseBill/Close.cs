using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.App.Core;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.App.Core.KafkaProducer;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单手工整单关闭"), HotUpdate]
	public class Close : AbstractOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FBillTypeID");
			e.FieldKeys.Add("FPurchaserId");
			e.FieldKeys.Add("FCLOSESTATUS");
			e.FieldKeys.Add("FCloserId");
			e.FieldKeys.Add("FMANUALCLOSE");
			e.FieldKeys.Add("FMRPCloseStatus");
		}
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			foreach (var item in e.DataEntitys)
			{
				//华东五部，不包含费用采购
				if (Convert.ToInt64(item["PURCHASEORGID_Id"]).Equals(7401803) && !Convert.ToString(item["BillTypeID_Id"]).EqualsIgnoreCase("b1985f24f35841fdb418329af6ed7bd0") && Convert.ToBoolean(item["MANUALCLOSE"]))
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
			MesPurchaseOrderCloseEntity entity = new MesPurchaseOrderCloseEntity();
			var sSql = $@"/*dialect*/select t1.FID Id,t1.FBILLNO BillNo,t2.FENTRYID EntryId,t2.FSEQ BillSeq,isnull(ul.FNAME,'') closeUserName
							from T_PUR_POORDER  t1
							inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
							left join T_SEC_USER ul on ul.FUSERID=t1.FCloserId
							where t1.FID={fId} and  FMRPCloseStatus='A' and FMANUALCLOSE='1' order by t2.FSEQ ";
			var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
			if (datas.Count() > 0)
			{
				entity.Id = Convert.ToInt64(datas[0]["Id"]);
				entity.BillNo = Convert.ToString(datas[0]["BillNo"]);
				entity.Details = new List<MesPurchaseOrderCloseDetEntity>();
				foreach (var item in datas)
				{
					entity.Details.Add(new MesPurchaseOrderCloseDetEntity
					{
						EntryId = Convert.ToInt64(item["EntryId"]),
						BillSeq = Convert.ToInt32(item["BillSeq"]),
						CloseUserName = Convert.ToString(item["CloseUserName"]),
					});
				}
			}
			entity.FormId = "PUR_PurchaseOrderManuallyClose";
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
