using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kingdee.Mymooo.ServicePlugIn.AR_Receivable
{
	[Description("应收单反审核插件"), HotUpdate]
	public class UnAudit : AbstractRabbitMQOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FByVerify");
		}

		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);

			var headSql = @"select o.FID Id,o.FBILLNO ReceivableNo,o.FDATE SalesOrderDate,c.FNUMBER CustomerNumber,cl.FNAME CustomerName,org.FNUMBER OrganizationNumber,orgl.FNAME OrganizationName
,dept.FNUMBER DepartmentNumber,deptl.FNAME DepartmentName,sales.FWECHATCODE SalesNumber,salesl.FNAME SalesName
from t_AR_receivable o
	inner join T_BD_CUSTOMER c on o.FCUSTOMERID = c.FCUSTID
	inner join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
	inner join T_ORG_ORGANIZATIONS org on o.FSALEORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID and orgl.FLOCALEID = 2052
	left join T_BD_DEPARTMENT dept on o.FSALEDEPTID = dept.FDEPTID
	left join T_BD_DEPARTMENT_L deptl on dept.FDEPTID = deptl.FDEPTID and deptl.FLOCALEID = 2052
	left join V_BD_SALESMAN sales on o.FSALEERID = sales.fid
	left join V_BD_SALESMAN_L salesl on sales.fid = salesl.fid
where o.FID = @FID ";
			var entryDatas = e.DataEntitys.Where(p => p.GetValue<string>("ByVerify", "") == "0" && (p["CUSTOMERID"] as DynamicObject).GetValue<long>("CorrespondOrgId_Id") == 0).ToList();
			
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			foreach (var dataEntitie in entryDatas)
			{
				SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, dataEntitie["Id"]);
				var data = new
				{
					Id = dataEntitie["Id"],
					FormId = this.BusinessInfo.GetForm().Id,
					BillNo = dataEntitie["BillNo"].ToString(),
					OperationNumber = this.FormOperation.Operation,
					Head = DBUtils.ExecuteDynamicObject(this.Context, headSql, paramList: sqlParam).FirstOrDefault(),
				};
                RabbitMQMessage message = new RabbitMQMessage()
                {
                    Exchange = "financeManagement",
                    Routingkey = data.FormId,
                    Keyword = data.BillNo,
                    Message = JsonConvertUtils.SerializeObject(data)
                };
                messages.Add(message);
            }
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            kafkaProducer.AddMessage(this.Context, messages.ToArray());
        }
	}
}
