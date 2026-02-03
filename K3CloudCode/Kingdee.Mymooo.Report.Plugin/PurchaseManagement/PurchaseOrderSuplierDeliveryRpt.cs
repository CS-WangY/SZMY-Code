using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.PurchaseManagement
{
    [Description("供应商回复交期报表"), HotUpdate]
    public class PurchaseOrderSuplierDeliveryRpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            this.ReportProperty.IsGroupSummary = false;
        }

        /// <summary>
        /// 获取表头
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            //把过滤条件的内容，全部传入filter
            ReportTitles reportTitles = new ReportTitles();
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            if (customFilter != null)
            {
                reportTitles.AddTitle("F_PENY_PONO", Convert.ToString(customFilter["F_Filter_PONO"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_PONO"]));
                reportTitles.AddTitle("F_PENY_POSEQ", Convert.ToString(customFilter["F_Filter_POSEQ"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_POSEQ"]));
                reportTitles.AddTitle("F_PENY_MATERIALCODE", Convert.ToString(customFilter["F_Filter_MATERIALCODE"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_MATERIALCODE"]));
                reportTitles.AddTitle("F_PENY_MATERIALNAME", Convert.ToString(customFilter["F_Filter_MATERIALNAME"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_MATERIALNAME"]));
            }
            return reportTitles;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            string where = " where 1=1 ";

            //采购单号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_PONO"])))
            {
                where += $" and t1.FSOURCEBILLNO='{Convert.ToString(customFilter["F_Filter_PONO"]).Trim()}' ";
            }

            //采购序号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_POSEQ"])))
            {
                where += $" and t1.FPOSEQ='{Convert.ToString(customFilter["F_Filter_POSEQ"]).Trim()}' ";
            }

            //物料编号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MATERIALCODE"])))
            {
                where += $" and t3.FNUMBER='{Convert.ToString(customFilter["F_Filter_MATERIALCODE"]).Trim()}' ";
            }

            //物料名称
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MATERIALNAME"])))
            {
                where += $" and t4.FNAME  like '%{Convert.ToString(customFilter["F_Filter_MATERIALNAME"]).Trim()}%' ";
            }

            string sql = $@"/*dialect*/select {KSQL_SEQ},t2.FNAME FPURCHASEORGNAME,t1.FSOURCEBILLNO FPONO,t1.FPOSEQ,t1.FDATE,t1.FREMARKS,
                            t3.FNUMBER FMATERIALCODE,t4.FNAME FMATERIALNAME,t5.FNAME FCREATORNAME,t1.F_PENY_CREATEDATE FCREATEDATE
                            into {tableName}
                            from T_PUR_SuplierDelivery t1
                            inner join T_ORG_ORGANIZATIONS_L t2 on t1.F_PENY_PURCHASEORGID=t2.FORGID and t2.FLOCALEID=2052
                            inner join T_BD_MATERIAL t3 on t3.FMATERIALID=t1.FMATERIALID
                            inner join T_BD_MATERIAL_L t4 on t3.FMATERIALID=t4.FMATERIALID and t4.FLOCALEID=2052
                            left join T_SEC_USER t5 on t5.FUSERID=t1.F_PENY_CREATORID {where} ";

            //排序
            string dataSort = Convert.ToString(filter.FilterParameter.SortString);
            if (dataSort != "")
            {
                sql = string.Format(sql, dataSort);
            }
            else
            {
                sql = string.Format(sql, " t1.F_PENY_CREATEDATE desc ");
            }
            DBUtils.Execute(this.Context, sql);
        }
    }
}
