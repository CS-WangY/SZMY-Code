using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.QM.App.ReportPlugIn.Defect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.StockManagement
{
    [Description("供应商质量统计表扩展字段插件"), HotUpdate]
    public class QMSupplierQuaStatisticRpt : SupplierQuaStatisticRpt
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();

            this.IsCreateTempTableByPlugin = true;

        }
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            var sql = $" ALTER TABLE {tableName} ADD FPOBILLNO nvarchar(200);   ";
            DBUtils.Execute(this.Context, sql);
            //更新来源单号2
            sql = $@"/*dialect*/update t1 set t1.FPOBILLNO=t2.FPOBILLNO from {tableName} t1,
                    (select t1.FINSPECTBILLNUMBER,t1.FINSPECTBILLROWSEQ,
                    t7.FBILLNO+'-'+CONVERT(nvarchar(10),t6.FSEQ) FPOBILLNO from {tableName} t1
                    inner join T_QM_INSPECTBILL t2 on t1.FINSPECTBILLNUMBER=t2.FBILLNO
                    inner join T_QM_INSPECTBILLENTRY t3 on t2.FID=t3.FID and t3.FSEQ=t1.FINSPECTBILLROWSEQ
                    inner join T_QM_INSPECTBILLENTRY_LK t4 on t4.FENTRYID=t3.FENTRYID
                    inner join T_PUR_RECEIVEENTRY_LK t5 on t5.FENTRYID=t4.FSID
                    inner join T_PUR_POORDERENTRY t6 on t6.FID=t5.FSBILLID and t6.FENTRYID=t5.FSID
                    inner join T_PUR_POORDER t7 on t6.FID=t7.FID) t2 
                    where t1.FINSPECTBILLNUMBER=t2.FINSPECTBILLNUMBER and t1.FINSPECTBILLROWSEQ=t2.FINSPECTBILLROWSEQ ";
            DBUtils.Execute(this.Context, sql);
        }
    }
}
