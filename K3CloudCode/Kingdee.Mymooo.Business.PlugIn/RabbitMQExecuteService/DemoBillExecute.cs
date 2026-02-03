using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Bill;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.RabbitMQExecuteService
{
    public class DemoBillExecute : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            var context = LoginServiceUtils.BackgroundLogin(ctx);
            //RestoreCustomerMaterial(context);
            RestorePaymentBill(context);
        }

        private void RestorePaymentBill(Context ctx)
        {
            var sql = @"select p.FID,p.FPAYORGID 
from T_AP_PAYBILL p 
	inner join T_ORG_ORGANIZATIONS org on p.FPAYORGID = org.FORGID 
where org.FACCTORGTYPE = '2' and p.FDOCUMENTSTATUS = 'C'";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);

            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operateOption.SetVariableValue("RemoveValidators", true);
            if (datas.Count > 0)
            {
                FormMetadata paybillMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "AP_PAYBILL");
                var oper = BusinessDataServiceHelper.UnAudit(ctx, paybillMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), operateOption);

                if (oper.IsSuccess)
                {
                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update p
set p.FPAYORGID = e.FRELATIONORGID,FCONTACTUNIT = sn.FSUPPLIERID,FRECTUNIT = rsn.FSUPPLIERID
from T_AP_PAYBILL p 
	inner join T_ORG_ORGANIZATIONS org on p.FPAYORGID = org.FORGID 
	inner join t_org_bizrelationEntry e on e.FORGID = p.FPAYORGID
	inner join t_org_bizrelation o on e.FBIZRELATIONID = o.FBIZRELATIONID and o.FBRTypeId =108
	inner join T_BD_SUPPLIER s on p.FCONTACTUNIT = s.FSUPPLIERID and p.FCONTACTUNITTYPE = 'BD_Supplier'
	inner join T_BD_SUPPLIER sn on s.FMASTERID = sn.FMASTERID and sn.FUSEORGID = e.FRELATIONORGID
	inner join T_BD_SUPPLIER rs on p.FRECTUNIT = rs.FSUPPLIERID and p.FRECTUNITTYPE = 'BD_Supplier'
	inner join T_BD_SUPPLIER rsn on rs.FMASTERID = rsn.FMASTERID and rsn.FUSEORGID = e.FRELATIONORGID
where org.FACCTORGTYPE = '2' ");

                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update p
set p.FPAYORGID = e.FRELATIONORGID,FCONTACTUNIT = sn.FSUPPLIERID
from T_AP_PAYBILL p 
	inner join T_ORG_ORGANIZATIONS org on p.FPAYORGID = org.FORGID 
	inner join t_org_bizrelationEntry e on e.FORGID = p.FPAYORGID
	inner join t_org_bizrelation o on e.FBIZRELATIONID = o.FBIZRELATIONID and o.FBRTypeId =108
	inner join T_BD_SUPPLIER s on p.FCONTACTUNIT = s.FSUPPLIERID and p.FCONTACTUNITTYPE = 'BD_Supplier'
	inner join T_BD_SUPPLIER sn on s.FMASTERID = sn.FMASTERID and sn.FUSEORGID = e.FRELATIONORGID
where org.FACCTORGTYPE = '2'  and p.FRECTUNITTYPE = 'BD_Empinfo' ");

                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update pe
set FINNERACCOUNTID = inc.FID
from T_AP_PAYBILL p 
	inner join T_AP_PAYBILLENTRY pe on p.FID = pe.FId 
	inner join T_CN_INNERACCOUNT inc on p.FPAYORGID = inc.FUseOrgId and p.FSETTLEORGID = inc.FMAPPINGORGID
where p.FPAYORGID <> p.FSETTLEORGID and pe.FINNERACCOUNTID = 0");

                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update pe
set FACCOUNTID = bank.FMASTERID
from T_AP_PAYBILL p 
	inner join T_AP_PAYBILLENTRY pe on p.FID = pe.FId --and pe.FSETTLETYPEID = 231285
	inner join T_CN_BANKACNT bank on pe.FACCOUNTID = bank.FBANKACNTID 
where bank.FBANKACNTID <> bank.FMASTERID");

                    oper = BusinessDataServiceHelper.Submit(ctx, paybillMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), "Submit", operateOption);
                    oper = BusinessDataServiceHelper.Audit(ctx, paybillMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), operateOption);
                }
            }

            sql = @"select p.FID,p.FPAYORGID,p.FBILLNO
from T_AP_REFUNDBILL p 
	inner join T_ORG_ORGANIZATIONS org on p.FPAYORGID = org.FORGID 
where org.FACCTORGTYPE = '2' and p.FDOCUMENTSTATUS = 'C'";

            datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);

            if (datas.Count > 0)
            {
                FormMetadata refundMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "AP_REFUNDBILL");
                var oper = BusinessDataServiceHelper.UnAudit(ctx, refundMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), operateOption);

                if (oper.IsSuccess)
                {
                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update p
set p.FPAYORGID = e.FRELATIONORGID,FCONTACTUNIT = sn.FSUPPLIERID,FPAYUNIT = rsn.FSUPPLIERID
from T_AP_REFUNDBILL p 
	inner join T_ORG_ORGANIZATIONS org on p.FPAYORGID = org.FORGID 
	inner join t_org_bizrelationEntry e on e.FORGID = p.FPAYORGID
	inner join t_org_bizrelation o on e.FBIZRELATIONID = o.FBIZRELATIONID and o.FBRTypeId =108
	inner join T_BD_SUPPLIER s on p.FCONTACTUNIT = s.FSUPPLIERID and p.FCONTACTUNITTYPE = 'BD_Supplier'
	inner join T_BD_SUPPLIER sn on s.FMASTERID = sn.FMASTERID and sn.FUSEORGID = e.FRELATIONORGID
	inner join T_BD_SUPPLIER rs on p.FPAYUNIT = rs.FSUPPLIERID and p.FPAYUNITTYPE = 'BD_Supplier'
	inner join T_BD_SUPPLIER rsn on rs.FMASTERID = rsn.FMASTERID and rsn.FUSEORGID = e.FRELATIONORGID
where org.FACCTORGTYPE = '2'");

                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update pe
set FACCOUNTID = bank.FMASTERID
from T_AP_REFUNDBILL p 
	inner join T_AP_REFUNDBILLENTRY pe on p.FID = pe.FId --and pe.FSETTLETYPEID = 231285
	inner join T_CN_BANKACNT bank on pe.FACCOUNTID = bank.FBANKACNTID 
where bank.FBANKACNTID <> bank.FMASTERID");

                    DBServiceHelper.Execute(ctx, @"/*dialect*/
update pe
set FINNERACCOUNTID = inc.FID
from T_AP_REFUNDBILL p 
	inner join T_AP_REFUNDBILLENTRY pe on p.FID = pe.FId 
	inner join T_CN_INNERACCOUNT inc on p.FPAYORGID = inc.FUseOrgId and p.FSETTLEORGID = inc.FMAPPINGORGID
where p.FPAYORGID <> p.FSETTLEORGID and pe.FINNERACCOUNTID = 0");

                    oper = BusinessDataServiceHelper.Submit(ctx, refundMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), "Submit", operateOption);
                    oper = BusinessDataServiceHelper.Audit(ctx, refundMeta.BusinessInfo, datas.Select(p => p["FId"]).ToArray(), operateOption);
                }
            }
        }

        private static void RestoreCustomerMaterial(Context context)
        {
            var sql = @"select e.FMAPID,FSALEORGID,o.FCUSTID,e.FMATERIALID,m.FNUMBER,o.FBILLNO,e.FENTRYID,m.FMASTERID
from T_SAL_ORDER o
	inner join T_SAL_ORDERENTRY e on o.FID = e.FID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	left join T_V_SAL_CUSTMATMAPPING v on e.FMAPID = v.fid
where o.FSALEORGID = 224428 and v.fid is null";

            var custormMaterials = DBServiceHelper.ExecuteDynamicObject(context, sql);

            var orders = custormMaterials.GroupBy(p => Convert.ToString(p["FBILLNO"])).ToList();
            sql = "select FMessage from RabbitMQScheduledMessage where FKeyword = @FKeyword";
            var updateSql = "update T_SAL_ORDERENTRY set FMAPID=@FMAPID where FENTRYID=@FENTRYID";
            CustomerServcie customerServcie = new CustomerServcie();
            foreach (var order in orders)
            {
                string message = DBServiceHelper.ExecuteScalar(context, sql, "", new SqlParam("@FKeyword", KDDbType.String, order.Key));
                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }
                SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(message);
                var org = FormMetadataUtils.GetIdForNumber(context, new OrganizationsInfo(request.OrgNumber, ""));
                request.OrgId = org.Id;
                var customer = customerServcie.TryGetOrAdd(context, new CustomerInfo(request.CustomerInfo.Code, request.CustomerInfo.Name));

                var materials = MaterialServiceHelper.TryGetOrAdds(context, request.SalesOrderDetailList.GroupBy(r => r.ItemNo.Trim()).Select(p =>
                {
                    var material = p.First();
                    var materialInfo = new MaterialInfo(p.Key, material.ItemName);
                    materialInfo.ProductId = material.ProductId;
                    materialInfo.UseOrgId = request.OrgId;
                    materialInfo.ShortNumber = material.ShortNumber.Trim();
                    materialInfo.PriceType = material.PriceType;
                    materialInfo.ProductSmallClass = material.ProductSmallClass;
                    materialInfo.CustomerMaterialNumber = string.IsNullOrWhiteSpace(material.CustItemNo) ? material.ItemNo.Trim() : material.CustItemNo.Trim();
                    materialInfo.CustomerMaterialName = string.IsNullOrWhiteSpace(material.CustItemName) ? material.ItemName : material.CustItemName;

                    return materialInfo;
                }).ToArray(), new List<long>() { 224428 });
                var custMaterials = MaterialServiceHelper.TryGetOrAddCustMsterials(context, customer, materials);

                foreach (var custormMaterial in order)
                {
                    var material = materials.FirstOrDefault(p => p.Code.EqualsIgnoreCase(Convert.ToString(custormMaterial["FNUMBER"])));
                    if (material != null)
                    {
                        DBServiceHelper.Execute(context, updateSql, new List<SqlParam> { new SqlParam("@FMAPID", KDDbType.String, material.CustomerMaterialId), new SqlParam("@FENTRYID", KDDbType.Int64, custormMaterial["FENTRYID"]) });
                    }
                }
            }
        }
    }
}
