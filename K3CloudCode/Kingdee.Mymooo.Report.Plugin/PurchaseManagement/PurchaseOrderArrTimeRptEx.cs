using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Purchase.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.PurchaseManagement
{
    [Description("订单到货及时率报表"), HotUpdate]
    public class PurchaseOrderArrTimeRptEx : PurchaseOrderArrTimeRpt
    {
        private string[] tempTableNames;
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            IDBService dBService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            tempTableNames = dBService.CreateTemporaryTableName(this.Context, 1);
            string strTable = tempTableNames[0];
            base.BuilderReportSqlAndTempTable(filter, strTable);
            string sSql = string.Format(@"/*dialect*/SELECT t1.*,t2.FENTRYID,
CASE t2.FPENYDeliveryDate WHEN NULL THEN '1'
ELSE DATEADD(day,-1,t2.FPENYDeliveryDate) END AS FPENYSalDate,
(
SELECT
SUM(FREALQTY)
FROM
(
SELECT tsk.FPOORDERENTRYID,tsk.FREALQTY,ts.FDATE FROM T_STK_INSTOCKENTRY tsk
INNER JOIN T_STK_INSTOCK ts ON tsk.FID=ts.FID
WHERE ts.FDOCUMENTSTATUS='C'
) t3
WHERE
t3.FPOORDERENTRYID = t2.FENTRYID AND DATEADD(day,-1,t2.FPENYDeliveryDate)>=CAST(t3.FDATE AS DATE)
)/t1.FORDERQTY*100 AS FPENYArrivalTimeliness
,t3.FNAME AS FPARENTSMALLID,t4.FNAME AS FSMALLID
INTO {0} from {1} t1
LEFT JOIN 
(SELECT t1.FENTRYID,t1.FSEQ,t2.FBILLNO,FPENYDeliveryDate,t1.FPARENTSMALLID,t1.FSMALLID
FROM t_PUR_POOrderEntry t1 INNER JOIN t_PUR_POOrder t2 ON t1.FID=t2.FID) t2
ON t1.FBILLNO=t2.FBILLNO AND t1.FBILLSEQID=t2.FSEQ
LEFT JOIN dbo.T_BD_MATERIALGROUP_L t3 ON t2.FPARENTSMALLID=t3.FID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L t4 ON t2.FSMALLID=t4.FID
", tableName, strTable);
            DBUtils.Execute(this.Context, sSql);
        }
        public override void CloseReport()
        {
            if (tempTableNames.IsNullOrEmptyOrWhiteSpace())
            {
                base.CloseReport();
                return;
            }
            IDBService dBService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            dBService.DeleteTemporaryTableName(this.Context, tempTableNames);
            base.CloseReport();
        }
    }
}
