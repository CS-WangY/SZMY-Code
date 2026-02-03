using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.CN.App.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.PurchaseManagement
{
    [Description("应收票据收发存明细表"), HotUpdate]
    public class CN_BillRectransactReportEx : ReceivableBillTransactReport
    {
        private string[] tempTableNames;
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            IDBService dBService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            tempTableNames = dBService.CreateTemporaryTableName(this.Context, 1);
            string strTable = tempTableNames[0];
            base.BuilderReportSqlAndTempTable(filter, strTable);
            string sSql = string.Format(@"SELECT t1.*,t2.FDISDATE,t4.FNUMBER as FPENYBankNumber,t5.FNAME as FPENYBankName,
t2.FDISCOUNTRATE,t2.FDISINTERESTAMOUNT,t2.FEXPENSESAMOUNT
,t1.FPARAMOUNTFOR-t2.FDISINTERESTAMOUNT-t2.FEXPENSESAMOUNT AS FPAIDAMNOUT
,t3.FCOMMENT,t6.FSOURRECBILL,t7.FENDORSEDATE,t7.FENDORSER
INTO {0} from {1} t1
LEFT JOIN T_CN_BILLRECEIVABLE t3 ON t1.FID=t3.FID
LEFT JOIN T_CN_BILLRECEIVABLE_D t2 ON t1.FID=t2.FID
LEFT JOIN T_CN_BANKACNT t4 ON t2.FDISRECBANKACNTID=t4.FBANKACNTID
LEFT JOIN T_BD_BANK_L t5 ON t4.FBANKID=t5.FBANKID
LEFT JOIN T_CN_BILLRECEIVABLE_O t6 ON t3.FID=t6.FID
LEFT JOIN 
(SELECT MAX(FENTRYID) FENTRYID,FID,FENDORSEDATE,FENDORSER FROM T_CN_BILLRECEIVABLEENDORSE GROUP BY FID,FENDORSEDATE,FENDORSER) t7 ON t3.FID=t7.FID", tableName, strTable);
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
