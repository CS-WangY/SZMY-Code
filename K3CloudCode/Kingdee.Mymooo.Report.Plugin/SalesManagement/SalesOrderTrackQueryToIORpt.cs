using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单跟踪出入库明细报表"), HotUpdate]
    public class SalesOrderTrackQueryToIORpt : SysReportBaseService
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
                if (OpenParameter.ContainsKey("FEntryId"))
                {
                    var FCGENTRYID = OpenParameter["FEntryId"].ToString();
                    string sql = $@"/*dialect*/ select {KSQL_SEQ},* into {tableName} from (
                    select  po.FBILLNO +'-' +convert(nvarchar(3),pod.FSEQ) FBILLNO,t1.FPOORDERENTRYID,t2.FDATE,t3.FNAME FSTOCKNAME,'收料通知单' FBUSINESSTYPE,t1.FACTRECEIVEQTY FQTY,
                    case when t2.FDOCUMENTSTATUS='Z' then '暂存' when t2.FDOCUMENTSTATUS='A' then '创建' 
                    when t2.FDOCUMENTSTATUS='B' then '审批中' when t2.FDOCUMENTSTATUS='C' then '已审核'
                    when t2.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,t2.FCREATEDATE
                    from T_PUR_RECEIVEENTRY t1
                    inner join T_PUR_RECEIVE t2 on t1.FID= t2.FID
                    left join t_BD_Stock_L t3 on t3.FSTOCKID=t1.FSTOCKID
                    left join T_PUR_POORDERENTRY pod on pod.FENTRYID=t1.FPOORDERENTRYID
					left join T_PUR_POORDER po on po.FID=pod.FID
                    where t1.FPOORDERENTRYID in ({FCGENTRYID})
                    union all
                    select  po.FBILLNO +'-' +convert(nvarchar(3),pod.FSEQ) FBILLNO,t1.FPOORDERENTRYID,t4.FDATE,t5.FNAME FSTOCKNAME,'检验单' FBUSINESSTYPE,t1.FACTRECEIVEQTY FQTY,
                    case when t4.FDOCUMENTSTATUS='Z' then '暂存' when t4.FDOCUMENTSTATUS='A' then '创建' 
                    when t4.FDOCUMENTSTATUS='B' then '审批中' when t4.FDOCUMENTSTATUS='C' then '已审核'
                    when t4.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,t4.FCREATEDATE
                    from T_PUR_RECEIVEENTRY t1
                    inner join T_QM_INSPECTBILLENTRY_LK t2 on t1.FID=t2.FSBILLID and t1.FENTRYID=t2.FSID
                    inner join T_QM_INSPECTBILLENTRY t3 on t2.FENTRYID= t3.FENTRYID
                    inner join T_QM_INSPECTBILL t4 on t4.FID=t3.FID
                    left join t_BD_Stock_L t5 on t3.FSTOCKID=t5.FSTOCKID
                    left join T_PUR_POORDERENTRY pod on pod.FENTRYID=t1.FPOORDERENTRYID
					left join T_PUR_POORDER po on po.FID=pod.FID
                    where t1.FPOORDERENTRYID  in ({FCGENTRYID})
                    union all
                    select  po.FBILLNO +'-' +convert(nvarchar(3),pod.FSEQ) FBILLNO,t1.FPOORDERENTRYID,t2.FDATE,t3.FNAME FSTOCKNAME,'采购入库单' FBUSINESSTYPE,t1.FREALQTY FQTY,
                    case when t2.FDOCUMENTSTATUS='Z' then '暂存' when t2.FDOCUMENTSTATUS='A' then '创建' 
                    when t2.FDOCUMENTSTATUS='B' then '审批中' when t2.FDOCUMENTSTATUS='C' then '已审核'
                    when t2.FDOCUMENTSTATUS='D' then '重新审核' else '' end FStatus,t2.FCREATEDATE
                    from T_STK_INSTOCKENTRY t1
                    inner join T_STK_INSTOCK t2 on t1.FID= t2.FID
                    left join t_BD_Stock_L t3 on t3.FSTOCKID=t1.FSTOCKID
                    left join T_PUR_POORDERENTRY pod on pod.FENTRYID=t1.FPOORDERENTRYID
					left join T_PUR_POORDER po on po.FID=pod.FID
                    where t1.FPOORDERENTRYID in ({FCGENTRYID})
                    ) t1 ";

                    sql = string.Format(sql, " FPOORDERENTRYID,FCREATEDATE ");
                    DBUtils.Execute(this.Context, sql);
                }
            }
        }

    }
}
