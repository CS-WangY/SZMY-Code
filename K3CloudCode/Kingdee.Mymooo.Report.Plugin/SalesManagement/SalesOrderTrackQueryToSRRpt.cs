using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单跟踪销退明细报表"), HotUpdate]
    public class SalesOrderTrackQueryToSRRpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            this.ReportProperty.IsGroupSummary = false;
        }


        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            if (filter.CustomParams.ContainsKey("OpenParameter"))
            {
                Dictionary<string, object> OpenParameter = (Dictionary<string, object>)filter.CustomParams["OpenParameter"];

                if (OpenParameter.ContainsKey("FId"))
                {
                    //销售ID
                    var FXSID = OpenParameter["FId"].ToString();
                    var FXSENTRYID = OpenParameter["FEntryId"].ToString();
                    string sql = $@"/*dialect*/ select {KSQL_SEQ},* into {tableName} from (
                    select t3.FBILLNO, t2.FSEQ,'发货通知单' FBILLTYPE,t2.FQTY,t3.FAPPROVEDATE,
                    case when t3.FDOCUMENTSTATUS='Z' then '暂存' when t3.FDOCUMENTSTATUS='A' then '创建' 
                    when t3.FDOCUMENTSTATUS='B' then '审批中' when t3.FDOCUMENTSTATUS='C' then '已审核'
                    when t3.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,
                    t3.FTrackingName,t3.FTrackingNumber,
                    t2.FNOTE,t3.FPICKINGCOMPLETEDATE,t3.FPACKAGINGCOMPLETEDATE,t3.FLOGISTICSRECEIVINGDATE,t3.FCUSTRECEIVINGDATE,t3.FCREATEDATE  
                    from T_SAL_DELIVERYNOTICEENTRY_LK t1
                    inner join T_SAL_DELIVERYNOTICEENTRY t2 on t1.FENTRYID= t2.FENTRYID
                    inner join T_SAL_DELIVERYNOTICE t3 on t3.FID= t2.FID
                    where  t1.FSBILLID= {FXSID} AND t1.FSID= {FXSENTRYID}
                    union all
                    select t3.FBILLNO, t2.FSEQ,'销售出库单' FBILLTYPE,t2.FREALQTY FQTY,t3.FAPPROVEDATE,
                    case when t3.FDOCUMENTSTATUS='Z' then '暂存' when t3.FDOCUMENTSTATUS='A' then '创建' 
                    when t3.FDOCUMENTSTATUS='B' then '审批中' when t3.FDOCUMENTSTATUS='C' then '已审核'
                    when t3.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,
                    t3.FTrackingName,t3.FTrackingNumber,
                    t2.FNOTE,t3.FPICKINGCOMPLETEDATE,t3.FPACKAGINGCOMPLETEDATE,t3.FLOGISTICSRECEIVINGDATE,t3.FCUSTRECEIVINGDATE,t3.FCREATEDATE  
                    from  T_SAL_OUTSTOCKENTRY_R t1
                    inner join T_SAL_OUTSTOCKENTRY t2 on t2.FENTRYID= t1.FENTRYID
                    inner join T_SAL_OUTSTOCK t3 on t3.FID= t2.FID
                    where t1.FSOENTRYID= {FXSENTRYID} 
                    union all
                    select t3.FBILLNO, t2.FSEQ,'退货通知单' FBILLTYPE,t2.FQTY,t3.FAPPROVEDATE,
                    case when t3.FDOCUMENTSTATUS='Z' then '暂存' when t3.FDOCUMENTSTATUS='A' then '创建' 
                    when t3.FDOCUMENTSTATUS='B' then '审批中' when t3.FDOCUMENTSTATUS='C' then '已审核'
                    when t3.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,
                    '' FTrackingName,'' FTrackingNumber,
                    t2.FDESCRIPTION FNOTE,null FPICKINGCOMPLETEDATE,null FPACKAGINGCOMPLETEDATE,null FLOGISTICSRECEIVINGDATE,null FCUSTRECEIVINGDATE,t3.FCREATEDATE  
                    from T_SAL_RETURNNOTICEENTRY t2 
                    inner join T_SAL_RETURNNOTICE t3 on t3.FID= t2.FID
                    where t2.FSOENTRYID= {FXSENTRYID} 
                    union all
                    select t3.FBILLNO, t2.FSEQ,'退货出库单' FBILLTYPE,t2.FREALQTY FQTY,t3.FAPPROVEDATE,
                    case when t3.FDOCUMENTSTATUS='Z' then '暂存' when t3.FDOCUMENTSTATUS='A' then '创建' 
                    when t3.FDOCUMENTSTATUS='B' then '审批中' when t3.FDOCUMENTSTATUS='C' then '已审核'
                    when t3.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,
                    '' FTrackingName,'' FTrackingNumber,
                    t2.FNOTE,null FPICKINGCOMPLETEDATE,null FPACKAGINGCOMPLETEDATE,null FLOGISTICSRECEIVINGDATE,null FCUSTRECEIVINGDATE,t3.FCREATEDATE  
                    from T_SAL_RETURNSTOCKENTRY t2 
                    inner join T_SAL_RETURNSTOCK t3 on t3.FID= t2.FID
                    where t2.FSOENTRYID= {FXSENTRYID} 
                    ) t1 ";
                    sql = string.Format(sql, " FCREATEDATE ");
                    DBUtils.Execute(this.Context, sql);
                }
            }
        }

    }
}
