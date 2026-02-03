using com.mymooo.credit.SDK;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Model.BussinessModel.K3Cloud;
using mymooo.core.Model.BussinessModel.K3Cloud.Finance;
using mymooo.core.Model.SqlSugarCore;
using mymooo.core.Utils.Service;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.SqlSugarCore.Finance;

namespace mymooo.k3cloud.business.Services
{
    /// <summary>
    /// 财务服务
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class FinanceService(KingdeeContent kingdeeContent, CreditServiceClient creditService, KafkaSendService<KingdeeContent, User> kafkaSendService)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        private readonly CreditServiceClient _creditService = creditService;
        private readonly KafkaSendService<KingdeeContent, User> _kafkaSendService = kafkaSendService;

        /// <summary>
        /// 初始化客户逾期数据
        /// </summary>
        /// <returns></returns>
        public async Task InitOverdueCache()
        {
            int pageIndex = 1;
            var query = _kingdeeContent.SqlSugar.Queryable<ArReceivable>().Where(p => p.TatolAmount > 0 && p.ByVerify == "0" && p.DocumentStatus == "C").Select(p => p.Id);
            var ids = query.ToOffsetPage(pageIndex++, 1000);
            var headSql = GetReceivableHeadSql();
            while (ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    Receivable? head = _kingdeeContent.SqlSugar.Ado.SqlQuery<Receivable>(headSql, new { FID = id }).FirstOrDefault();
                    if (head == null)
                    {
                        continue;
                    }
                    K3CloudRabbitMQMessage<Receivable, ReceivableDetail> message = new()
                    {
                        //MessageId = _kingdeeContent.RedisCache.StringIncrement<RabbitMQMessage>(),
                        Id = head.Id,
                        FormId = "AR_receivable",
                        BillNo = head.ReceivableNo,
                        OperationNumber = "Update",
                        Head = head,

                    };
                    var result = await _creditService.UpdateOverdue(message);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Message ?? result.ErrorMessage);
                    }
                }
                ids = query.ToOffsetPage(pageIndex++, 1000);
            }

        }

        /// <summary>
        /// 根据核销记录重新计算应收单的收款金额信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MatchRecord(K3CloudRabbitMQMessage<MatchRecord, MatchRecord> request, CancellationToken cancellationToken)
        {
            ResponseMessage<dynamic> response = new();
            if (request == null || request.Details == null || request.Details.Length == 0)
            {
                return response;
            }
            var headSql = GetReceivableHeadSql();
            var entrySql = GetReceivableDetailSql();
            List<RabbitMQMessage> rabbitMQMessages = [];
            foreach (var item in request.Details.Where(p => p.SourceFromId == "AR_receivable").Select(p => p.SourceId).Distinct())
            {
                Receivable? head = _kingdeeContent.SqlSugar.Ado.SqlQuery<Receivable>(headSql, new { FID = item }).FirstOrDefault();
                if (head == null)
                {
                    continue;
                }
                K3CloudRabbitMQMessage<Receivable, ReceivableDetail> message = new()
                {
                    Id = head.Id,
                    FormId = "AR_receivable",
                    BillNo = head.ReceivableNo,
                    OperationNumber = "Update",
                    Head = head,
                    Details = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<ReceivableDetail>(entrySql, new { FID = item })]
                };

                rabbitMQMessages.Add(_kafkaSendService.CreateMessage(_kingdeeContent, "financeManagement", message.FormId, message.BillNo, message));
            }
            _kingdeeContent.SqlSugar.Insertable(rabbitMQMessages).SplitTable().ExecuteCommand();
            _kingdeeContent.RabbitMQService.SendMessage(cancellationToken);
            return response;
        }
        private static string GetReceivableDetailSql()
        {
            return @"select e.FENTRYID EntryId,e.FSEQ Seq,m.FNUMBER MaterialNumber,ml.FNAME MaterialName,ml.FSPECIFICATION Specification,e.FCustItemNo CustomerMaterialNumber,e.FCustItemName CustomerMaterialName,e.FCUSTMATERIALNO CustomerMaterialNo
,isnull(msg.FNUMBER,wmsg.FNUMBER) SmallNumber,isnull(msgl.FNAME,wmsgl.FNAME) SmallName,isnull(pmsg.FNUMBER,wpmsg.FNUMBER) ParentSmallNumber,isnull(pmsgl.FNAME,wpmsgl.FNAME) ParentSmallName,e.FCUSTPURCHASENO CustPurchaseNo
,e.FPRICEQTY Qty,e.FTAXPRICE TaxPrice,e.FPRICE Price,e.FAllAmount Amount,e.FOPENQTY InvoiceQty,e.FOPENAMOUNT InvoiceAmount,e.FOPENSTATUS InvoiceStatus,e.FReceivableAmount ReceivableAmount,e.FReceivableStatus ReceivableStatus 
from t_AR_receivable o
	inner join T_AR_RECEIVABLEENTRY e on o.FID = e.FID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID	
	inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	left join T_BD_MATERIALSALGROUP msg on e.FSMALLID = msg.FID
	left join T_BD_MATERIALSALGROUP_L msgl on msg.FID = msgl.FID and msgl.FLOCALEID = 2052
	left join T_BD_MATERIALSALGROUP pmsg on e.FPARENTSMALLID = pmsg.FID
	left join T_BD_MATERIALSALGROUP_L pmsgl on pmsg.FID = pmsgl.FID and pmsgl.FLOCALEID = 2052
	left join T_BD_MATERIALGROUP wmsg on m.FMATERIALGROUP = wmsg.FID
	left join T_BD_MATERIALGROUP_L wmsgl on wmsg.FID = wmsgl.FID and wmsgl.FLOCALEID = 2052
	left join T_BD_MATERIALGROUP wpmsg on wmsg.FPARENTID = wpmsg.FID
	left join T_BD_MATERIALGROUP_L wpmsgl on wpmsg.FID = wpmsgl.FID and wpmsgl.FLOCALEID = 2052
where o.FID = @FID";
        }

        private static string GetReceivableHeadSql()
        {
            return @"select o.FID Id,o.FBILLNO ReceivableNo,o.FDATE ReceivableDate,o.FAPPROVEDATE AuditTime,c.FNUMBER CustomerNumber,cl.FNAME CustomerName,org.FNUMBER OrganizationNumber,orgl.FNAME OrganizationName
,dept.FNUMBER DepartmentNumber,deptl.FNAME DepartmentName,sales.FWECHATCODE SalesNumber,salesl.FNAME SalesName,o.FCancelStatus CancelStatus
,f.FENDDATE EndDate,f.FPAYAMOUNTFOR PayAmount,f.FPAYRATE PayRate,f.FWRITTENOFFSTATUS WriteOnOffStatus,f.FRELATEHADPAYAMOUNT JoinAmount,f.FWRITTENOFFAMOUNTFOR CollectionAmount
from t_AR_receivable o
	inner join t_AR_receivablePlan f on o.FID = f.FID
	inner join T_BD_CUSTOMER c on o.FCUSTOMERID = c.FCUSTID and c.FCORRESPONDORGID = 0
	inner join T_BD_CUSTOMER_L cl on c.FCUSTID = cl.FCUSTID and cl.FLOCALEID = 2052
	inner join T_ORG_ORGANIZATIONS org on o.FSALEORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID and orgl.FLOCALEID = 2052
	left join T_BD_DEPARTMENT dept on o.FSALEDEPTID = dept.FDEPTID
	left join T_BD_DEPARTMENT_L deptl on dept.FDEPTID = deptl.FDEPTID and deptl.FLOCALEID = 2052
	left join V_BD_SALESMAN sales on o.FSALEERID = sales.fid
	left join V_BD_SALESMAN_L salesl on sales.fid = salesl.fid
where o.FID = @FID  and o.FByVerify= '0' ";
        }
    }
}
