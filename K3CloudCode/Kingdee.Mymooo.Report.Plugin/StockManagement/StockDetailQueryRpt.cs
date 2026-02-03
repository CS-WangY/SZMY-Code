using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Stock.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.StockManagement
{
    [Description("物料收发明细扩展字段插件"), HotUpdate]
    public class StockDetailQueryRpt : StockDetailRpt
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();

            this.IsCreateTempTableByPlugin = true;

        }
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            var sql = $" ALTER TABLE {tableName} ADD FSRCBILLNO2 nvarchar(200); ";
            DBUtils.Execute(this.Context, sql);
            //更新来源单号2
             sql = $@"/*dialect*/update t1 set t1.FSRCBILLNO2=t2.FSRCBILLNO2 from {tableName} t1,
                        (select t1.FENTRYID,(t5.FBILLNO+'/'+t4.FDEMANDBILLNO) FSRCBILLNO2,'STK_InStock' FFORMID from T_STK_INSTOCKENTRY_LK t1
                        inner join T_PUR_RECEIVEENTRY t2 on t1.FSBILLID=t2.FID and t1.FSID=t2.FENTRYID
                        inner join T_PUR_RECEIVEENTRY_LK t3 on t2.FENTRYID=t3.FENTRYID
                        inner join T_PUR_POORDERENTRY_R t4 on t3.FSBILLID=t4.FID and t3.FSID=t4.FENTRYID
                        inner join T_PUR_POORDER t5 on t4.FID=t5.FID
                        union all
                        select t1.FENTRYID,FSOORDERNO FSRCBILLNO2,'SAL_OUTSTOCK' FFORMID from T_SAL_OUTSTOCKENTRY_R t1) t2
                        where t1.FBILLENTRYID=t2.FENTRYID and t1.FFORMID=t2.FFORMID ";
            DBUtils.Execute(this.Context, sql);
        }
    }
}
