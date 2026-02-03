using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.CustMatMapping
{
    [Description("客户物料对应表服务端插件---保存校验")]
    [HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            e.FieldKeys.Add("FSaleOrgId");
            e.FieldKeys.Add("FCustomerId");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FCustMatNo");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<SqlObject> list = new List<SqlObject>();
            list.Add(new SqlObject("/*dialect*/ delete v from T_V_SAL_CUSTMATMAPPING v where not exists (select 1 from T_SAL_CUSTMATMAPPINGENTRY e where e.FENTRYID = v.FENTRYID)", new List<SqlParam>()));
            list.Add(new SqlObject("/*dialect*/ delete v from T_V_SAL_CUSTMATMAPPING_L v where not exists (select 1 from T_SAL_CUSTMATMAPPINGENTRY e where e.FENTRYID = v.FENTRYID)", new List<SqlParam>()));

            foreach (var item in e.DataEntitys)
            {
                list.Add(new SqlObject($@"/*dialect*/ update v
set fnumber = e.FCUSTMATNO
from T_V_SAL_CUSTMATMAPPING v
	inner join T_SAL_CUSTMATMAPPINGENTRY e on v.FENTRYID = e.FENTRYID
where e.FID = @FID", new SqlParam("@FID", KDDbType.Int64, item["Id"])));
                list.Add(new SqlObject($@"/*dialect*/ 
update v
set fname = el.FCUSTMATNAME
from T_V_SAL_CUSTMATMAPPING_L v
	inner join T_SAL_CUSTMATMAPPINGENTRY e on v.FENTRYID = e.FENTRYID
	inner join T_SAL_CUSTMATMAPPINGENTRY_L el on e.FENTRYID = el.FENTRYID
where e.FID = @FID", new SqlParam("@FID", KDDbType.Int64, item["Id"])));
            }

            list.Add(new SqlObject(@"/*dialect*/
Insert into T_V_SAL_CUSTMATMAPPING (fid,fheadfid,fnumber,FAUXPROPID,fcreateorgid,FUSEORGID,fcustomerid,FCREATORID,FCREATEDATE,FMODIFIERID,FMODIFYDATE,fdocumentstatus,fforbidstatus,FMATERIALID,FEFFECTIVE,FDEFCARRY,FISOLDVERSION,FENTRYID)
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), HEAD.FUSEORGID)) fid, HEAD.FID fheadfid, ENTRY.FCUSTMATNO fnumber, ENTRY.FAUXPROPID, HEAD.FSALEORGID fcreateorgid, 
HEAD.FUSEORGID, ISNULL(CUST.FMASTERID, 0) fcustomerid, HEAD.FCREATORID, HEAD.FCREATEDATE, HEAD.FMODIFIERID, HEAD.FMODIFYDATE, 'C' fdocumentstatus, 'A' fforbidstatus, ENTRY.FMATERIALID, ENTRY.FEFFECTIVE, ENTRY.FDEFCARRY, HEAD.FISOLDVERSION , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPING HEAD 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON HEAD.FID = ENTRY.FID 
INNER JOIN T_BD_CUSTOMER CUST ON CUST.FCUSTID = HEAD.FCUSTOMERID 
WHERE HEAD.FDOCUMENTSTATUS = 'A' and not exists (select 1 from T_V_SAL_CUSTMATMAPPING v where ENTRY.FENTRYID = v.FENTRYID and v.FUSEORGID = 1)
UNION ALL 
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), ISSUE.FISSUEORGID)) fid, HEAD.FID fheadfid, ENTRY.FCUSTMATNO fnumber, ENTRY.FAUXPROPID, HEAD.FSALEORGID fcreateorgid, 
ISSUE.FISSUEORGID fuseorgid, CUST.FMASTERID fcustomerid, HEAD.FCREATORID, HEAD.FCREATEDATE, HEAD.FMODIFIERID, HEAD.FMODIFYDATE, 'C'fdocumentstatus, 'A' fforbidstatus, MMASTER.FMATERIALID, ENTRY.FEFFECTIVE, ENTRY.FDEFCARRY, HEAD.FISOLDVERSION , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPING_ISSUE ISSUE 
	INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ISSUE.FID = HEAD.FID 
	INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON HEAD.FID = ENTRY.FID 
	INNER JOIN T_BD_MATERIAL M ON M.FMATERIALID = ENTRY.FMATERIALID 
	INNER JOIN T_BD_MATERIAL MMASTER ON (MMASTER.FMASTERID = M.FMASTERID AND (ISSUE.FISSUEORGID = MMASTER.FUSEORGID) )
	INNER JOIN T_BD_CUSTOMER CUST ON CUST.FCUSTID = HEAD.FCUSTOMERID 
WHERE HEAD.FDOCUMENTSTATUS = 'A' and not exists (select 1 from T_V_SAL_CUSTMATMAPPING v where ENTRY.FENTRYID = v.FENTRYID  and v.FUSEORGID = ISSUE.FISSUEORGID)", new List<SqlParam>()));

            list.Add(new SqlObject(@"/*dialect*/
Insert into T_V_SAL_CUSTMATMAPPING_L (fid,FLOCALEID,fname,fdescription,FENTRYID)
SELECT  ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), HEAD.FUSEORGID)) fid, L.FLOCALEID, L.FCUSTMATNAME fname, ' ' fdescription , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPINGENTRY_L L 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON L.FENTRYID = ENTRY.FENTRYID 
INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ENTRY.FID = HEAD.FID 
where  not exists (select 1 from T_V_SAL_CUSTMATMAPPING v inner join T_V_SAL_CUSTMATMAPPING_L vl on v.fid = vl.fid where ENTRY.FENTRYID = v.FENTRYID and v.FUSEORGID = 1)
UNION ALL 
SELECT ((((CONVERT(VARCHAR(80), HEAD.FID) + '&') + CONVERT(VARCHAR(80), ENTRY.FENTRYID)) + '&') + CONVERT(VARCHAR(80), ISSUE.FISSUEORGID)) fid, L.FLOCALEID, L.FCUSTMATNAME fname, ' ' fdescription , ENTRY.FENTRYID
FROM T_SAL_CUSTMATMAPPINGENTRY_L L 
INNER JOIN T_SAL_CUSTMATMAPPINGENTRY ENTRY ON L.FENTRYID = ENTRY.FENTRYID 
INNER JOIN T_SAL_CUSTMATMAPPING HEAD ON ENTRY.FID = HEAD.FID 
INNER JOIN T_SAL_CUSTMATMAPPING_ISSUE ISSUE ON ISSUE.FID = HEAD.FID 
where  not exists (select 1 from T_V_SAL_CUSTMATMAPPING v inner join T_V_SAL_CUSTMATMAPPING_L vl on v.fid = vl.fid where ENTRY.FENTRYID = v.FENTRYID and v.FUSEORGID = ISSUE.FISSUEORGID)", new List<SqlParam>()));

            DBUtils.ExecuteBatch(this.Context, list);
        }
    }
}
