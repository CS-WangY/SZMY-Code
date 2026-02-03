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
    [Description("应收成本大表服务插件"), HotUpdate]
    public class AuditCostReport : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称
            this.ReportProperty.ReportName = new LocaleValue("应收成本大表", base.Context.UserLocale.LCID);
            //
            this.IsCreateTempTableByPlugin = true;
            //
            this.ReportProperty.IsUIDesignerColumns = false;
            //
            this.ReportProperty.IsGroupSummary = true;
            //
            this.ReportProperty.SimpleAllCols = false;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示
            this.ReportProperty.PrimaryKeyFieldName = "FCUSTID";
            //
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;

            // 报表主键字段名：默认为FIDENTITYID，可以修改
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            //
            // 设置精度控制
            //List<DecimalControlField> list = new List<DecimalControlField>();
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FPRICEQTY",
            //    DecimalControlFieldName = "FUnitPrecision"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FTAXPRICE",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FPRICE",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FENTRYTAXRATE",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FENTRYDISCOUNTRATE",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FNOTAXAMOUNTFOR",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FTAXAMOUNTFOR",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FALLAMOUNTFOR_D",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FOPENAMOUNTFOR",
            //    DecimalControlFieldName = "FPRICEDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FRECEIVEAMOUNT",
            //    DecimalControlFieldName = "FAMOUNTDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FNORECEIVEAMOUNT",
            //    DecimalControlFieldName = "FAMOUNTDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FNOINVOICEAMOUNT",
            //    DecimalControlFieldName = "FAMOUNTDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "FNOINVOICEQTY",
            //    DecimalControlFieldName = "FAMOUNTDIGITS"
            //});
            //list.Add(new DecimalControlField
            //{
            //    ByDecimalControlFieldName = "F_PENY_DIFFALLAMOUNT",
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
            string seqFld = string.Format(base.KSQL_SEQ, " t1.FNUMBER ");
            // 取数SQL
            string sql = string.Format(@"/*dialect*/
SELECT {0},t1.FCUSTID,t1.FNUMBER,t2.FNAME AS FCustName,t3.FBILLNO AS FSalBillNo,t3.FDATE AS FSalDate,t3.FBILLNO AS FSalBillNo2
,t4.FSETTLEMODEID,t5.FNAME AS FSettleMode,t4.FRECCONDITIONID,t6.FNAME AS FReccondition,IIF(t7.FISFREE=1, '是', '否') AS FISFREE
,t9.FNUMBER AS FMaterialNumber,t8.FQTY AS FSalQty,t7.FTAXPRICE,t7.FPRICE,t7.FTAXRATE,t7.FALLAMOUNT,t7.FAMOUNT
,sto.FDATE AS FSTDate,sto.FBILLNO AS FSTBillNo,tom.FNUMBER AS FSTMaterialNumber,torf.FPRICEUNITQTY AS FSTPriceQty,torf.FTAXPRICE AS FSTTaxPrice,torf.FAMOUNT AS FSTAmount
,tor.FSRCBILLNO AS FDBillNo,tde.FTrackingName,tde.FTrackingNumber,tde.FDELIVERYADDRESS,td.FLINKMAN,td.FLINKPHONE
,tiv.FDATE AS FIVDate,tiv.FBILLNO AS FIVBillNo
,gv.FDATE AS FGVDate,gv.FVOUCHERGROUPNO AS FGVBillNo,gv.FDEBITTOTAL AS FGVAmount,gv.FCREDITTOTAL AS FGVTaxAmount
,tarecos.FCOSTAMT
into {1}
FROM dbo.T_BD_CUSTOMER t1
INNER JOIN T_BD_CUSTOMER_L t2 ON t1.FCUSTID=t2.FCUSTID
LEFT JOIN dbo.T_SAL_ORDER t3 ON t1.FCUSTID=t3.FCUSTID
LEFT JOIN T_SAL_ORDERENTRY t8 ON t8.FID=t3.FID
LEFT JOIN T_SAL_ORDERFIN t4 ON t3.FID=t4.FID
LEFT JOIN T_BD_SETTLETYPE_L t5 ON t4.FSETTLEMODEID=t5.FID
LEFT JOIN T_BD_RecCondition_L t6 ON t4.FRECCONDITIONID=t6.FID
LEFT JOIN T_SAL_ORDERENTRY_F t7 ON t8.FENTRYID=t7.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL t9 ON t8.FMATERIALID=t9.FMATERIALID
LEFT JOIN T_SAL_OUTSTOCKENTRY_R tor ON tor.FSOENTRYID=t8.FENTRYID
LEFT JOIN T_SAL_OUTSTOCKENTRY tore ON tor.FSOENTRYID=tore.FENTRYID
LEFT JOIN T_SAL_OUTSTOCKENTRY_F torf ON torf.FENTRYID=tore.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL tom ON tore.FMATERIALID=tom.FMATERIALID
LEFT JOIN T_SAL_OUTSTOCK sto ON sto.FID=tore.FID
LEFT JOIN T_SAL_OUTSTOCKENTRY_LK toel ON toel.FENTRYID=tore.FENTRYID
LEFT JOIN T_SAL_DELIVERYNOTICEENTRY tde ON toel.FSID=tde.FENTRYID
LEFT JOIN T_SAL_DELIVERYNOTICE td ON td.FID=tde.FID
LEFT JOIN t_AR_receivableEntry_LK tarel ON tarel.FSID=tor.FENTRYID
LEFT JOIN t_AR_receivableEntry tare ON tare.FENTRYID=tarel.FENTRYID
LEFT JOIN T_AR_SOCCOSTENTRY tarecos ON tarecos.FENTRYID=tare.FENTRYID
LEFT JOIN t_AR_receivable tar ON tar.FID=tare.FID
LEFT JOIN T_IV_SALESICENTRY_LK tivl ON tivl.FSID=tare.FENTRYID
LEFT JOIN T_IV_SALESICENTRY tive ON tive.FENTRYID=tivl.FENTRYID
LEFT JOIN T_IV_SALESIC tiv ON tive.FID=tiv.FID
LEFT JOIN t_AR_receivable_VH arvh ON tar.FID=arvh.FID
LEFT JOIN T_BAS_VOUCHER bv ON arvh.FBIZVOUCHERID=bv.FVOUCHERID
LEFT JOIN T_GL_VOUCHER gv ON arvh.FGLVOUCHERID=gv.FVOUCHERID
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

            //var FCustName = header.AddChild("FCustName", new LocaleValue("客户名称"));
            //FCustName.ColIndex = 0;
            //FCustName.Mergeable = true;
            //var FBILLNO = header.AddChild("FBILLNO", new LocaleValue("单据编号"));
            //FBILLTYPEID.ColIndex = 1;
            //FBILLNO.Mergeable = true;
            //var FDATE = header.AddChild("FDATE", new LocaleValue("业务日期"));
            //FBILLTYPEID.ColIndex = 2;
            //FDATE.Mergeable = true;
            //var FCUSTOMERNumber = header.AddChild("FCUSTOMERNumber", new LocaleValue("客户编码"));
            //FBILLTYPEID.ColIndex = 3;
            //FCUSTOMERNumber.Mergeable = true;
            //var FCUSTOMERName = header.AddChild("FCUSTOMERName", new LocaleValue("客户名称"));
            //FBILLTYPEID.ColIndex = 4;
            //FCUSTOMERName.Mergeable = true;
            //var FCurrency = header.AddChild("FCurrency", new LocaleValue("币别"));
            //FBILLTYPEID.ColIndex = 5;
            //FCurrency.Mergeable = true;
            //var FALLAMOUNTFOR = header.AddChild("FALLAMOUNTFOR", new LocaleValue("价税合计"), SqlStorageType.SqlDecimal);
            //FALLAMOUNTFOR.Mergeable = true;
            //var FSETTLEORG = header.AddChild("FSETTLEORG", new LocaleValue("结算组织"));
            //FSETTLEORG.Mergeable = true;
            //var FSALEORG = header.AddChild("FSALEORG", new LocaleValue("销售组织"));
            //FSALEORG.Mergeable = true;
            //var FDOCUMENTSTATUS = header.AddChild("FDOCUMENTSTATUS", new LocaleValue("单据状态"));
            //FDOCUMENTSTATUS.Mergeable = true;
            //var FENDDATE = header.AddChild("FENDDATE", new LocaleValue("到期日"));
            //FENDDATE.Mergeable = true;
            //var FMATERIALIDNUMBER = header.AddChild("FMATERIALIDNUMBER", new LocaleValue("物料编码"));
            //var FMATERIALIDNAME = header.AddChild("FMATERIALIDNAME", new LocaleValue("物料名称"));
            //var FPRICEUNITNAME = header.AddChild("FPRICEUNITNAME", new LocaleValue("计价单位"));
            //var FPRICEQTY = header.AddChild("FPRICEQTY", new LocaleValue("计价数量"), SqlStorageType.SqlDecimal);
            //var FTAXPRICE = header.AddChild("FTAXPRICE", new LocaleValue("含税单价"), SqlStorageType.SqlDecimal);
            //var FPRICE = header.AddChild("FPRICE", new LocaleValue("单价"), SqlStorageType.SqlDecimal);
            //var FENTRYTAXRATE = header.AddChild("FENTRYTAXRATE", new LocaleValue("税率(%)"), SqlStorageType.SqlDecimal);
            //var FENTRYDISCOUNTRATE = header.AddChild("FENTRYDISCOUNTRATE", new LocaleValue("折扣率(%)"), SqlStorageType.SqlDecimal);
            //var FNOTAXAMOUNTFOR = header.AddChild("FNOTAXAMOUNTFOR", new LocaleValue("不含税金额"), SqlStorageType.SqlDecimal);
            //var FTAXAMOUNTFOR = header.AddChild("FTAXAMOUNTFOR", new LocaleValue("税额"), SqlStorageType.SqlDecimal);
            //var FALLAMOUNTFOR_D = header.AddChild("FALLAMOUNTFOR_D", new LocaleValue("价税合计"), SqlStorageType.SqlDecimal);
            //var FORDERNUMBER = header.AddChild("FORDERNUMBER", new LocaleValue("销售订单号"));
            //var FOPENAMOUNTFOR = header.AddChild("FOPENAMOUNTFOR", new LocaleValue("已开票核销金额"), SqlStorageType.SqlDecimal);
            //var FCOMMENT = header.AddChild("FCOMMENT", new LocaleValue("备注"));
            //var FISFREE = header.AddChild("FISFREE", new LocaleValue("是否赠品"));
            //var FRECEIVEAMOUNT = header.AddChild("FRECEIVEAMOUNT", new LocaleValue("已结算金额"), SqlStorageType.SqlDecimal);
            //var FNORECEIVEAMOUNT = header.AddChild("FNORECEIVEAMOUNT", new LocaleValue("未结算金额"), SqlStorageType.SqlDecimal);
            //var FNOINVOICEAMOUNT = header.AddChild("FNOINVOICEAMOUNT", new LocaleValue("未开票核销金额"), SqlStorageType.SqlDecimal);
            //var FNOINVOICEQTY = header.AddChild("FNOINVOICEQTY", new LocaleValue("未开票核销数量"), SqlStorageType.SqlDecimal);
            //var F_PENY_DIFFALLAMOUNT = header.AddChild("F_PENY_DIFFALLAMOUNT", new LocaleValue("价税差额"), SqlStorageType.SqlDecimal);
            //var FPGNAME = header.AddChild("FPGNAME", new LocaleValue("大类"));
            //var FGNAME = header.AddChild("FGNAME", new LocaleValue("小类"));
            //var FIVCODE = header.AddChild("FIVCODE", new LocaleValue("开票编码"));
            //FIVCODE.Width = 400;
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
            //result.Add(new SummaryField("FPRICEUNITID", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FPRICEQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FTAXPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FENTRYTAXRATE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FENTRYDISCOUNTRATE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FNOTAXAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FTAXAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FALLAMOUNTFOR_D", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FOPENAMOUNTFOR", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FRECEIVEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FNORECEIVEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FNOINVOICEAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FNOINVOICEQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("F_PENY_DIFFALLAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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
            var result = "";
            var CustomerFrom = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerFrom"])?["Number"];
            var CustomerTo = ((DynamicObject)filter.FilterParameter.CustomFilter["CustomerTo"])?["Number"];
            if (!(CustomerFrom is null))
            {
                result += $" and t1.FNUMBER BETWEEN '{CustomerFrom}' AND '{CustomerTo}'";
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
