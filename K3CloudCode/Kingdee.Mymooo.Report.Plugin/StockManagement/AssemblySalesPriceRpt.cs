using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.StockManagement
{
    [Description("组装销售价报表"), HotUpdate]
    public class AssemblySalesPriceRpt : SysReportBaseService
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
            }
            return reportTitles;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                string where = " where t1.FSTOCKORGID=7401780 and t1.FDOCUMENTSTATUS='C' and t1.FAFFAIRTYPE='Assembly' ";

                //单据日期开始日期
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StartDate"])))
                {
                    where += $" and t1.FDATE>='{Convert.ToString(customFilter["F_Filter_StartDate"])}' ";
                }

                //单据日期结束日期
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_EndDate"])))
                {
                    where += $" and t1.FDATE<'{DateTime.Parse(Convert.ToString(customFilter["F_Filter_EndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
                }

                string sql = $@"/*dialect*/select {KSQL_SEQ},
                            t1.FBILLNO,t2.FSEQ,t1.FDATE,t3.FNUMBER FItemNo,t4.FNAME FItemName,t2.FQty,t5.FNAME FStockName,t6.FNAME FUnitName,t1.FSTOCKORGID,convert(decimal(23,10),0) as FSalesPrice
                            into {tableName}
                            from T_STK_ASSEMBLY t1
                            inner join T_STK_ASSEMBLYPRODUCT t2 on t1.FID=t2.FID
                            left join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
                            left join T_BD_MATERIAL_L t4 on t3.FMATERIALID=t4.FMATERIALID
                            left join t_BD_Stock_L t5 on t2.FSTOCKID=t5.FSTOCKID
                            left join t_BD_Unit_L t6 on t2.FUNITID=t6.FUNITID
                            {where} ";

                //排序
                string dataSort = Convert.ToString(filter.FilterParameter.SortString);
                if (dataSort != "")
                {
                    sql = string.Format(sql, dataSort);
                }
                else
                {
                    sql = string.Format(sql, " t1.FDATE desc ");
                }
                DBUtils.Execute(this.Context, sql);

                //更新销售价
                sql = $@"/*dialect*/update {tableName} set FSalesPrice=(select top 1 t3.FTAXPRICE from T_SAL_ORDER t1
						inner join T_SAL_ORDERENTRY t2 on t1.FID=t2.FID
						inner join T_SAL_ORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
                        where t1.FDOCUMENTSTATUS='C' and t3.FTAXPRICE>0  and t4.FNUMBER={tableName}.FItemNo and t2.FSupplyTargetOrgId={tableName}.FSTOCKORGID order by t1.FAuditTime desc) ";
                DBServiceHelper.Execute(this.Context, sql);
                cope.Complete();
            }
        }
    }
}
