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

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("加急发货申请报表"), HotUpdate]
    public class SalesDnUrgentShipmentRpt : SysReportBaseService
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
                reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["F_Filter_StartDate"]) == "" ? "全部" : Convert.ToDateTime(customFilter["F_Filter_StartDate"]).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["F_Filter_EndDate"]) == "" ? "全部" : Convert.ToDateTime(customFilter["F_Filter_EndDate"]).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_DnNo", Convert.ToString(customFilter["F_Filter_DnNo"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_DnNo"]));
                reportTitles.AddTitle("F_PENY_CustName", Convert.ToString(customFilter["F_Filter_CustName"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_CustName"]));
                reportTitles.AddTitle("F_PENY_ApprovalNo", Convert.ToString(customFilter["F_Filter_ApprovalNo"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_ApprovalNo"]));
            }
            return reportTitles;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            string where = " where 1=1 ";

            //申请日期开始日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StartDate"])))
            {
                where += $" and FApplyDate>='{Convert.ToString(customFilter["F_Filter_StartDate"])}' ";
            }

            //申请日期结束日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_EndDate"])))
            {
                where += $" and FApplyDate<'{DateTime.Parse(Convert.ToString(customFilter["F_Filter_EndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
            }

            //DN单号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_DnNo"])))
            {
                where += $" and FDnNo like '%{Convert.ToString(customFilter["F_Filter_DnNo"]).Trim()}%' ";
            }

            //审批单号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_ApprovalNo"])))
            {
                where += $" and FApprovalNo='{Convert.ToString(customFilter["F_Filter_ApprovalNo"]).Trim()}' ";
            }

            //客户名称
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_CustName"])))
            {
                where += $" and FCustName like '%{Convert.ToString(customFilter["F_Filter_CustName"]).Trim()}%' ";
            }

            string sql = $@"/*dialect*/select {KSQL_SEQ},*,
                            case when FSpStatus=1 then '审批中' when FSpStatus=2 then '已批核' when FSpStatus=3 then '已驳回' when FSpStatus=4 then '已撤销' when FSpStatus=6 then '通过后撤销' else '其他' end FSpStatusName
                            into {tableName}
                            from T_SAL_DnUrgentShipment {where} ";

            //排序
            string dataSort = Convert.ToString(filter.FilterParameter.SortString);
            if (dataSort != "")
            {
                sql = string.Format(sql, dataSort);
            }
            else
            {
                sql = string.Format(sql, " FApplyDate desc ");
            }
            DBUtils.Execute(this.Context, sql);
        }
    }
}
