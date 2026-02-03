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
    [Description("销售订单跟踪加急收货申请记录"), HotUpdate]
    public class SalesOrderTrackQueryDeliveryLogRpt : SysReportBaseService
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
                    var FXSENTRYID = OpenParameter["FEntryId"].ToString();
                    string sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from T_PUR_UrgentDeliveryLog where FSoEntryId={FXSENTRYID}";
                    sql = string.Format(sql, " FCreateDate ");
                    DBUtils.Execute(this.Context, sql);
                }
            }
        }

    }
}
