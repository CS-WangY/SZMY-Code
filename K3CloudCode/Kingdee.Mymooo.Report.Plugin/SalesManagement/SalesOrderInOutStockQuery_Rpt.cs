using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Text;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售送退货记录查询报表"), HotUpdate]
    public class SalesOrderInOutStockQuery_Rpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            //this.ReportProperty.IsGroupSummary = false;
            //this.ReportProperty.IdentityFieldName = "FENTRYID";
        }
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            ReportTitles reportTitles = new ReportTitles();
            foreach (var item in filter.FilterParameter.CustomFilter.DynamicObjectType.Properties)
            {
                switch (item.PropertyType.Name)
                {
                    case "DynamicObject":
                        reportTitles.AddTitle(item.Name + "LAB", Convert.ToString(((DynamicObject)filter.FilterParameter.CustomFilter[item])?["Name"] ?? ""));
                        break;
                    default:
                        reportTitles.AddTitle(item.Name + "LAB", Convert.ToString(filter.FilterParameter.CustomFilter[item] ?? ""));
                        break;
                }

            }
            return reportTitles;
        }
        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            string sTable1 = @"SELECT TE.FENTRYID,T.FCREATEDATE AS FDATE,T.FAPPROVEDATE,T.FBILLNO,TE.FSEQ
,TER.FSOORDERNO,TER.FSOENTRYID,TSO.FSEQ AS FSOSEQ,T.FCREATEDATE
,TC.FNUMBER AS FCNUMBER,TCL.FNAME AS FCNAME,VSAL.FNAME AS FSALNAME,M.FNUMBER AS FMNUMBER,ML.FNAME AS FMNAME,TE.FCUSTITEMNO AS FCUNUMBER,TE.FCUSTITEMNAME AS FCUNAME
,BDUL.FNAME AS FUNAME
,TE.FREALQTY,RE.FPRICE,RE.FTAXPRICE,TEF.FAMOUNT,RE.FWRITTENOFFAMOUNTFOR,RE.FHadMatchAmountFor
,REP.FENDDATE,REP.FPAYAMOUNT,REP.FNOTWRITTENOFFAMOUNTFOR
,CASE WHEN REP.FENDDATE<GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR>0
	THEN '逾期'
	ELSE '正常'
END AS FSFYQWSK
,CASE WHEN REP.FENDDATE<GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR>0
	THEN REP.FNOTWRITTENOFFAMOUNTFOR
	ELSE 0
END AS FSFYQWSKJE
,'送货' AS FTYPE
,TE.FPROJECTNO
,CASE REP.FWRITTENOFFSTATUS
	WHEN 'A' THEN '空'
	WHEN 'B' THEN '部分'
	WHEN 'C' THEN '完全'
	ELSE '空'
END AS FWRITTENOFFSTATUS
,R.FBILLNO AS FARNO,R.FCREATEDATE AS FARDATE
,STL.FNAME AS FTLNAME
,PMSG.FNAME AS FPCLASSNAME,MSG.FNAME AS FCLASSNAME
FROM T_SAL_OUTSTOCKENTRY TE
LEFT JOIN T_SAL_OUTSTOCKENTRY_R TER ON TE.FENTRYID=TER.FENTRYID
LEFT JOIN T_SAL_OUTSTOCKENTRY_F TEF ON TE.FENTRYID=TEF.FENTRYID
LEFT JOIN T_AR_RECEIVABLEENTRY_LK REL ON TE.FID=REL.FSBILLID AND TE.FENTRYID=REL.FSID
LEFT JOIN T_AR_RECEIVABLEENTRY RE ON RE.FENTRYID=REL.FENTRYID
LEFT JOIN t_AR_receivablePlan REP ON REP.FID=RE.FID
LEFT JOIN t_AR_receivable R ON RE.FID=R.FID
LEFT JOIN T_SAL_ORDERENTRY TSO ON TER.FSOENTRYID=TSO.FENTRYID
LEFT JOIN T_BD_MATERIAL M ON TE.FMATERIALID=M.FMATERIALID
LEFT JOIN T_BD_MATERIAL_L ML ON ML.FMATERIALID=M.FMATERIALID
LEFT JOIN T_BD_MATERIALGROUP_L MSG ON TE.FSMALLID=MSG.FID
LEFT JOIN T_BD_MATERIALGROUP_L PMSG ON TE.FPARENTSMALLID=PMSG.FID
LEFT JOIN T_SAL_OUTSTOCK T ON T.FID=TE.FID
LEFT JOIN T_SAL_OUTSTOCKFIN TF ON T.FID=TF.FID
LEFT JOIN T_BD_SETTLETYPE_L STL ON STL.FID=TF.FSETTLETYPEID
LEFT JOIN T_BD_CUSTOMER TC ON T.FCUSTOMERID=TC.FCUSTID
LEFT JOIN T_BD_CUSTOMER_L TCL ON TCL.FCUSTID=TC.FCUSTID
LEFT JOIN V_BD_SALESMAN_L VSAL ON T.FSALESMANID=VSAl.FID
LEFT JOIN T_BD_UNIT_L BDUL ON BDUL.FUNITID=TE.FUNITID
WHERE 1=1";
            string sTable2 = @"SELECT TE.FENTRYID,T.FCREATEDATE AS FDATE,T.FAPPROVEDATE,T.FBILLNO,TE.FSEQ
,TE.FORDERNO,TE.FSOENTRYID,TSO.FSEQ AS FSOSEQ,T.FCREATEDATE
,TC.FNUMBER AS FCNUMBER,TCL.FNAME AS FCNAME,VSAL.FNAME,M.FNUMBER,ML.FNAME, TE.FCUSTITEMNO AS FNUMBER,TE.FCUSTITEMNAME AS FNAME
,BDUL.FNAME
,TE.FREALQTY,RE.FPRICE,RE.FTAXPRICE,TEF.FAMOUNT,RE.FWRITTENOFFAMOUNTFOR,RE.FHadMatchAmountFor
,REP.FENDDATE,REP.FPAYAMOUNT,REP.FNOTWRITTENOFFAMOUNTFOR
,CASE WHEN REP.FENDDATE<GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR>0
	THEN '逾期'
	ELSE '正常'
END AS FSFYQWSK
,CASE WHEN REP.FENDDATE<GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR>0
	THEN REP.FNOTWRITTENOFFAMOUNTFOR
	ELSE 0
END AS FSFYQWSKJE
,'退货' AS FTYPE
,TE.FPROJECTNO
,CASE REP.FWRITTENOFFSTATUS
	WHEN 'A' THEN '空'
	WHEN 'B' THEN '部分'
	WHEN 'C' THEN '完全'
	ELSE '空'
END AS FWRITTENOFFSTATUS
,R.FBILLNO,R.FCREATEDATE
,STL.FNAME
,PMSG.FNAME,MSG.FNAME
FROM T_SAL_RETURNSTOCKENTRY TE
LEFT JOIN T_SAL_RETURNSTOCKENTRY_R TER ON TE.FENTRYID=TER.FENTRYID
LEFT JOIN T_SAL_RETURNSTOCKENTRY_F TEF ON TE.FENTRYID=TEF.FENTRYID
LEFT JOIN T_SAL_RETURNSTOCK T ON T.FID=TE.FID
LEFT JOIN T_SAL_RETURNSTOCKFIN TF ON T.FID=TF.FID
LEFT JOIN T_AR_RECEIVABLEENTRY_LK REL ON TE.FID=REL.FSBILLID AND TE.FENTRYID=REL.FSID
LEFT JOIN T_AR_RECEIVABLEENTRY RE ON RE.FENTRYID=REL.FENTRYID
LEFT JOIN t_AR_receivablePlan REP ON REP.FID=RE.FID
LEFT JOIN t_AR_receivable R ON RE.FID=R.FID
LEFT JOIN T_SAL_ORDERENTRY TSO ON TE.FSOENTRYID=TSO.FENTRYID
LEFT JOIN T_BD_MATERIAL M ON TE.FMATERIALID=M.FMATERIALID
LEFT JOIN T_BD_MATERIAL_L ML ON ML.FMATERIALID=M.FMATERIALID
LEFT JOIN T_BD_MATERIALGROUP_L MSG ON TE.FSMALLID=MSG.FID
LEFT JOIN T_BD_MATERIALGROUP_L PMSG ON TE.FPARENTSMALLID=PMSG.FID
LEFT JOIN T_BD_SETTLETYPE_L STL ON STL.FID=TF.FSETTLETYPEID
LEFT JOIN T_BD_CUSTOMER TC ON T.FRETCUSTID=TC.FCUSTID
LEFT JOIN T_BD_CUSTOMER_L TCL ON TCL.FCUSTID=TC.FCUSTID
LEFT JOIN V_BD_SALESMAN_L VSAL ON T.FSALESMANID=VSAl.FID
LEFT JOIN T_BD_UNIT_L BDUL ON BDUL.FUNITID=TE.FUNITID
WHERE 1=1";
            StringBuilder sWhere = new StringBuilder();
            StringBuilder sWhere2 = new StringBuilder();
            int iType = 0;
            if (filter.FilterParameter.CustomFilter["FCUSTOMERID"] != null)
            {
                sWhere.AppendLine(" AND T.FCUSTOMERID='" + filter.FilterParameter.CustomFilter["FCUSTOMERID_Id"] + "'");
                sWhere2.AppendLine(" AND T.FRETCUSTID='" + filter.FilterParameter.CustomFilter["FCUSTOMERID_Id"] + "'");
            }
            if (!string.IsNullOrWhiteSpace((filter.FilterParameter.CustomFilter["FBILLNO"] ?? "").ToString()))
            {
                sWhere.AppendLine($" AND T.FBILLNO LIKE '%{filter.FilterParameter.CustomFilter["FBILLNO"]}%'");
                sWhere2.AppendLine($" AND T.FBILLNO LIKE '%{filter.FilterParameter.CustomFilter["FBILLNO"]}%'");
            }
            if (!string.IsNullOrWhiteSpace((filter.FilterParameter.CustomFilter["FCUSTMATMAPCODE"] ?? "").ToString()))
            {
                sWhere.AppendLine($" AND TE.FCUSTITEMNO LIKE '%{filter.FilterParameter.CustomFilter["FCUSTMATMAPCODE"]}%'");
                sWhere2.AppendLine($" AND TE.FCUSTITEMNO LIKE '%{filter.FilterParameter.CustomFilter["FCUSTMATMAPCODE"]}%'");
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FilterParameter.CustomFilter["FTYPE"])))
            {
                iType = Convert.ToInt32(filter.FilterParameter.CustomFilter["FTYPE"]);
            }
            if (filter.FilterParameter.CustomFilter["FAPPROVEDATESTAR"] != null && filter.FilterParameter.CustomFilter["FAPPROVEDATEEND"] != null)
            {
                string dStar = Convert.ToDateTime(filter.FilterParameter.CustomFilter["FAPPROVEDATESTAR"]).ToString();
                string dEnd = Convert.ToDateTime(filter.FilterParameter.CustomFilter["FAPPROVEDATEEND"]).AddDays(1).AddSeconds(-1).ToString();
                sWhere.AppendLine($" AND T.FAPPROVEDATE BETWEEN '{dStar}' AND '{dEnd}'");
                sWhere2.AppendLine($" AND T.FAPPROVEDATE BETWEEN '{dStar}' AND '{dEnd}'");
            }
            if (!string.IsNullOrWhiteSpace((filter.FilterParameter.CustomFilter["FSOORDERNO"] ?? "").ToString()))
            {
                sWhere.AppendLine(" AND TER.FSOORDERNO LIKE '%" + filter.FilterParameter.CustomFilter["FSOORDERNO"] + "%'");
                sWhere2.AppendLine(" AND TE.FORDERNO LIKE '%" + filter.FilterParameter.CustomFilter["FSOORDERNO"] + "%'");
            }
            if (filter.FilterParameter.CustomFilter["FSALESMANID"] != null)
            {
                sWhere.AppendLine(" AND T.FSALESMANID='" + filter.FilterParameter.CustomFilter["FSALESMANID_Id"] + "'");
                sWhere2.AppendLine(" AND T.FSALESMANID='" + filter.FilterParameter.CustomFilter["FSALESMANID_Id"] + "'");
            }
            if (filter.FilterParameter.CustomFilter["FMATERIALID"] != null)
            {
                sWhere.AppendLine(" AND TE.FMATERIALID='" + filter.FilterParameter.CustomFilter["FMATERIALID_Id"] + "'");
                sWhere2.AppendLine(" AND TE.FMATERIALID='" + filter.FilterParameter.CustomFilter["FMATERIALID_Id"] + "'");
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FilterParameter.CustomFilter["FSFYQWSK"])))
            {
                switch (Convert.ToString(filter.FilterParameter.CustomFilter["FSFYQWSK"]))
                {
                    case "0":
                        sWhere.AppendLine(" AND REP.FENDDATE< GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR> 0");
                        sWhere2.AppendLine(" AND REP.FENDDATE< GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR> 0");
                        break;
                    case "1":
                        sWhere.AppendLine(" AND REP.FENDDATE> GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR< 0");
                        sWhere2.AppendLine(" AND REP.FENDDATE> GETDATE() AND REP.FNOTWRITTENOFFAMOUNTFOR< 0");
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FilterParameter.CustomFilter["FOPENSTATUS"])))
            {
                sWhere.AppendLine(" AND RE.FOPENSTATUS='" + filter.FilterParameter.CustomFilter["FOPENSTATUS"] + "'");
                sWhere2.AppendLine(" AND RE.FOPENSTATUS='" + filter.FilterParameter.CustomFilter["FOPENSTATUS"] + "'");
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FilterParameter.CustomFilter["FWRITTENOFFSTATUS"])))
            {
                sWhere.AppendLine(" AND REP.FWRITTENOFFSTATUS='" + filter.FilterParameter.CustomFilter["FWRITTENOFFSTATUS"] + "'");
                sWhere2.AppendLine(" AND REP.FWRITTENOFFSTATUS='" + filter.FilterParameter.CustomFilter["FWRITTENOFFSTATUS"] + "'");
            }

            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FilterParameter.CustomFilter["F_PENY_Combo2"])))
            {
                sWhere.AppendLine(" AND REP.FWRITTENOFFSTATUS='" + filter.FilterParameter.CustomFilter["F_PENY_Combo2"] + "'");
                sWhere2.AppendLine(" AND REP.FWRITTENOFFSTATUS='" + filter.FilterParameter.CustomFilter["F_PENY_Combo2"] + "'");
            }
            if (filter.FilterParameter.CustomFilter["FARDATESTAR"] != null && filter.FilterParameter.CustomFilter["FARDATEEND"] != null)
            {
                string dStar = Convert.ToDateTime(filter.FilterParameter.CustomFilter["FARDATESTAR"]).ToString();
                string dEnd = Convert.ToDateTime(filter.FilterParameter.CustomFilter["FARDATEEND"]).AddDays(1).AddSeconds(-1).ToString();
                sWhere.AppendLine($" AND R.FCREATEDATE BETWEEN '{dStar}' AND '{dEnd}'");
                sWhere2.AppendLine($" AND R.FCREATEDATE BETWEEN '{dStar}' AND '{dEnd}'");
            }

            StringBuilder tempTables = new StringBuilder();
            switch (iType)
            {
                case 0:
                    tempTables.AppendLine(sTable1);
                    tempTables.AppendLine(sWhere.ToString());
                    tempTables.AppendLine("UNION ALL");
                    tempTables.AppendLine(sTable2);
                    tempTables.AppendLine(sWhere2.ToString());
                    break;
                case 1:
                    tempTables.AppendLine(sTable1);
                    tempTables.AppendLine(sWhere.ToString());
                    break;
                case 2:
                    tempTables.AppendLine(sTable2);
                    tempTables.AppendLine(sWhere2.ToString());
                    break;
            }
            string sql = $@"/*dialect*/SELECT {KSQL_SEQ},* into {tableName} FROM 
                        (
                        {tempTables}
                        ) t1";
            sql = string.Format(sql, " FDATE ");
            DBUtils.Execute(this.Context, sql);
        }
    }
}
