using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单列表查询物料情况报表"), HotUpdate]
    public class SalOrderMaterialQuery_Rpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            //this.ReportProperty.IsGroupSummary = false;
        }
        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            if (filter.CustomParams.ContainsKey("OpenParameter"))
            {
                Dictionary<string, object> OpenParameter = (Dictionary<string, object>)filter.CustomParams["OpenParameter"];
                if (OpenParameter.ContainsKey("FMATERIALID"))
                {
                    //
                    var materialid = OpenParameter["FMATERIALID"].ToString();
                    string sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} From (SELECT t1.FID,t1.FENTRYID,t3.FSALEORGID,t1.FSUPPLYTARGETORGID AS FSTOCKORGID
                                    ,t3.FBILLNO,t1.FMATERIALID,M.FMASTERID,M.FNumber AS FMNumber,ML.FName AS FMName
                                    ,t3.FCREATEDATE,t1.FQTY,t2.FREMAINOUTQTY,t4.FNAME AS FSALNAME,t5.FNAME AS FCREATENAME
                                    ,0 AS FAVBQTYText,0 AS FBASEQTYText
                                    FROM T_SAL_ORDERENTRY t1
                                    INNER JOIN T_SAL_ORDERENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
                                    INNER JOIN T_BD_MATERIAL M ON t1.FMATERIALID=M.FMATERIALID
                                    INNER JOIN T_BD_MATERIAL_L ML ON M.FMATERIALID=ML.FMATERIALID
                                    INNER JOIN T_SAL_ORDER t3 ON t1.FID=t3.FID
                                    INNER JOIN V_BD_SALESMAN_L t4 ON t3.FSALERID=t4.FID
                                    INNER JOIN T_SEC_user t5 ON t3.FCREATORID=t5.FUSERID
                                    WHERE t1.FMATERIALID='{materialid}' AND t2.FREMAINOUTQTY>0 AND t3.FCloseStatus='A') t1";
                    sql = string.Format(sql, " FCREATEDATE ");
                    DBUtils.Execute(this.Context, sql);
                }
            }
        }
    }

    [Description("销售订单列表查询物料情况报表表单插件"), HotUpdate]
    public class SalOrderMaterialQuery_List : AbstractSysReportPlugIn
    {
        public override void FormatCellValue(BOS.Core.Report.PlugIn.Args.FormatCellValueArgs args)
        {
            base.FormatCellValue(args);
            string sSql = "";
            string strlist = "";
            DynamicObjectCollection dolist = null;
            if (args.Header.Key.Equals("FAVBQTYText", StringComparison.OrdinalIgnoreCase))
            {
                var billid = args.DataRow["FID"];
                var entryid = args.DataRow["FENTRYID"];
                var materialid = args.DataRow["FMATERIALID"];
                sSql = $@"SELECT * FROM v_ReservedStockAll WHERE FSRCENTRYID={entryid}";
                dolist = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                strlist = String.Join(" ", (from d in dolist
                                            select
                                            Convert.ToString(d["stockname"])
                                            + ":" +
                                            Convert.ToDecimal(d["FBASEQTY"]).ToString("0.##")
                                            + "(" +
                                            Convert.ToString(d["FRESERVETYPE"])
                                            + ")").ToList());
                args.FormateValue = string.Format("{0}", strlist);
            }
            else if (args.Header.Key.Equals("FBASEQTYText", StringComparison.OrdinalIgnoreCase))
            {
                var msterID = args.DataRow["FMASTERID"];
                var orgid = args.DataRow["FSTOCKORGID"];
                //sSql = $@"SELECT FMATERIALID FROM T_BD_MATERIAL WHERE FMASTERID='{msterID}' AND FUSEORGID='{orgid}'";
                //var materialid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0);

                sSql = $@"/*dialect*/if object_id(N'tempdb..#STK_INVENTORY_TEMP',N'U') is not null
DROP TABLE #STK_INVENTORY_TEMP;
SELECT FMATERIALID,FSTOCKID,FSTOCKORGID,FAVBQTY INTO #STK_INVENTORY_TEMP
FROM V_STK_INVENTORY_CUS WHERE FAVBQTY>0
CREATE INDEX IX_#STK_INVENTORY_TEMP_FMATERIALID ON #STK_INVENTORY_TEMP (FMATERIALID);
SELECT t6.FNAME as ORGNAME,t1.FSTOCKID,ISNULL(t1.FAVBQTY,0) as FBASEQTY
,t4.FNAME,t1.FMATERIALID,t1.FSTOCKORGID FROM #STK_INVENTORY_TEMP t1
LEFT JOIN T_BD_STOCK_L t4 on t1.FSTOCKID=t4.FSTOCKID
LEFT JOIN T_ORG_ORGANIZATIONS_L t6 ON t1.FSTOCKORGID=t6.FORGID
WHERE t1.FMATERIALID='{msterID}' AND t1.FSTOCKORGID='{orgid}'";
                dolist = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                strlist = String.Join(" ", (from d in dolist select Convert.ToString(d["ORGNAME"]) + ":" + Convert.ToDecimal(d["FBASEQTY"]).ToString("0.##")).ToList());
                args.FormateValue = string.Format("{0}", strlist);
            }
        }
    }
}
