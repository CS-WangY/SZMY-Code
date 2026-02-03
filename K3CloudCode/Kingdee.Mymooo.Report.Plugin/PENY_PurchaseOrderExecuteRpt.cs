using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Purchase.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin
{
    [Description("采购订单执行明细表扩展"), HotUpdate]
    public class PENY_PurchaseOrderExecuteRpt : PurchaseOrderExecuteRpt
    {
        private string[] customRptTempTableNames;
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            IDBService dBService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            customRptTempTableNames = dBService.CreateTemporaryTableName(this.Context, 1);
            string strTable = customRptTempTableNames[0];

            base.BuilderReportSqlAndTempTable(filter, strTable);

            StringBuilder sb = new StringBuilder();
            string sSql = @"/*dialect*/select t1.*,t2.FPENYDELIVERYDATE AS FPENYSalDate into {0} from {1} t1
                                LEFT JOIN T_PUR_POORDERENTRY t2 ON t1.FORDERID=t2.FENTRYID";
            sb.AppendFormat(sSql, tableName, strTable);
            DBUtils.Execute(this.Context, sb.ToString());
        }

        public override void CloseReport()
        {
            if (customRptTempTableNames.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }
            IDBService dBService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            dBService.DeleteTemporaryTableName(this.Context, customRptTempTableNames);
            base.CloseReport();
        }
    }
}
