using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.App.Core;
using Kingdee.K3.FIN.App.Core.Calculator.Object;
using Kingdee.K3.FIN.CN.App.Report;
using Kingdee.K3.FIN.Core;
using Kingdee.K3.FIN.Core.FilterCondition;
using static Kingdee.BOS.Core.Enums.BOSEnums;

namespace Kingdee.Mymooo.Report.Plugin.ReceivableManagement
{
    [Description("对账单开票查询报表服务插件"), HotUpdate]
    public class SAL_OUTSTOCK_Income : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称
            this.ReportProperty.ReportName = new LocaleValue("对账单开票查询报表", base.Context.UserLocale.LCID);
            //
            this.IsCreateTempTableByPlugin = true;
            //
            this.ReportProperty.IsUIDesignerColumns = false;
            //
            this.ReportProperty.IsGroupSummary = true;
            //
            this.ReportProperty.SimpleAllCols = false;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示
            this.ReportProperty.PrimaryKeyFieldName = "FID";
            //
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;

            // 报表主键字段名：默认为FIDENTITYID，可以修改
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            //
            // 设置精度控制
            List<DecimalControlField> list = new List<DecimalControlField>();
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FPRICEQTY",
                DecimalControlFieldName = "FUnitPrecision"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FTAXPRICE",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FPRICE",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FENTRYTAXRATE",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FENTRYDISCOUNTRATE",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FNOTAXAMOUNTFOR",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FTAXAMOUNTFOR",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FALLAMOUNTFOR_D",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FOPENAMOUNTFOR",
                DecimalControlFieldName = "FPRICEDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FRECEIVEAMOUNT",
                DecimalControlFieldName = "FAMOUNTDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FNORECEIVEAMOUNT",
                DecimalControlFieldName = "FAMOUNTDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FNOINVOICEAMOUNT",
                DecimalControlFieldName = "FAMOUNTDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "FNOINVOICEQTY",
                DecimalControlFieldName = "FAMOUNTDIGITS"
            });
            list.Add(new DecimalControlField
            {
                ByDecimalControlFieldName = "F_PENY_DIFFALLAMOUNT",
                DecimalControlFieldName = "FAMOUNTDIGITS"
            });
            this.ReportProperty.DecimalControlFieldList = list;
            //基础资料字段处理
            //this.ReportProperty.DspInsteadColumnsInfo.DefaultDspInsteadColumns.Add("FORGID", "FNAME");//销售组织
        }
        public override string GetTableName()
        {
            var result = base.GetTableName();
            return result;
        }
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            // 拼接过滤条件 ： filter
            var whereFilter = BuilderFromWhereSQL(filter);
            // 默认排序字段：需要从filter中取用户设置的排序字段
            string seqFld = string.Format(base.KSQL_SEQ, " t2.FDATE ");
            // 取数SQL
            string sql = string.Format(@"/*dialect*/
SELECT {0},t1.FID,tbl.FNAME AS FBILLTYPEID,t2.FBILLNO,t2.FDATE,t2.FCUSTOMERID,
dbcur.FNAME AS FCurrency,t2.FALLAMOUNTFOR,setorg.FNAME AS FSETTLEORG,salorg.FNAME AS FSALEORG,t2.FDOCUMENTSTATUS,t2.FENDDATE
,t1.FMATERIALID,t1.FPRICEUNITID,t1.FPRICEQTY,t1.FTAXPRICE,t1.FPRICE,t1.FENTRYTAXRATE,t1.FENTRYDISCOUNTRATE,t1.FNOTAXAMOUNTFOR,t1.FTAXAMOUNTFOR
,t1.FALLAMOUNTFOR AS FALLAMOUNTFOR_D,t1.FORDERNUMBER,t1.FOPENAMOUNTFOR,t1.FCOMMENT,
CASE to1.FISFREE WHEN 0 THEN '否' ELSE '是' END AS FISFREE
,t1.FRECEIVEAMOUNT,t1.FNORECEIVEAMOUNT,t1.FNOINVOICEAMOUNT,t1.FNOINVOICEQTY,t1.F_PENY_DIFFALLAMOUNT
,psg.FNAME AS FPGNAME,sg.FNAME AS FGNAME
,(SELECT 
STUFF((SELECT ',' + p1.FSRCBILLNO
FROM T_AR_BillingMatchLogENTRY p1
WHERE p1.FID=bme.FID AND p1.FSOURCEFROMID='IV_SALESIC'
FOR XML PATH('')), 1, 1, '')) AS FIVCODE
into {1}
FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLEENTRY_O to1 ON t1.FENTRYID=to1.FENTRYID
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_BD_CUSTOMER bdc ON t2.FCUSTOMERID=bdc.FCUSTID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
LEFT JOIN dbo.T_BD_CURRENCY_L dbcur ON t2.FCURRENCYID=dbcur.FCURRENCYID
LEFT JOIN T_BAS_BILLTYPE_L tbl ON t2.FBILLTYPEID=tbl.FBILLTYPEID AND tbl.FLOCALEID=2052
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L setorg ON t2.FSETTLEORGID=setorg.FORGID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L salorg ON t2.FSALEORGID=salorg.FORGID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L psg ON t1.FPARENTSMALLID=psg.FID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L sg ON t1.FSMALLID=sg.FID
LEFT JOIN T_AR_BillingMatchLogENTRY bme ON t1.FENTRYID=bme.FSRCROWID
WHERE 1=1 {2}",
seqFld,
            tableName, whereFilter);
            DBUtils.Execute(this.Context, sql);
        }

        protected override string GetIdentityFieldIndexSQL(string tableName)
        {
            string result = base.GetIdentityFieldIndexSQL(tableName);
            return result;
        }
        protected override void ExecuteBatch(List<string> listSql)
        {
            base.ExecuteBatch(listSql);
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            var FBILLTYPEID = header.AddChild("FBILLTYPEID", new LocaleValue("单据类型"));
            FBILLTYPEID.ColIndex = 0;
            FBILLTYPEID.Mergeable = true;
            var FBILLNO = header.AddChild("FBILLNO", new LocaleValue("单据编号"));
            FBILLTYPEID.ColIndex = 1;
            FBILLNO.Mergeable = true;
            var FDATE = header.AddChild("FDATE", new LocaleValue("业务日期"));
            FBILLTYPEID.ColIndex = 2;
            FDATE.Mergeable = true;
            var FCUSTOMERNumber = header.AddChild("FCUSTOMERNumber", new LocaleValue("客户编码"));
            FBILLTYPEID.ColIndex = 3;
            FCUSTOMERNumber.Mergeable = true;
            var FCUSTOMERName = header.AddChild("FCUSTOMERName", new LocaleValue("客户名称"));
            FBILLTYPEID.ColIndex = 4;
            FCUSTOMERName.Mergeable = true;
            var FCurrency = header.AddChild("FCurrency", new LocaleValue("币别"));
            FBILLTYPEID.ColIndex = 5;
            FCurrency.Mergeable = true;
            var FALLAMOUNTFOR = header.AddChild("FALLAMOUNTFOR", new LocaleValue("价税合计"), SqlStorageType.SqlDecimal);
            FALLAMOUNTFOR.Mergeable = true;
            var FSETTLEORG = header.AddChild("FSETTLEORG", new LocaleValue("结算组织"));
            FSETTLEORG.Mergeable = true;
            var FSALEORG = header.AddChild("FSALEORG", new LocaleValue("销售组织"));
            FSALEORG.Mergeable = true;
            var FDOCUMENTSTATUS = header.AddChild("FDOCUMENTSTATUS", new LocaleValue("单据状态"));
            FDOCUMENTSTATUS.Mergeable = true;
            var FENDDATE = header.AddChild("FENDDATE", new LocaleValue("到期日"));
            FENDDATE.Mergeable = true;
            var FMATERIALIDNUMBER = header.AddChild("FMATERIALIDNUMBER", new LocaleValue("物料编码"));
            var FMATERIALIDNAME = header.AddChild("FMATERIALIDNAME", new LocaleValue("物料名称"));
            var FPRICEUNITNAME = header.AddChild("FPRICEUNITNAME", new LocaleValue("计价单位"));
            var FPRICEQTY = header.AddChild("FPRICEQTY", new LocaleValue("计价数量"), SqlStorageType.SqlDecimal);
            var FTAXPRICE = header.AddChild("FTAXPRICE", new LocaleValue("含税单价"), SqlStorageType.SqlDecimal);
            var FPRICE = header.AddChild("FPRICE", new LocaleValue("单价"), SqlStorageType.SqlDecimal);
            var FENTRYTAXRATE = header.AddChild("FENTRYTAXRATE", new LocaleValue("税率(%)"), SqlStorageType.SqlDecimal);
            var FENTRYDISCOUNTRATE = header.AddChild("FENTRYDISCOUNTRATE", new LocaleValue("折扣率(%)"), SqlStorageType.SqlDecimal);
            var FNOTAXAMOUNTFOR = header.AddChild("FNOTAXAMOUNTFOR", new LocaleValue("不含税金额"), SqlStorageType.SqlDecimal);
            var FTAXAMOUNTFOR = header.AddChild("FTAXAMOUNTFOR", new LocaleValue("税额"), SqlStorageType.SqlDecimal);
            var FALLAMOUNTFOR_D = header.AddChild("FALLAMOUNTFOR_D", new LocaleValue("价税合计"), SqlStorageType.SqlDecimal);
            var FORDERNUMBER = header.AddChild("FORDERNUMBER", new LocaleValue("销售订单号"));
            var FOPENAMOUNTFOR = header.AddChild("FOPENAMOUNTFOR", new LocaleValue("已开票核销金额"), SqlStorageType.SqlDecimal);
            var FCOMMENT = header.AddChild("FCOMMENT", new LocaleValue("备注"));
            var FISFREE = header.AddChild("FISFREE", new LocaleValue("是否赠品"));
            var FRECEIVEAMOUNT = header.AddChild("FRECEIVEAMOUNT", new LocaleValue("已结算金额"), SqlStorageType.SqlDecimal);
            var FNORECEIVEAMOUNT = header.AddChild("FNORECEIVEAMOUNT", new LocaleValue("未结算金额"), SqlStorageType.SqlDecimal);
            var FNOINVOICEAMOUNT = header.AddChild("FNOINVOICEAMOUNT", new LocaleValue("未开票核销金额"), SqlStorageType.SqlDecimal);
            var FNOINVOICEQTY = header.AddChild("FNOINVOICEQTY", new LocaleValue("未开票核销数量"), SqlStorageType.SqlDecimal);
            var F_PENY_DIFFALLAMOUNT = header.AddChild("F_PENY_DIFFALLAMOUNT", new LocaleValue("价税差额"), SqlStorageType.SqlDecimal);
            var FPGNAME = header.AddChild("FPGNAME", new LocaleValue("大类"));
            var FGNAME = header.AddChild("FGNAME", new LocaleValue("小类"));
            var FIVCODE = header.AddChild("FIVCODE", new LocaleValue("发票号"));
            FIVCODE.Width = 400;
            return header;
        }
        //public override ReportTitles GetReportTitles(IRptParams filter)
        //{
        //    var result = base.GetReportTitles(filter);
        //    DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
        //    if (dyFilter != null)
        //    {
        //        if (result == null)
        //        {
        //            result = new ReportTitles();
        //        }
        //        result.AddTitle("F_JD_Date", Convert.ToString(dyFilter["F_JD_Date"]));
        //    }
        //    return result;
        //}
        protected override string AnalyzeDspCloumn(IRptParams filter, string tablename)
        {
            string result = base.AnalyzeDspCloumn(filter, tablename);
            return result;
        }
        protected override void AfterCreateTempTable(string tablename)
        {
            base.AfterCreateTempTable(tablename);
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FPRICEUNITID", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FPRICEQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FENTRYTAXRATE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FENTRYDISCOUNTRATE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FNOTAXAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FALLAMOUNTFOR_D", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FOPENAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FRECEIVEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FNORECEIVEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FNOINVOICEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FNOINVOICEQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("F_PENY_DIFFALLAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            return result;
        }
        protected override string GetSummaryColumsSQL(List<SummaryField> summaryFields)
        {
            var result = base.GetSummaryColumsSQL(summaryFields);
            return result;
        }
        protected override System.Data.DataTable GetListData(string sSQL)
        {
            var result = base.GetListData(sSQL);
            return result;
        }
        protected override System.Data.DataTable GetReportData(IRptParams filter)
        {
            var result = base.GetReportData(filter);
            return result;
        }
        protected override System.Data.DataTable GetReportData(string tablename, IRptParams filter)
        {
            var result = base.GetReportData(tablename, filter);
            return result;
        }
        public override int GetRowsCount(IRptParams filter)
        {
            var result = base.GetRowsCount(filter);
            return result;
        }
        protected override string BuilderFromWhereSQL(IRptParams filter)
        {
            string result = $" and t2.FSETTLEORGID IN ({filter.FilterParameter.CustomFilter["FSaleOrgList"]})";
            var DateFrom = filter.FilterParameter.CustomFilter["DateFrom"];
            var DateTo = filter.FilterParameter.CustomFilter["DateTo"];
            result += $" and t2.FDATE BETWEEN '{DateFrom}' AND '{DateTo}'";
            var CustomerFrom = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerFrom"])?["Number"];
            var CustomerTo = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerTo"])?["Number"];
            if (!(CustomerFrom is null))
            {
                result += $" and bdc.FNUMBER BETWEEN '{CustomerFrom}' AND '{CustomerTo}'";
            }
            var MaterialFrom = ((DynamicObject)filter.FilterParameter.CustomFilter["FMaterialFrom"])?["Number"];
            var MaterialTo = ((DynamicObject)filter.FilterParameter.CustomFilter["FMaterialTo"])?["Number"];
            if (!(MaterialFrom is null))
            {
                result += $" and bdm.FNUMBER BETWEEN '{MaterialFrom}' AND '{MaterialTo}'";
            }

            return result;
        }
        protected override string BuilderSelectFieldSQL(IRptParams filter)
        {
            string result = base.BuilderSelectFieldSQL(filter);
            return result;
        }
        protected override string BuilderTempTableOrderBySQL(IRptParams filter)
        {
            string result = base.BuilderTempTableOrderBySQL(filter);
            return result;
        }
        public override void CloseReport()
        {
            base.CloseReport();
        }
        protected override string CreateGroupSummaryData(IRptParams filter, string tablename)
        {
            string result = base.CreateGroupSummaryData(filter, tablename);
            return result;
        }
        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }
        public override void DropTempTable()
        {
            base.DropTempTable();
        }
        public override System.Data.DataTable GetList(IRptParams filter)
        {
            var result = base.GetList(filter);
            return result;
        }
        public override List<long> GetOrgIdList(IRptParams filter)
        {
            var result = base.GetOrgIdList(filter);
            return result;
        }
        public override List<TreeNode> GetTreeNodes(IRptParams filter)
        {
            var result = base.GetTreeNodes(filter);
            return result;
        }
    }
}
