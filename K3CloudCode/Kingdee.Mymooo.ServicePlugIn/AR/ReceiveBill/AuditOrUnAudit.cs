using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.AR.ReceiveBill
{
    [Description("收款单审核反审核插件"), HotUpdate]
    public class AuditOrUnAudit : AbstractRabbitMQOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            var headSql = @"select r.FId Id,r.FBILLNO ReceiveBillNo,r.FDATE ReceiveDate,c.FNUMBER CustomerNumber,cl.FNAME CustomerName,org.FNUMBER OrganizationNumber,orgl.FNAME OrganizationName,dept.FNUMBER DepartmentNumber,deptl.FNAME DepartmentName
,sales.FWECHATCODE SalesNumber,salesl.FNAME SalesName,r.FRECEIVEAMOUNTFOR ReceiveAmount
from T_AR_RECEIVEBILL r
	inner join T_BD_CUSTOMER c on r.FCONTACTUNIT = c.FCUSTID and r.FCONTACTUNITTYPE = 'BD_CUSTOMER' and c.FCORRESPONDORGID = 0
	inner join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
	inner join T_ORG_ORGANIZATIONS org on r.FSALEORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID and orgl.FLOCALEID = 2052
	left join T_BD_DEPARTMENT dept on r.FSALEDEPTID = dept.FDEPTID
	left join T_BD_DEPARTMENT_L deptl on dept.FDEPTID = deptl.FDEPTID and deptl.FLOCALEID = 2052
	left join V_BD_SALESMAN sales on r.FSALEERID = sales.fid
	left join V_BD_SALESMAN_L salesl on sales.fid = salesl.fid
where r.FID = @FID";
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            //标准收款单
            var entryDatas = e.DataEntitys.Where(p => p.GetValue<string>("BillTypeID_Id", "") == "36cf265bd8c3452194ed9c83ec5e73d2").ToList();
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var dataEntity in entryDatas)
            {
                SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, dataEntity["Id"]);
                var head = DBUtils.ExecuteDynamicObject(this.Context, headSql, paramList: sqlParam).FirstOrDefault();
                if (head == null)
                {
                    continue;
                }
                var data = new
                {
                    Id = dataEntity["Id"],
                    FormId = this.BusinessInfo.GetForm().Id,
                    BillNo = dataEntity["BillNo"].ToString(),
                    OperationNumber = this.FormOperation.Operation,
                    Head = head
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
            kafkaProducer.AddMessage(this.Context, messages.ToArray());
        }
    }
}
