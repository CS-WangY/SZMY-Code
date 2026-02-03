using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.AR.ARMatchRecord
{
    [Description("应收核销记录审核插件"), HotUpdate]
	public class Save : AbstractRabbitMQOperationServicePlugIn
	{
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			var entrySql = @"select e.FSRCBILLNO SourceBillNo,e.FSRCBILLID SourceId,e.FSRCSEQ SourceSeq,e.FSRCROWID SourceEntryId,e.FSRCDATE SourceDate,e.FSOURCEFROMID SourceFromId,e.FCONTACTUNITTYPE ContactUnitType
,c.FNUMBER ContactUnitNumber,cl.fname ContactUnitName,org.FNUMBER OrganizationNumber,orgl.FNAME OrganizationName,dept.FNUMBER DepartmentNumber,deptl.FNAME DepartmentName
,sales.FWECHATCODE SalesNumber,salesl.FNAME SalesName,e.FPLANAMOUNT PlanAmount,e.FCURWRITTENOFFAMOUNT TheMatchAmount,e.FISADIBILL IsAdiBill
,e.FTARGETBILLID TargetId,e.FTARGETBILLNO TargetBillNO,e.FTARGETBILLSEQ TargetSeq,e.FTARGETENTRYID TargetEntryId,e.FTARGETFROMID TargetFromId
from T_AR_RECMacthLog o
	inner join T_AR_RECMACTHLOGENTRY e on o.FID = e.FID
	inner join V_FIN_CONTACTTYPE c on e.FCONTACTUNIT = c.FITEMID and e.FCONTACTUNITTYPE = c.FFORMID
	inner join V_FIN_CONTACTTYPE_L cl on c.FITEMID = cl.fitemid and cl.FLOCALEID = 2052
	inner join T_ORG_ORGANIZATIONS org on e.FSETTLEORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID and orgl.FLOCALEID = 2052
	left join T_BD_DEPARTMENT dept on e.FBUSINESSDEPTID = dept.FDEPTID
	left join T_BD_DEPARTMENT_L deptl on dept.FDEPTID = deptl.FDEPTID and deptl.FLOCALEID = 2052
	left join V_BD_SALESMAN sales on e.FBUSINESSERID = sales.fid
	left join V_BD_SALESMAN_L salesl on sales.fid = salesl.fid
where o.FID = @FID";
			
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var dataEntity in e.DataEntitys)
			{
				SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, dataEntity["Id"]);
				var data = new
				{
					Id = dataEntity["Id"],
					FormId = this.BusinessInfo.GetForm().Id,
					BillNo = dataEntity["BillNo"].ToString(),
					OperationNumber = this.FormOperation.Operation,
					Details = DBUtils.ExecuteDynamicObject(this.Context, entrySql, paramList: sqlParam)
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
