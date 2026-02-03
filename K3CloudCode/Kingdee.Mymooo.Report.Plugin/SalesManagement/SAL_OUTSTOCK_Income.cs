using System;
using System.Collections.Generic;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Report.PlugIn.Args;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售出库成本表服务插件"), HotUpdate]
    public class SAL_OUTSTOCK_Income : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称
            this.ReportProperty.ReportName = new LocaleValue("销售出库实际利润表(集团)", base.Context.UserLocale.LCID);
            //
            this.IsCreateTempTableByPlugin = true;
            //
            this.ReportProperty.IsUIDesignerColumns = false;
            //
            this.ReportProperty.IsGroupSummary = true;
            //
            this.ReportProperty.SimpleAllCols = false;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示
            //this.ReportProperty.PrimaryKeyFieldName = "FID";
            //
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;

            // 报表主键字段名：默认为FIDENTITYID，可以修改
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            //
            //// 设置精度控制
            //List<DecimalControlField> list = new List<DecimalControlField>();
            //// 数量
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FQty",
            //    DecimalControlFieldName = "FUnitPrecision"
            //});
            //// 单价
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FTAXPRICE",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //// 金额
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FALLAMOUNT",
            //    DecimalControlFieldName = "FAMOUNTDIGITS"
            //});
            //this.ReportProperty.DecimalControlFieldList = list;
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
            string seqFld = string.Format(base.KSQL_SEQ, " _t0.FDATE ");
            // 取数SQL
            string sql = string.Format(@"/*dialect*/
SELECT {0}
,_t0.*
,tbl.FNAME AS FBILLTYPENAME
,parg.FNAME AS FPARENTSMALLNAME
,sg.FNAME AS FSMALLNAME
,'外购' AS FMATERIALTYPE
into {1}
FROM 
(
SELECT tb.FID,t1.FENTRYID,tb.FSALEORGID,tb.FBILLTYPEID,tb.FDATE,tb.FBILLNO,tb.FCUSTOMERID,tb.FSTOCKORGID,tb.FDOCUMENTSTATUS
,tb.FLINKMAN,tb.FLINKPHONE,tb.FOWNERID
,t1.FMATERIALID,t1.FUNITID
,t1.FREALQTY,t1.FSTOCKID
,tf.FPRICE,tf.FTAXPRICE,tf.FAMOUNT,tf.FDISCOUNTRATE,tf.FDISCOUNT,tf.FISFREE,tf.FTAXAMOUNT,tf.FALLAMOUNT,tf.FTAXNETPRICE,tc.FPRICE FCOSTPRICE,tc.FAMOUNT_LC
,tr.FSUMRETSTOCKQTY,tb.FPACKAGINGCOMPLETEDATE,tb.FPICKINGCOMPLETEDATE,t1.FTRACKINGNUMBER,t1.FTRACKINGNAME,t1.FTRACKINGDATE,t1.FTRACKINGUSER,t1.FPACKAGINGUSER,t1.FPACKAGINGDATE
,t1.FCUSTMATERIALNO,t1.FCUSTPURCHASENO,t1.FPARENTSMALLID,t1.FSMALLID,tr.FSOORDERNO,t1.FSUPPLYTARGETORGID
FROM dbo.T_SAL_OUTSTOCKENTRY t1
LEFT JOIN dbo.T_SAL_OUTSTOCKENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
LEFT JOIN dbo.T_SAL_OUTSTOCKENTRY_R tr ON t1.FENTRYID=tr.FENTRYID
LEFT JOIN T_SAL_OUTSTOCKENTRY_C tc ON t1.FENTRYID=tc.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK tb ON t1.FID=tb.FID
--UNION ALL
--SELECT tb.FID,t1.FENTRYID,tb.FSALEORGID,tb.FBILLTYPEID,tb.FDATE,tb.FBILLNO,tb.FRETCUSTID,tb.FSTOCKORGID,tb.FDOCUMENTSTATUS
--,tb.FLINKMAN,tb.FLINKPHONE,tb.FOWNERID
--,t1.FMATERIALID,t1.FUNITID
--,t1.FREALQTY,t1.FSTOCKID
--,tf.FPRICE,tf.FTAXPRICE,tf.FAMOUNT,tf.FDISCOUNTRATE,tf.FDISCOUNT,tf.FISFREE,tf.FTAXAMOUNT,tf.FALLAMOUNT,tf.FTAXNETPRICE,tc.FPRICE,tc.FAMOUNT_LC
--,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
--,t1.FCUSTMATERIALNO,t1.FCUSTPURCHASENO,t1.FPARENTSMALLID,t1.FSMALLID,t1.FORDERNO,t1.FSUPPLYTARGETORGID
--FROM T_SAL_RETURNSTOCKENTRY t1
--LEFT JOIN T_SAL_RETURNSTOCKENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
--LEFT JOIN dbo.T_SAL_RETURNSTOCKENTRY_R tr ON tr.FENTRYID = t1.FENTRYID
--LEFT JOIN dbo.T_SAL_RETURNSTOCKENTRY_C tc ON tc.FENTRYID = t1.FENTRYID
--LEFT JOIN dbo.T_SAL_RETURNSTOCK tb ON tb.FID = t1.FID
) _t0 
LEFT JOIN T_BAS_BILLTYPE_L tbl ON _t0.FBILLTYPEID=tbl.FBILLTYPEID AND tbl.FLOCALEID=2052
LEFT JOIN T_BD_MATERIALGROUP_L parg ON _t0.FPARENTSMALLID=parg.FID
LEFT JOIN T_BD_MATERIALGROUP_L sg ON _t0.FSMALLID=sg.FID
LEFT JOIN dbo.T_BD_CUSTOMER bdc ON _t0.FCUSTOMERID=bdc.FCUSTID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON _t0.FMATERIALID=bdm.FMATERIALID
{2}
ORDER BY _t0.FDATE,_t0.FBILLNO,_t0.FENTRYID",
seqFld,
            tableName, whereFilter);
            DBUtils.Execute(this.Context, sql);
            //            sql = string.Format(@"UPDATE {0} SET 
            //FSALEORGID=0
            //,FBILLTYPEID=''
            //,FDATE=''
            //,FBILLNO=''
            //,FCUSTOMERID=0
            //,FSTOCKORGID=0
            //,FDOCUMENTSTATUS=''
            //,FLINKMAN=''
            //,FLINKPHONE=''
            //,FOWNERID=0
            //,FPACKAGINGCOMPLETEDATE=''
            //,FPICKINGCOMPLETEDATE=''
            //WHERE FENTRYID NOT IN (SELECT MIN(FENTRYID) FROM {0} GROUP BY FBILLNO)", tableName);
            //            DBUtils.Execute(this.Context, sql);
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

        //public override ReportHeader GetReportHeaders(IRptParams filter)
        //{
        //    //FID, FEntryId, 编号、状态、物料、数量、单位、单位精度、单价、价税合计
        //    ReportHeader header = new ReportHeader();
        //    var FSALEORGID = header.AddChild("FSALEORGID", new LocaleValue("单据编号"));
        //    FSALEORGID.Mergeable = true;
        //    var FBILLTYPEID = header.AddChild("FBILLTYPEID", new LocaleValue("单据编号"));
        //    FBILLTYPEID.Mergeable = true;
        //    var FDATE = header.AddChild("FDATE", new LocaleValue("单据编号"));
        //    FDATE.Mergeable = true;
        //    var FBILLNO = header.AddChild("FBILLNO", new LocaleValue("单据编号"));
        //    FBILLNO.Mergeable = true;
        //    var FCUSTOMERID = header.AddChild("FCUSTOMERID", new LocaleValue("单据编号"));
        //    FCUSTOMERID.Mergeable = true;
        //    var FSTOCKORGID = header.AddChild("FSTOCKORGID", new LocaleValue("单据编号"));
        //    FSTOCKORGID.Mergeable = true;
        //    var FDOCUMENTSTATUS = header.AddChild("FDOCUMENTSTATUS", new LocaleValue("单据编号"));
        //    FDOCUMENTSTATUS.Mergeable = true;
        //    var FLINKMAN = header.AddChild("FLINKMAN", new LocaleValue("单据编号"));
        //    FLINKMAN.Mergeable = true;
        //    var FLINKPHONE = header.AddChild("FLINKPHONE", new LocaleValue("单据编号"));
        //    FLINKPHONE.Mergeable = true;
        //    var FOWNERID = header.AddChild("FOWNERID", new LocaleValue("单据编号"));
        //    FOWNERID.Mergeable = true;


        //    var material = header.AddChild("FMATERIALID", new LocaleValue("物料编码"));
        //    material.Mergeable = false;
        //    var FUNITID = header.AddChild("FUNITID", new LocaleValue("物料编码"));
        //    FUNITID.Mergeable = false;
        //    var FREALQTY = header.AddChild("FREALQTY", new LocaleValue("物料编码"));
        //    FREALQTY.Mergeable = false;
        //    var FSTOCKID = header.AddChild("FSTOCKID", new LocaleValue("物料编码"));
        //    FSTOCKID.Mergeable = false;
        //    var FPRICE = header.AddChild("FPRICE", new LocaleValue("物料编码"));
        //    FPRICE.Mergeable = false;
        //    var FTAXPRICE = header.AddChild("FTAXPRICE", new LocaleValue("物料编码"));
        //    FTAXPRICE.Mergeable = false;
        //    var FAMOUNT = header.AddChild("FAMOUNT", new LocaleValue("物料编码"));
        //    FAMOUNT.Mergeable = false;
        //    var FDISCOUNTRATE = header.AddChild("FDISCOUNTRATE", new LocaleValue("物料编码"));
        //    FDISCOUNTRATE.Mergeable = false;
        //    var FDISCOUNT = header.AddChild("FDISCOUNT", new LocaleValue("物料编码"));
        //    FDISCOUNT.Mergeable = false;
        //    var FISFREE = header.AddChild("FISFREE", new LocaleValue("物料编码"));
        //    FISFREE.Mergeable = false;
        //    var FTAXAMOUNT = header.AddChild("FTAXAMOUNT", new LocaleValue("物料编码"));
        //    FTAXAMOUNT.Mergeable = false;
        //    var FALLAMOUNT = header.AddChild("FALLAMOUNT", new LocaleValue("物料编码"));
        //    FALLAMOUNT.Mergeable = false;
        //    var FTAXNETPRICE = header.AddChild("FTAXNETPRICE", new LocaleValue("物料编码"));
        //    FTAXNETPRICE.Mergeable = false;
        //    var FCOSTPRICE = header.AddChild("FCOSTPRICE", new LocaleValue("物料编码"));
        //    FCOSTPRICE.Mergeable = false;
        //    var FAMOUNT_LC = header.AddChild("FAMOUNT_LC", new LocaleValue("物料编码"));
        //    FAMOUNT_LC.Mergeable = false;
        //    var FSUMRETSTOCKQTY = header.AddChild("FSUMRETSTOCKQTY", new LocaleValue("物料编码"));
        //    FSUMRETSTOCKQTY.Mergeable = false;
        //    var FPACKAGINGCOMPLETEDATE = header.AddChild("FPACKAGINGCOMPLETEDATE", new LocaleValue("物料编码"));
        //    FPACKAGINGCOMPLETEDATE.Mergeable = false;
        //    var FPICKINGCOMPLETEDATE = header.AddChild("FPICKINGCOMPLETEDATE", new LocaleValue("物料编码"));
        //    FPICKINGCOMPLETEDATE.Mergeable = false;
        //    var FTRACKINGNUMBER = header.AddChild("FTRACKINGNUMBER", new LocaleValue("物料编码"));
        //    FTRACKINGNUMBER.Mergeable = false;
        //    var FTRACKINGNAME = header.AddChild("FTRACKINGNAME", new LocaleValue("物料编码"));
        //    FTRACKINGNAME.Mergeable = false;
        //    var FTRACKINGDATE = header.AddChild("FTRACKINGDATE", new LocaleValue("物料编码"));
        //    FTRACKINGDATE.Mergeable = false;
        //    var FTRACKINGUSER = header.AddChild("FTRACKINGUSER", new LocaleValue("物料编码"));
        //    FTRACKINGUSER.Mergeable = false;
        //    var FPACKAGINGUSER = header.AddChild("FPACKAGINGUSER", new LocaleValue("物料编码"));
        //    FPACKAGINGUSER.Mergeable = false;
        //    var FPACKAGINGDATE = header.AddChild("FPACKAGINGDATE", new LocaleValue("物料编码"));
        //    FPACKAGINGDATE.Mergeable = false;
        //    var FCUSTMATERIALNO = header.AddChild("FCUSTMATERIALNO", new LocaleValue("物料编码"));
        //    FCUSTMATERIALNO.Mergeable = false;
        //    var FCUSTPURCHASENO = header.AddChild("FCUSTPURCHASENO", new LocaleValue("物料编码"));
        //    FCUSTPURCHASENO.Mergeable = false;
        //    var FPARENTSMALLID = header.AddChild("FPARENTSMALLID", new LocaleValue("物料编码"));
        //    FPARENTSMALLID.Mergeable = false;
        //    var FSMALLID = header.AddChild("FSMALLID", new LocaleValue("物料编码"));
        //    FSMALLID.Mergeable = false;
        //    var FSOORDERNO = header.AddChild("FSOORDERNO", new LocaleValue("物料编码"));
        //    FSOORDERNO.Mergeable = false;
        //    var FSUPPLYTARGETORGID = header.AddChild("FSUPPLYTARGETORGID", new LocaleValue("物料编码"));
        //    FSUPPLYTARGETORGID.Mergeable = false;
        //    return header;
        //}
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
            result.Add(new SummaryField("FREALQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FALLAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXNETPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FCOSTPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAMOUNT_LC", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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
            string result = $" WHERE _t0.FSALEORGID IN ({filter.FilterParameter.CustomFilter["FSaleOrgList"]})";
            var DateFrom = filter.FilterParameter.CustomFilter["DateFrom"];
            var DateTo = filter.FilterParameter.CustomFilter["DateTo"];
            result += $" and _t0.FDATE BETWEEN '{DateFrom}' AND '{DateTo}'";
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
    [Description("销售出库成本表表单插件"), HotUpdate]
    public class SAL_OUTSTOCK_IncomePlugIn : AbstractSysReportPlugIn
    {
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var sp = this.View.GetControl<SplitContainer>("FSpliteContainer");
            sp.HideFirstPanel(true);
        }
        public override void OnFormatRowConditions(ReportFormatConditionArgs args)
        {
            base.OnFormatRowConditions(args);
            var fc = new FormatCondition();
            if (Convert.ToInt64(args.DataRow["FIDENTITYID"]) % 2 == 0)
            {
                fc.BackColor = "#D1EEEE";
            }
            if (Convert.ToString(args.DataRow["FBILLTYPENAME"]) == "标准销售退货单")
            {
                fc.ForeColor = "#FF0000";
            }
            args.FormatConditions.Add(fc);
        }
        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);
            var fldkey = args.Header.Key;
            if (fldkey == "FMATERIALTYPE")
            {
                var materialid = args.DataRow["FMATERIALID"];
                string sSql = $"SELECT FMASTERID FROM T_BD_MATERIAL WHERE FMATERIALID={materialid}";
                var masterid = DBUtils.ExecuteScalar<long>(Context, sSql, 0);
                var fdate = Convert.ToDateTime(args.DataRow["FDATE"]);
                DateTime DateS = new DateTime(fdate.Year, fdate.Month, 1);
                DateTime DateE = new DateTime(fdate.AddMonths(1).Year, fdate.AddMonths(1).Month, 1);
                sSql = $@"SELECT t1.FENTRYID FROM dbo.T_PRD_MOENTRY t1 INNER JOIN dbo.T_PRD_MO t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
WHERE bdm.FMASTERID={masterid} AND t2.FDATE>='{DateS}' AND t2.FDATE<'{DateE}'";
                var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                if (datas.Count > 0)
                {
                    args.FormateValue = string.Format("{0}", "自制");
                }
            }
        }
    }
}
