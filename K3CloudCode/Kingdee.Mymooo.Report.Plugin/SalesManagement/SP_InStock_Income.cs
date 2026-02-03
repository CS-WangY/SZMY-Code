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
    [Description("生产入库单利润表服务插件"), HotUpdate]
    public class SP_InStock_Income : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称
            this.ReportProperty.ReportName = new LocaleValue("生产入库单利润表", base.Context.UserLocale.LCID);
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
            string seqFld = string.Format(base.KSQL_SEQ, " t1.FENTRYID ");
            // 取数SQL
            string sql = string.Format(@"/*dialect*/
SELECT {0},t1.FID,t1.FENTRYID,bdm.FMASTERID
,t2.FBILLNO,t2.FDATE,t2.FDOCUMENTSTATUS,t2.FSTOCKORGID,t2.FPRDORGID
,t1.FMATERIALID,bdm.FNUMBER,t1.FPRODUCTTYPE,t1.FUNITID,t1.FMUSTQTY,t1.FREALQTY,t1.FSTOCKID
,t1.FWORKSHOPID,t1.FMOBILLNO,t1.FMOENTRYSEQ,t1.FPARENTSMALLID,t1.FSMALLID,t1.FBUSINESSDIVISIONID
,ul.FNAME AS FUNITIDNAME,pmgl.FNAME AS FPARENTSMALLIDNAME,mgl.FNAME AS FSMALLIDNAME,bl.FDATAVALUE AS FBUSINESSDIVISIONNAME
,sp.FTAXPRICE,t1.FPRICE,t1.FAMOUNT
into {1}
FROM dbo.T_PRD_INSTOCKENTRY t1
INNER JOIN T_PRD_INSTOCK t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
LEFT JOIN dbo.T_BD_UNIT_L ul ON t1.FUNITID=ul.FUNITID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L pmgl ON t1.FPARENTSMALLID=pmgl.FID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L mgl ON t1.FSMALLID=mgl.FID
LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L bl ON t1.FBUSINESSDIVISIONID=bl.FENTRYID
LEFT JOIN (
SELECT _d.FENTRYID,_d.FMASTERID,_d.FTAXPRICE FROM 
(
SELECT ROW_NUMBER() OVER(PARTITION BY bdm.FMASTERID ORDER BY t1.FENTRYID DESC) AS KeyId,t2.FENTRYID,bdm.FMASTERID,t2.FTAXPRICE from dbo.T_SAL_ORDERENTRY t1
LEFT JOIN dbo.T_SAL_ORDERENTRY_F t2 ON t1.FENTRYID=t2.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
) _d WHERE KeyId=1
) sp ON bdm.FMASTERID=sp.FMASTERID
{2}",
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

        //public override ReportHeader GetReportHeaders(IRptParams filter)
        //{
        //    //FID, FEntryId, 编号、状态、物料、数量、单位、单位精度、单价、价税合计
        //    ReportHeader header = new ReportHeader();
        //    header.AddChild("FBILLNO", new LocaleValue("单据编号"));
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
            result.Add(new SummaryField("FMUSTQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FREALQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FTAXPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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
            string result = $" WHERE t2.FSTOCKORGID IN ({filter.FilterParameter.CustomFilter["FSaleOrgList"]})";
            var DateFrom = filter.FilterParameter.CustomFilter["DateFrom"];
            var DateTo = filter.FilterParameter.CustomFilter["DateTo"];
            result += $" and t2.FDATE BETWEEN '{DateFrom}' AND '{DateTo}'";
            //var CustomerFrom = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerFrom"])?["Number"];
            //var CustomerTo = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerTo"])?["Number"];
            //if (!(CustomerFrom is null))
            //{
            //    result += $" and bdc.FNUMBER BETWEEN '{CustomerFrom}' AND '{CustomerTo}'";
            //}
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
    [Description("生产入库单利润表表单插件"), HotUpdate]
    public class SP_InStock_IncomePlugIn : AbstractSysReportPlugIn
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
            //if (Convert.ToString(args.DataRow["FBILLTYPENAME"]) == "标准销售退货单")
            //{
            //    fc.ForeColor = "#FF0000";
            //}
            args.FormatConditions.Add(fc);
        }
        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);
//            var fldkey = args.Header.Key;
//            if (fldkey == "FTAXPRICE")
//            {
//                var materialid = args.DataRow["FMASTERID"];
//                string sSql = $@"SELECT TOP 1 t2.FTAXPRICE FROM dbo.T_SAL_ORDERENTRY t1
//LEFT JOIN dbo.T_SAL_ORDERENTRY_F t2 ON t1.FENTRYID=t2.FENTRYID
//LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
//WHERE bdm.FMASTERID={materialid}
//ORDER BY t2.FENTRYID DESC";
//                var salprice = DBUtils.ExecuteScalar<decimal>(Context, sSql, 0);
//                args.FormateValue = string.Format("{0}", salprice);
//            }
        }
    }
}
