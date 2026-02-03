using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.App.Core;
using Kingdee.K3.FIN.CN.App.Report;
using Kingdee.K3.FIN.Core;
using Kingdee.K3.FIN.Core.FilterCondition;
using static Kingdee.BOS.Core.Enums.BOSEnums;

namespace Kingdee.Mymooo.Report.Plugin.ReceivableManagement
{
    [Description("应收票据收发存明细表")]
    public class PENY_CN_BillRectransactReport : ReceivableBillReportBase
    {
        private string GroupLevelFieldName;

        private ReceivableBillTransactCondition FilterCondition { get; set; }

        public override void Initialize()
        {
            GroupLevelFieldName = ((AbstractSysReportServicePlugIn)this).ReportProperty.GroupSummaryInfoData.GroupLevelFieldName;
            ((AbstractSysReportServicePlugIn)this).ReportProperty.IsGroupSummary = true;
            SetDecimalControlField();
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0017: Expected O, but got Unknown
            //IL_001e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0028: Expected O, but got Unknown
            //IL_002f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0039: Expected O, but got Unknown
            //IL_0040: Unknown result type (might be due to invalid IL or missing references)
            //IL_004a: Expected O, but got Unknown
            //IL_0051: Unknown result type (might be due to invalid IL or missing references)
            //IL_005b: Expected O, but got Unknown
            //IL_0062: Unknown result type (might be due to invalid IL or missing references)
            //IL_006c: Expected O, but got Unknown
            //IL_0073: Unknown result type (might be due to invalid IL or missing references)
            //IL_007d: Expected O, but got Unknown
            //IL_0084: Unknown result type (might be due to invalid IL or missing references)
            //IL_008e: Expected O, but got Unknown
            //IL_0095: Unknown result type (might be due to invalid IL or missing references)
            //IL_009f: Expected O, but got Unknown
            //IL_00a6: Unknown result type (might be due to invalid IL or missing references)
            //IL_00b0: Expected O, but got Unknown
            //IL_00b7: Unknown result type (might be due to invalid IL or missing references)
            //IL_00c1: Expected O, but got Unknown
            //IL_00c8: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d2: Expected O, but got Unknown
            List<SummaryField> list = new List<SummaryField>();
            list.Add(new SummaryField("FPARAMOUNTFOR", (Enu_SummaryType)1));
            list.Add(new SummaryField("FRELATEDPAYMENT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FINITIALBALANCE", (Enu_SummaryType)1));
            list.Add(new SummaryField("FAMOUNTRECEIVED", (Enu_SummaryType)1));
            list.Add(new SummaryField("FENDORSEAMOUNT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FENDORSERETAMOUNT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FDUEPAYMENT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FDISCOUNT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FRETURNBILLAMOUNT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FBALANCE", (Enu_SummaryType)1));
            list.Add(new SummaryField("FPLEDGEAMT", (Enu_SummaryType)1));
            list.Add(new SummaryField("FISTRUSTAMT", (Enu_SummaryType)1));
            return list;
        }

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            //((AbstractSysReportServicePlugIn)this).BuilderReportSqlAndTempTable(filter, tableName);
            FilterCondition = GetReportFilter(filter);
            CreateTable(tableName);
            if (DateTimeFormatUtils.IsValidDate(FilterCondition.BizDateFrom) && DateTimeFormatUtils.IsValidDate(FilterCondition.BizDateTo))
            {
                FillReportTable(tableName);
            }
        }

        private void CreateTable(string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(@"CREATE TABLE {0}(
FBILLNO                     nvarchar(80),
FBILLNUMBER                 nvarchar(80),
FCURRENCYID                 int,
FCURRENCY_E                 nvarchar(80),
FAMOUNTDIGITS               int,
FDATE                       datetime,
FDUEDATE                    datetime,
FBILLTYPE                   varchar(20),
FRECBANK                    nvarchar(255),
FDAYSREMAINING              int,
FRECORGID                   int,
FRECORG_N                   nvarchar(255),
FSETTLEORGID                int,
FSETTLEORG_N                nvarchar(255),
FCONTACTUNITTYPE            varchar(36),
FCONTACTUNITTYPE_N          nvarchar(255),
FCONTACTUNIT                nvarchar(255),
FCONTACTUNIT_N              nvarchar(255),
FPARAMOUNTFOR               decimal(23, 10),
FRELATEDPAYMENT             decimal(38, 10),
FSETTLESTATUS               varchar(20),
FISSUEDATE                  datetime,
FPAYMENTPERIOD              int,
FPARRATE                    decimal(23, 10),
FDUEAMOUNTFOR               decimal(23, 10),
FDRAWER                     nvarchar(80),
FACCEPTOR                   nvarchar(80),
FACCEPTDATE                 datetime,
FINITIALBALANCE             decimal(38, 10),
FAMOUNTRECEIVED             decimal(38, 10),
FENDORSEAMOUNT              decimal(34, 10),
FENDORSERETAMOUNT           decimal(34, 10),
FDUEPAYMENT                 decimal(23, 10),
FDISCOUNT                   decimal(23, 10),
FRETURNBILLAMOUNT           decimal(23, 10),
FBALANCE                    decimal(38, 10),
FID                         int,
FNUMBER                     nvarchar(255),
FIDENTITYID                 int,
FBEENDORSEDUNITTYPE         nvarchar(100), 
FBEENDORSEDUNIT             nvarchar(100),
FPLEDGEAMT             decimal(23, 10),
FISTRUSTAMT             decimal(23, 10),
FRCBILLNUMBER                    nvarchar(255)
)", tableName);
            DBUtils.Execute(((AbstractSysReportServicePlugIn)this).Context, stringBuilder.ToString());
        }

        private void FillReportTable(string tableName)
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            //IL_060a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0610: Expected O, but got Unknown
            //IL_0668: Unknown result type (might be due to invalid IL or missing references)
            //IL_066f: Expected O, but got Unknown
            SqlParamStringBuilder filterString = GetFilterString();
            if (filterString == null)
            {
                return;
            }
            SqlParamStringBuilder val = new SqlParamStringBuilder();
            val.AppendFormat(" INSERT INTO {0}(FPLEDGEAMT,FISTRUSTAMT,FBEENDORSEDUNITTYPE,FBEENDORSEDUNIT ,FBILLNO,FBILLNUMBER,FCURRENCYID,FCURRENCY_E,FAMOUNTDIGITS,FDATE,FDUEDATE,FBILLTYPE,FRECBANK,FDAYSREMAINING,FRECORGID,FRECORG_N,FSETTLEORGID,FSETTLEORG_N,FCONTACTUNITTYPE,FCONTACTUNITTYPE_N,FCONTACTUNIT,FCONTACTUNIT_N,FPARAMOUNTFOR,FRELATEDPAYMENT,FSETTLESTATUS,FISSUEDATE,FPAYMENTPERIOD,FPARRATE,FDUEAMOUNTFOR,FDRAWER,FACCEPTOR,FACCEPTDATE,FINITIALBALANCE,FAMOUNTRECEIVED,FENDORSEAMOUNT,FENDORSERETAMOUNT,FDUEPAYMENT,FDISCOUNT,FRETURNBILLAMOUNT,FBALANCE,FID,FNUMBER, FRCBILLNUMBER,FIDENTITYID) ", new object[1] { tableName });
            val.AppendFormat(" SELECT  FPLEDGEAMT,FISTRUSTAMT,FBEENDORSEDUNITTYPE,  FBEENDORSEDUNIT , FBILLNO,FBILLNUMBER,FCURRENCYID,FCURRENCY_E,FAMOUNTDIGITS,FDATE,FDUEDATE,FBILLTYPE,FRECBANK,FDAYSREMAINING,FRECORGID,FRECORG_N,FSETTLEORGID,FSETTLEORG_N,FCONTACTUNITTYPE,FCONTACTUNITTYPE_N,FCONTACTUNIT,FCONTACTUNIT_N,FPARAMOUNTFOR,FRELATEDPAYMENT,FSETTLESTATUS,FISSUEDATE,FPAYMENTPERIOD,FPARRATE,FDUEAMOUNTFOR,FDRAWER,FACCEPTOR,FACCEPTDATE\r\n,ISNULL(FINITIALBALANCE - FPLEDGEINITAMT - FISTRUSTINITAMT,0) AS FINITIALBALANCE\r\n,ISNULL(FAMOUNTRECEIVED,0) AS FAMOUNTRECEIVED\r\n,ISNULL(FENDORSEAMOUNT,0) AS FENDORSEAMOUNT\r\n,ISNULL(FENDORSERETAMOUNT,0) AS FENDORSERETAMOUNT\r\n,ISNULL(FDUEPAYMENT,0) AS FDUEPAYMENT\r\n,ISNULL(FDISCOUNT,0) AS FDISCOUNT\r\n,ISNULL(FRETURNBILLAMOUNT,0) AS FRETURNBILLAMOUNT\r\n,ISNULL(FINITIALBALANCE - FPLEDGEINITAMT - FISTRUSTINITAMT,0) + ISNULL(FAMOUNTRECEIVED,0) - ISNULL(FENDORSEAMOUNT,0) - ISNULL(FRETURNBILLAMOUNT,0) - ISNULL(FDISCOUNT,0) - ISNULL(FDUEPAYMENT,0) + ISNULL(FENDORSERETAMOUNT,0)\r\n-ISNULL(FPLEDGEAMT,0) - ISNULL(FISTRUSTAMT,0) AS FBALANCE\r\n,FID,FNUMBER,FRCBILLNUMBER,{0} ", new object[1] { string.Format(((AbstractSysReportServicePlugIn)this).KSQL_SEQ, FilterCondition.OrderbyFields) });
            val.Append(" FROM (SELECT t.FBILLNO,t0.FRCBILLNUMBER,t.FBILLNUMBER,t.FCURRENCYID,curl.FNAME AS FCURRENCY_E,cur.FAMOUNTDIGITS,t.FDATE,t.FDUEDATE,t.FBILLTYPE,bl.FNAME as FRECBANK\r\n,CASE WHEN t.FSETTLESTATUS IN ('0','4') THEN DATEDIFF(DD,CURDATE(),t.FDUEDATE) ELSE NULL END AS FDAYSREMAINING\r\n,t.FPAYORGID AS FRECORGID,or2.FNAME AS FRECORG_N,t.FSETTLEORGID,or4.FNAME AS FSETTLEORG_N,t.FCONTACTUNITTYPE,obj1.FNAME AS FCONTACTUNITTYPE_N,mm1.FNUMBER AS FCONTACTUNIT,mm2.FNAME AS FCONTACTUNIT_N,t.FPARAMOUNTFOR\r\n,(SELECT ISNULL(SUM(t1.FUSEDAMOUNTFOR),0) FROM T_CN_BILLREFUNDRECENTRY t1 INNER JOIN T_AP_REFUNDBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FDOCUMENTSTATUS = 'C')\r\n+ (SELECT ISNULL(SUM(t1.FUSEDAMOUNTFOR),0) FROM T_AR_RECEIVEBILLREC t1 INNER JOIN T_AR_RECEIVEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FDOCUMENTSTATUS = 'C') \r\n+(SELECT ISNULL(SUM(t1.FREALRECAMOUNTFOR),0) FROM T_SC_RECEIVESETTLEBILLENTRY t1 INNER JOIN T_SC_RECEIVESETTLEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FDOCUMENTSTATUS = 'C') \r\n+(SELECT ISNULL(SUM(t1.FPARAMOUNTFOR),0) FROM T_SC_FUNDSUPENTRY t1 INNER JOIN T_SC_FUNDSUP t2 ON t1.FID = t2.FID WHERE t1.FID = t.FSOURCEFUNDSBILLID AND t.FSOURCEPATTERN='1' AND t2.FDOCUMENTSTATUS = 'C' AND t.FPARAMOUNTFOR=t1.FPARAMOUNTFOR) AS FRELATEDPAYMENT\r\n,t.FSETTLESTATUS,t.FISSUEDATE,t.FPAYMENTPERIOD,t.FPARRATE,t.FDUEAMOUNTFOR,t.FDRAWER,t.FACCEPTOR,t.FACCEPTDATE  ");
            if (FilterCondition.UseFDATE)
            {
                AppenInitAmountByDateSql(FilterCondition.BizDateFrom, val);
            }
            else
            {
                AppenInitAmountSql(FilterCondition.BizDateFrom, val);
            }
            if (FilterCondition.UseFDATE)
            {
                val.AppendFormat(" ,CASE WHEN t.FDATE >= {0} AND t.FDATE <= {1} AND T0.FSOURRECBILL=0  THEN t.FPARAMOUNTFOR ELSE 0 END AS FAMOUNTRECEIVED ", new object[2]
                {
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                });
            }
            else
            {
                val.AppendFormat(" ,(SELECT ISNULL(SUM(t1.FUSEDAMOUNTFOR),0) FROM T_CN_BILLREFUNDRECENTRY t1 INNER JOIN T_AP_REFUNDBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A'  AND t2.FDATE >= {0} AND t2.FDATE <= {1})\r\n+ (SELECT ISNULL(SUM(t1.FUSEDAMOUNTFOR),0) FROM T_AR_RECEIVEBILLREC t1 INNER JOIN T_AR_RECEIVEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A'  AND t2.FDATE >= {0} AND t2.FDATE <= {1})\r\n+ (SELECT ISNULL(SUM(t1.FREALRECAMOUNTFOR),0) FROM T_SC_RECEIVESETTLEBILLENTRY t1 INNER JOIN T_SC_RECEIVESETTLEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A'  AND t2.FDATE >= {0} AND t2.FDATE <= {1}) \r\n+ (SELECT ISNULL(SUM(t1.FPARAMOUNTFOR),0) FROM T_SC_FUNDSUPENTRY t1 INNER JOIN T_SC_FUNDSUP t2 ON t1.FID = t2.FID WHERE t1.FID = t.FSOURCEFUNDSBILLID AND t.FSOURCEPATTERN='1' AND t2.FDOCUMENTSTATUS = 'C'  AND t2.FDATE >= {0} AND t2.FDATE <= {1} AND t.FPARAMOUNTFOR=t1.FPARAMOUNTFOR) AS FAMOUNTRECEIVED ", new object[2]
                {
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                });
            }
            val.AppendFormat(" ,(SELECT case t0.FISSUPPORTSPLIT WHEN 1 THEN  ISNULL( SUM (t2.FENDORSEAMOUNT) ,0)   ELSE  ISNULL(COUNT(DISTINCT t1.FOPERINDENTIFY)*t.FPARAMOUNTFOR ,0)  END  FROM T_CN_BILLRECSETTLE t1 INNER JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID WHERE t1.FSETTLESTATUS = '3' AND t1.FDOCUMENTSTATUS = 'C' AND t1.FBILLRECID = t.FID AND t1.FDATE >= {0} AND t1.FDATE <= {1})  AS FENDORSEAMOUNT ", new object[2]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
            });
            val.AppendFormat(" ,(SELECT COUNT(DISTINCT t1.FOPERINDENTIFY)*t.FPARAMOUNTFOR FROM T_CN_BILLRECSETTLE t1 INNER JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID WHERE t1.FSETTLESTATUS = '4' AND t1.FDOCUMENTSTATUS = 'C' AND t1.FBILLRECID = t.FID AND t1.FDATE >= {0} AND t1.FDATE <= {1}) AS FENDORSERETAMOUNT ", new object[2]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
            });
            val.AppendFormat(" , (select  top 1 a.FNAME from   (SELECT max(t1.FDATE) as FDATE, obj2.FNAME ,t1.FBILLRECID  FROM T_CN_BILLRECSETTLE t1 left JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID  left join T_META_OBJECTTYPE_L obj2 on obj2.FID=t2.FBEENDORSEDUNITTYPE and obj2.FLOCALEID={2}  WHERE t1.FSETTLESTATUS  IN ( '4','3')  AND t1.FDOCUMENTSTATUS = 'C'  AND t1.FDATE >= {0} AND t1.FDATE <= {1}  group by  obj2.FNAME,t1.FBILLRECID ) a  WHERE  A.FBILLRECID = t.FID ) AS FBEENDORSEDUNITTYPE ", new object[3]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)((AbstractSysReportServicePlugIn)this).Context.UserLocale.LCID, '@')
            });
            val.AppendFormat(" ,(SELECT top 1 a.FNAME from ( SELECT max(t1.FDATE) as FDATE,typ2.FNAME,t1.FBILLRECID FROM T_CN_BILLRECSETTLE t1 left JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID left join V_FIN_CONTACTTYPE typ1 on t2.FBEENDORSEDUNIT=typ1.fitemid left join V_FIN_CONTACTTYPE_L typ2 on typ1.fitemid=typ2.fitemid and typ2.FLOCALEID={2}     WHERE t1.FSETTLESTATUS  IN ( '4','3')  AND t1.FDOCUMENTSTATUS = 'C'  AND t1.FDATE >= {0} AND t1.FDATE <= {1}  group by  typ2.FNAME,t1.FBILLRECID ) a WHERE  A.FBILLRECID = t.FID  ) AS FBEENDORSEDUNIT ", new object[3]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)((AbstractSysReportServicePlugIn)this).Context.UserLocale.LCID, '@')
            });
            val.AppendFormat(" ,case t0.FISSUPPORTSPLIT WHEN 1 THEN   (SELECT ISNULL( SUM (t2.FDUERECEIVEAMOUNT),0)  FROM T_CN_BILLRECSETTLE t1 INNER JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID WHERE t1.FSETTLESTATUS = '1' AND t1.FDOCUMENTSTATUS = 'C' AND t1.FBILLRECID = t.FID AND t1.FDATE >= {0} AND t1.FDATE <= {1})   ELSE CASE WHEN EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FSETTLESTATUS = '1' AND t1.FDATE >= {0} AND t1.FDATE <= {1}) THEN t.FPARAMOUNTFOR ELSE 0 END END  AS FDUEPAYMENT ", new object[2]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
            });
            val.AppendFormat(" ,case t0.FISSUPPORTSPLIT WHEN 1 THEN  (SELECT ISNULL( SUM (t2.FDISCOUNTAMOUNT),0) FROM T_CN_BILLRECSETTLE t1 INNER JOIN T_CN_BILLRECSETTLEENTRY t2 ON t1.FID = t2.FID WHERE t1.FSETTLESTATUS = '2' AND t1.FDOCUMENTSTATUS = 'C' AND t1.FBILLRECID = t.FID AND t1.FDATE >= {0} AND t1.FDATE <= {1})    ELSE  CASE WHEN EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FSETTLESTATUS = '2' AND t1.FDATE >= {0} AND t1.FDATE <= {1}) THEN t.FPARAMOUNTFOR ELSE 0 END END AS FDISCOUNT ", new object[2]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
            });
            val.AppendFormat(" ,CASE WHEN EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FSETTLESTATUS = '5' AND t1.FDATE >= {0} AND t1.FDATE <= {1}) THEN t.FPARAMOUNTFOR ELSE 0 END AS FRETURNBILLAMOUNT ", new object[2]
            {
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
            SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
            });
            if (FilterCondition.IsShowPledge)
            {
                val.AppendLine(" ,ISNULL(bt.PLEDGEAMT,0) AS FPLEDGEAMT,ISNULL(bti.FPLEDGEINITAMT,0) AS FPLEDGEINITAMT");
            }
            else
            {
                val.AppendLine(" ,0 AS FPLEDGEAMT,0 AS FPLEDGEINITAMT ");
            }
            if (FilterCondition.IsShowTrust)
            {
                val.AppendFormat(" ,CASE WHEN (o.FTRUSTEESHIPDATE >= {0} AND o.FTRUSTEESHIPDATE <={1}) AND ( o.FCANCLETRSHIPDATE IS NULL OR (o.FCANCLETRSHIPDATE < {0}              OR o.FCANCLETRSHIPDATE>{1})) THEN t.FPARAMOUNTFOR\r\n            WHEN ((o.FCANCLETRSHIPDATE >= {0} AND o.FCANCLETRSHIPDATE <={1})\r\n            AND ( o.FTRUSTEESHIPDATE IS NULL OR (o.FTRUSTEESHIPDATE < {0} OR o.FTRUSTEESHIPDATE>{1}))) THEN 0-t.FPARAMOUNTFOR  \r\n            ELSE 0 END AS FISTRUSTAMT ", new object[2]
                {
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                });
                val.AppendFormat(" ,ISNULL(\r\nCASE WHEN (o.FTRUSTEESHIPDATE < {0} AND ( o.FCANCLETRSHIPDATE IS NULL OR  o.FCANCLETRSHIPDATE>= {0})) THEN t.FPARAMOUNTFOR\r\nWHEN ( o.FCANCLETRSHIPDATE < {0}  AND ( o.FTRUSTEESHIPDATE IS NULL OR  o.FTRUSTEESHIPDATE>= {0})) THEN 0-t.FPARAMOUNTFOR  \r\nELSE 0 END,0) AS FISTRUSTINITAMT ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@') });
            }
            else
            {
                val.AppendFormat(" ,0 AS FISTRUSTAMT,0 AS FISTRUSTINITAMT ", new object[0]);
            }
            val.AppendFormat(" ,t.FID,mm1.FNUMBER\r\n FROM T_CN_BILLRECEIVABLE t\r\nINNER JOIN  T_CN_BILLRECEIVABLE_O t0 on t.fid=t0.fid   \r\nINNER JOIN V_FIN_CONTACTTYPE mm1 ON mm1.FITEMID = t.FCONTACTUNIT\r\nLEFT JOIN V_FIN_CONTACTTYPE_L mm2 ON mm1.FITEMID = mm2.FITEMID AND mm2.FLOCALEID = {0}\r\nLEFT JOIN T_META_OBJECTTYPE_L obj1 on obj1.FID = t.FCONTACTUNITTYPE AND obj1.FLOCALEID = {0}\r\nINNER JOIN T_ORG_ORGANIZATIONS or1 ON or1.FORGID = t.FPAYORGID\r\nLEFT JOIN T_ORG_ORGANIZATIONS_L or2 ON or1.FORGID = or2.FORGID AND or2.FLOCALEID = {0}\r\nLEFT JOIN T_ORG_ORGANIZATIONS or3 ON or3.FORGID = t.FSETTLEORGID\r\nLEFT JOIN T_ORG_ORGANIZATIONS_L or4 ON or3.FORGID = or4.FORGID AND or4.FLOCALEID = {0}\r\nINNER JOIN T_BD_CURRENCY cur ON cur.FCURRENCYID = t.FCURRENCYID\r\nLEFT JOIN T_BD_CURRENCY_L curl ON curl.FCURRENCYID = cur.FCURRENCYID AND curl.FLOCALEID = {0} \r\nLEFT JOIN T_BD_BANK_L bl ON bl.FBANKID = t.FRECBANKID AND bl.FLOCALEID = {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)((AbstractSysReportServicePlugIn)this).Context.UserLocale.LCID, '@') });
            if (FilterCondition.IsShowTrust)
            {
                val.AppendLine(" LEFT JOIN T_CN_BILLRECEIVABLE_O o ON t.FID = o.FID ");
            }
            if (FilterCondition.IsShowPledge)
            {
                string text = " LEFT JOIN (SELECT FRECBILLID,SUM(ISNULL(PLEDGEAMT,0)-ISNULL(CANCELPLEDGEAMT,0)) AS PLEDGEAMT \r\nFROM(SELECT brs.FRECBILLID,\r\nSUM(CASE WHEN brs.FBILLTYPEID = '{0}' THEN t.FPARAMOUNTFOR end) AS PLEDGEAMT,\r\nSUM(CASE WHEN brs.FBILLTYPEID = '{1}' THEN t.FPARAMOUNTFOR end) AS CANCELPLEDGEAMT\r\n FROM\r\n T_CN_BILLRECSETTLE brs \r\n INNER JOIN T_CN_BILLRECEIVABLE t ON brs.FRECBILLID= t.FID\r\n WHERE brs.FBILLTYPEID in ('{0}','{1}') AND brs.FDATE >={2} AND brs.FDATE <={3}\r\nAND t.FPAYORGID in ({4}) AND brs.FDocumentStatus = {5} \r\n GROUP BY brs.FBILLTYPEID,brs.FRECBILLID)tmp GROUP BY tmp.FRECBILLID) bt\r\n ON t.FID=bt.FRECBILLID ";
                val.AppendFormat(text, new object[6]
                {
                "5b03c31b827690",
                "5b03c4c98276f8",
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.RecOrgList, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)DocumentStatus.C.ToString(), '@')
                });
                text = " LEFT JOIN (\r\nSELECT FRECBILLID,SUM(ISNULL(PLEDGEAMT,0)-ISNULL(CANCELPLEDGEAMT,0)) AS FPLEDGEINITAMT \r\nFROM(SELECT brs.FRECBILLID,\r\nSUM(CASE WHEN brs.FBILLTYPEID = '{0}' THEN t.FPARAMOUNTFOR end) AS PLEDGEAMT,\r\nSUM(CASE WHEN brs.FBILLTYPEID = '{1}' THEN t.FPARAMOUNTFOR end) AS CANCELPLEDGEAMT\r\nFROM\r\nT_CN_BILLRECSETTLE brs \r\nINNER JOIN T_CN_BILLRECEIVABLE t ON brs.FRECBILLID= t.FID\r\nWHERE brs.FBILLTYPEID in ('{0}','{1}') AND brs.FDATE <{2} \r\nAND t.FPAYORGID IN ({3}) AND brs.FDocumentStatus = {4} \r\nGROUP BY brs.FBILLTYPEID,brs.FRECBILLID)tmp GROUP BY tmp.FRECBILLID) bti\r\nON t.FID=bti.FRECBILLID";
                val.AppendFormat(text, new object[5]
                {
                "5b03c31b827690",
                "5b03c4c98276f8",
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.RecOrgList, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)DocumentStatus.C.ToString(), '@')
                });
            }
            val.Append(" WHERE t.FDocumentStatus = 'C'  and  (t0.FSOURRECBILL=0 or (  t0.FSOURRECBILL>0 and   (t.FSETTLESTATUS not in ('8','0')  or  (t.FISTRUST='1' or t.FISPLEDGE='1')   )   )   )  ");
            if (FilterCondition.UseFDATE)
            {
                val.AppendFormat(" AND ((t.FDATE >= {0} AND t.FDATE <= {1}) OR t0.FSOURRECBILL>0   OR EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FDATE >= {0} AND t1.FDATE <= {1})\r\nOR (t.FDATE < {0} AND CASE WHEN NOT EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FDATE >= {0})\r\nTHEN CASE WHEN  t.FSETTLESTATUS='6' THEN '0' ELSE t.FSETTLESTATUS END \r\nELSE (CASE WHEN EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FSETTLESTATUS = '4' AND t1.FID = (SELECT MIN(t1.FID) FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FDATE >= {0}))\r\nTHEN '3' ELSE '0' END) END IN ('0','4','8'))) ", new object[2]
                {
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                });
            }
            else
            {
                SqlParamStringBuilder val2 = new SqlParamStringBuilder();
                if (FilterCondition.IsShowTrust)
                {
                    val2.AppendFormat(" OR (\r\n(t0.FTRUSTEESHIPDATE>={0} AND  t0.FTRUSTEESHIPDATE<={1} AND (t0.FCANCLETRSHIPDATE IS NULL OR t0.FCANCLETRSHIPDATE<{0} OR t0.FCANCLETRSHIPDATE>{1}))\r\nOR\r\n(t0.FCANCLETRSHIPDATE>={0} AND  t0.FCANCLETRSHIPDATE<={1} AND (t0.FTRUSTEESHIPDATE IS NULL OR t0.FTRUSTEESHIPDATE<{0} OR t0.FTRUSTEESHIPDATE>{1})\r\n)) ", new object[2]
                    {
                    SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                    SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                    });
                }
                SqlParamStringBuilder val3 = new SqlParamStringBuilder();
                if (FilterCondition.IsShowPledge)
                {
                    val3.AppendFormat(" OR(\r\n EXISTS(\r\n SELECT 1 FROM (\r\n SELECT T1.FBILLRECID,T1.FBILLTYPEID,\r\n CASE WHEN T1.FBILLTYPEID='{2}' THEN COUNT(*) ELSE 0 END AS PLEDGECOUNT,\r\n CASE WHEN T1.FBILLTYPEID='{3}' THEN COUNT(*) ELSE 0 END AS TRUSTCOUNT\r\n FROM \r\nT_CN_BILLRECSETTLE T1 \r\nWHERE T1.FDOCUMENTSTATUS='C' AND T1.FBILLTYPEID IN ('{2}','{3}')  AND T1.FDATE>={0} AND T1.FDATE<={1}\r\nGROUP BY FBILLRECID,FBILLTYPEID\r\n)TC GROUP BY FBILLRECID HAVING FBILLRECID=t.FID AND SUM(PLEDGECOUNT)<>SUM(TRUSTCOUNT)\r\n)) ", new object[4]
                    {
                    SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                    SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@'),
                    "5b03c31b827690",
                    "5b03c4c98276f8"
                    });
                }
                val.AppendFormat(" AND (EXISTS(SELECT 1 FROM T_CN_BILLREFUNDRECENTRY t1 INNER JOIN T_AP_REFUNDBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A' AND t2.FDATE >= {0} AND t2.FDATE <= {1}) \r\nOR EXISTS(SELECT 1 FROM T_AR_RECEIVEBILLREC t1 INNER JOIN T_AR_RECEIVEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A') \r\nOR EXISTS(SELECT 1 FROM T_SC_RECEIVESETTLEBILLENTRY t1 INNER JOIN T_SC_RECEIVESETTLEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A' ) \r\nOR EXISTS(SELECT 1 FROM T_SC_FUNDSUP t2 WHERE t2.FID = t.FSOURCEFUNDSBILLID AND t.FSOURCEPATTERN='1' AND t2.FDOCUMENTSTATUS = 'C' ) \r\nOR EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' ) OR t0.FSOURRECBILL>0 ", new object[2]
                {
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@'),
                SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateTo, '@')
                });
                val.Append(val2.Sql, val2.SqlParams);
                val.AppendFormat(" \r\nOR (CASE WHEN NOT EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FDATE >= {0})\r\nTHEN CASE WHEN t0.FISSUPPORTSPLIT=1 AND  t.FSETTLESTATUS='6' THEN '0' ELSE t.FSETTLESTATUS END \r\nELSE (CASE WHEN EXISTS(SELECT 1 FROM T_CN_BILLRECSETTLE t1 WHERE t1.FSETTLESTATUS = '4' AND t1.FID = (SELECT MIN(t1.FID) FROM T_CN_BILLRECSETTLE t1 WHERE t1.FBILLRECID = t.FID AND t1.FDocumentStatus = 'C' AND t1.FDATE >= {0}))\r\nTHEN '3' ELSE '0' END) END IN ('0','4','8')\r\nAND ((EXISTS(SELECT 1 FROM T_CN_BILLREFUNDRECENTRY t1 INNER JOIN T_AP_REFUNDBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A' AND t2.FDATE < {0})\r\nOR EXISTS(SELECT 1 FROM T_AR_RECEIVEBILLREC t1 INNER JOIN T_AR_RECEIVEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A' AND t2.FDATE < {0}) \r\nOR EXISTS(SELECT 1 FROM T_SC_RECEIVESETTLEBILLENTRY t1 INNER JOIN T_SC_RECEIVESETTLEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A' AND t2.FDATE < {0})\r\nOR EXISTS(SELECT 1 FROM T_SC_FUNDSUP t2 WHERE t2.FID = t.FSOURCEFUNDSBILLID AND t.FSOURCEPATTERN='1' AND t2.FDOCUMENTSTATUS = 'C' AND t2.FDATE < {0})) \r\nOR (t.FDATE < {0}\r\nAND NOT EXISTS(SELECT 1 FROM T_CN_BILLREFUNDRECENTRY t1 INNER JOIN T_AP_REFUNDBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A')\r\nAND NOT EXISTS(SELECT 1 FROM T_AR_RECEIVEBILLREC t1 INNER JOIN T_AR_RECEIVEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A') \r\nAND NOT EXISTS(SELECT 1 FROM T_SC_RECEIVESETTLEBILLENTRY t1 INNER JOIN T_SC_RECEIVESETTLEBILL t2 ON t1.FID = t2.FID WHERE t1.FBILLID = t.FID AND t2.FCANCELSTATUS ='A')\r\nAND NOT EXISTS(SELECT 1 FROM T_SC_FUNDSUP t2 WHERE t2.FID = t.FSOURCEFUNDSBILLID AND t.FSOURCEPATTERN='1' AND t2.FDOCUMENTSTATUS = 'C')\r\n) \r\n)) ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BizDateFrom, '@') });
                val.Append(val3.Sql, val3.SqlParams);
                val.Append(" ) ");
            }
            val.Append(" ) tt ");
            val.AppendFormat(" WHERE {0}", new object[1] { filterString });
            DBUtils.Execute(((AbstractSysReportServicePlugIn)this).Context, val.Sql, val.SqlParams);
            RemoveNullAmountData(tableName);
        }

        private SqlParamStringBuilder GetFilterString()
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Expected O, but got Unknown
            SqlParamStringBuilder val = new SqlParamStringBuilder();
            if (FilterCondition.RecOrgList.Count == 0)
            {
                return null;
            }
            val.AppendFormat(" FRECORGID IN ({0}) ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.RecOrgList, '@') });
            if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.ContactUnitType))
            {
                val.AppendFormat(" AND FCONTACTUNITTYPE = {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.ContactUnitType, '@') });
                if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.StartContactObj.Number))
                {
                    val.AppendFormat(" AND FNUMBER >= {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.StartContactObj.Number, '@') });
                }
                if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.EndContactObj.Number))
                {
                    val.AppendFormat(" AND FNUMBER <= {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.EndContactObj.Number, '@') });
                }
            }
            if (FilterCondition.Currency.Id != 0)
            {
                val.AppendFormat(" And FCURRENCYID = {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.Currency.Id, '@') });
            }
            if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.BillNoFrom))
            {
                val.AppendFormat(" AND FBILLNUMBER >= {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BillNoFrom, '@') });
            }
            if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.BillNoTo))
            {
                val.AppendFormat(" AND FBILLNUMBER <= {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.BillNoTo, '@') });
            }
            if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)FilterCondition.SettleState))
            {
                val.AppendFormat(" AND FSETTLESTATUS = {0} ", new object[1] { SqlParamItemUtils.ToSqlParamItem((object)FilterCondition.SettleState, '@') });
            }
            if (!string.IsNullOrWhiteSpace(FilterCondition.AdvanceFilterString))
            {
                val.AppendFormat(" AND ({0}) ", new object[1] { FilterCondition.AdvanceFilterString });
            }
            return val;
        }

        private void SetDecimalControlField()
        {
            //IL_0007: Unknown result type (might be due to invalid IL or missing references)
            //IL_000d: Expected O, but got Unknown
            //IL_002a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0030: Expected O, but got Unknown
            //IL_004d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0053: Expected O, but got Unknown
            //IL_0070: Unknown result type (might be due to invalid IL or missing references)
            //IL_0077: Expected O, but got Unknown
            //IL_0097: Unknown result type (might be due to invalid IL or missing references)
            //IL_009e: Expected O, but got Unknown
            //IL_00be: Unknown result type (might be due to invalid IL or missing references)
            //IL_00c5: Expected O, but got Unknown
            //IL_00e5: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ec: Expected O, but got Unknown
            //IL_010c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0113: Expected O, but got Unknown
            //IL_0133: Unknown result type (might be due to invalid IL or missing references)
            //IL_013a: Expected O, but got Unknown
            //IL_015a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0161: Expected O, but got Unknown
            //IL_0181: Unknown result type (might be due to invalid IL or missing references)
            //IL_0188: Expected O, but got Unknown
            //IL_01a8: Unknown result type (might be due to invalid IL or missing references)
            //IL_01af: Expected O, but got Unknown
            //IL_01cf: Unknown result type (might be due to invalid IL or missing references)
            //IL_01d6: Expected O, but got Unknown
            List<DecimalControlField> list = new List<DecimalControlField>();
            DecimalControlField val = new DecimalControlField();
            val.ByDecimalControlFieldName = "FPARAMOUNTFOR";
            val.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val);
            DecimalControlField val2 = new DecimalControlField();
            val2.ByDecimalControlFieldName = "FRELATEDPAYMENT";
            val2.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val2);
            DecimalControlField val3 = new DecimalControlField();
            val3.ByDecimalControlFieldName = "FDUEAMOUNTFOR";
            val3.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val3);
            DecimalControlField val4 = new DecimalControlField();
            val4.ByDecimalControlFieldName = "FINITIALBALANCE";
            val4.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val4);
            DecimalControlField val5 = new DecimalControlField();
            val5.ByDecimalControlFieldName = "FAMOUNTRECEIVED";
            val5.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val5);
            DecimalControlField val6 = new DecimalControlField();
            val6.ByDecimalControlFieldName = "FENDORSEAMOUNT";
            val6.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val6);
            DecimalControlField val7 = new DecimalControlField();
            val7.ByDecimalControlFieldName = "FENDORSERETAMOUNT";
            val7.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val7);
            DecimalControlField val8 = new DecimalControlField();
            val8.ByDecimalControlFieldName = "FDUEPAYMENT";
            val8.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val8);
            DecimalControlField val9 = new DecimalControlField();
            val9.ByDecimalControlFieldName = "FDISCOUNT";
            val9.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val9);
            DecimalControlField val10 = new DecimalControlField();
            val10.ByDecimalControlFieldName = "FRETURNBILLAMOUNT";
            val10.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val10);
            DecimalControlField val11 = new DecimalControlField();
            val11.ByDecimalControlFieldName = "FBALANCE";
            val11.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val11);
            DecimalControlField val12 = new DecimalControlField();
            val12.ByDecimalControlFieldName = "FPLEDGEAMT";
            val12.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val12);
            DecimalControlField val13 = new DecimalControlField();
            val13.ByDecimalControlFieldName = "FISTRUSTAMT";
            val13.DecimalControlFieldName = "FAMOUNTDIGITS";
            list.Add(val13);
            ((AbstractSysReportServicePlugIn)this).ReportProperty.DecimalControlFieldList = list;
        }

        private ReceivableBillTransactCondition GetReportFilter(IRptParams filter)
        {
            ReceivableBillTransactCondition receivableBillTransactCondition = new ReceivableBillTransactCondition();
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            if (customFilter == null)
            {
                return receivableBillTransactCondition;
            }
            receivableBillTransactCondition.RecOrgList = ReportCommonFunction.GetOrgIds(customFilter, "RecOrgLst", "RecOrgLst_ID");
            receivableBillTransactCondition.ContactUnitType = customFilter["CONTACTUNITTYPE"].ToString();
            object obj = customFilter["CONTACTUNITFrom"];
            DynamicObject val = (DynamicObject)((obj is DynamicObject) ? obj : null);
            object obj2 = customFilter["CONTACTUNITTo"];
            DynamicObject val2 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
            receivableBillTransactCondition.StartContactObj.Number = ((val == null) ? " " : val["Number"].ToString());
            receivableBillTransactCondition.EndContactObj.Number = ((val2 == null) ? " " : val2["Number"].ToString());
            object obj3 = customFilter["CurrencyFrom"];
            DynamicObject val3 = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
            if (val3 != null)
            {
                receivableBillTransactCondition.Currency.Number = val3["Number"].ToString();
                receivableBillTransactCondition.Currency.Id = Convert.ToInt64(val3["Id"]);
            }
            receivableBillTransactCondition.BillNoFrom = customFilter["BILLNOFROM"] as string;
            receivableBillTransactCondition.BillNoTo = customFilter["BILLNOTO"] as string;
            receivableBillTransactCondition.BizDateFrom = Convert.ToDateTime(customFilter["DATE"]);
            receivableBillTransactCondition.BizDateTo = Convert.ToDateTime(customFilter["ENDDATE"]);
            receivableBillTransactCondition.SettleState = customFilter["SETTLESTATE"] as string;
            receivableBillTransactCondition.UseFDATE = (bool)customFilter["UseFDATE"];
            receivableBillTransactCondition.AdvanceFilterString = filter.FilterParameter.FilterString;
            receivableBillTransactCondition.OrderbyFields = (string.IsNullOrWhiteSpace(filter.FilterParameter.SortString) ? "FBILLNUMBER" : filter.FilterParameter.SortString);
            receivableBillTransactCondition.IsShowPledge = (bool)customFilter["FSHOWPLEDGE"];
            receivableBillTransactCondition.IsShowTrust = (bool)customFilter["FSHOWISTRUST"];
            return receivableBillTransactCondition;
        }

        private void RemoveNullAmountData(string tableName)
        {
            string text = $" DELETE FROM {tableName} WHERE FINITIALBALANCE=0 AND FAMOUNTRECEIVED=0 AND FENDORSEAMOUNT=0 AND FENDORSERETAMOUNT=0 AND FDUEPAYMENT=0 AND FDISCOUNT=0 AND FRETURNBILLAMOUNT=0 AND FPLEDGEAMT=0 AND FISTRUSTAMT=0 ";
            DBUtils.Execute(((AbstractSysReportServicePlugIn)this).Context, text);
        }
    }
}
