using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.App.Core.Match.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kingdee.Mymooo.Report.Plugin.ProductionManagement
{
    [Description("生产领料成本报表"), HotUpdate]
    public class PrdPickingCostRpt : SysReportBaseService
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
                if (Convert.ToBoolean(customFilter["F_Filter_IsAllOrg"]))
                {
                    reportTitles.AddTitle("F_PENY_OrgId", "所有组织");
                }
                else
                {
                    reportTitles.AddTitle("F_PENY_OrgId", GetBaseDataNameValue(customFilter["F_Filter_MulBaseOrgList"] as DynamicObjectCollection));
                }
                reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["F_Filter_StartDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_StartDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["F_Filter_EndDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_EndDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_MasterItemCode", Convert.ToString(customFilter["F_Filter_MasterItemCode"]) == "" ? "全部" : Convert.ToString(((DynamicObject)customFilter["F_Filter_MasterItemCode"])["NUMBER"]));
                reportTitles.AddTitle("F_PENY_MoBillNo", Convert.ToString(customFilter["F_Filter_MoBillNo"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_MoBillNo"]));
                reportTitles.AddTitle("F_PENY_OrderType", Convert.ToString(customFilter["F_Filter_OrderType"]) == "" ? "全部" : Convert.ToString(Convert.ToString(customFilter["F_Filter_OrderType"])));
            }
            return reportTitles;
        }

        private string GetBaseDataNameValue(DynamicObjectCollection dyobj)
        {
            List<string> strList = new List<string>();
            foreach (DynamicObject current in dyobj)
            {
                strList.Add(Convert.ToString(((DynamicObject)current["F_Filter_MulBaseOrgList"])["Name"]));
            }
            return string.Join("、", strList);
        }
        private string GetBaseDataValue(DynamicObjectCollection dyobj)
        {
            if (dyobj == null || dyobj.Count == 0)
            {
                return "0";
            }
            List<long> strList = new List<long>();
            foreach (DynamicObject current in dyobj)
            {
                strList.Add(Convert.ToInt64(current["F_Filter_MulBaseOrgList_Id"]));
            }
            return string.Join(",", strList);
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                string where = " ";

                //供货组织
                if (!Convert.ToBoolean(customFilter["F_Filter_IsAllOrg"]))
                {
                    if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MulBaseOrgList"])))
                    {
                        where += $" and t1.FPRDORGID in ({GetBaseDataValue(customFilter["F_Filter_MulBaseOrgList"] as DynamicObjectCollection)}) ";
                    }
                    else
                    {
                        where += $" and t1.FPRDORGID =0  ";
                    }
                }


                //成品物料号
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MasterItemCode"])))
                {
                    where += $" and t6.FNUMBER like '%{Convert.ToString(((DynamicObject)customFilter["F_Filter_MasterItemCode"])["NUMBER"])}%' ";
                }

                //生产单号
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MoBillNo"])))
                {
                    where += $" and t2.FMoBillNo like '%{Convert.ToString(customFilter["F_Filter_MoBillNo"]).Trim()}%' ";
                }

                //订单开始日期
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StartDate"])))
                {
                    where += $" and t1.FDATE>='{Convert.ToString(customFilter["F_Filter_StartDate"])}' ";
                }

                //订单结束日期
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_EndDate"])))
                {
                    where += $" and t1.FDATE<'{DateTime.Parse(Convert.ToString(customFilter["F_Filter_EndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
                }

                string sql = "";
                //领料单
                if (string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_OrderType"])) || Convert.ToString(customFilter["F_Filter_OrderType"]).Equals("领料单"))
                {
                    sql = $@"/*dialect*/select t2.FMoId,t2.FMoBillNo,t2.FMoEntryId,t2.FMoEntrySeq,t1.FDATE,t4.FPlanDemandQty,t4.FQTY FProQty,FSTOCKINQUAAUXQTY FInStockQty,
                                t1.FBILLNO,t2.FSEQ,t2.FAppQty,t2.FActualQty,t2.FSELPRCDRETURNQTY FReturnQty,t2.FPRICE FCostPrice,
                                t6.FNUMBER FMasterItemCode,t7.FNAME FMasterItemName,t8.FNUMBER FItemCode,t9.FNAME FItemName,
                                t10.FNUMBER FProOrgCode,t11.FNAME FProOrgName,'领料单' FOrderType
                                into #AA
                                from T_PRD_PICKMTRL t1
                                inner join T_PRD_PICKMTRLDATA t2 on t1.FID=t2.FID
                                inner join T_PRD_MO t3 on t3.FID=t2.FMoId
                                inner join T_PRD_MOENTRY t4 on t3.FID=t4.FID and t4.FENTRYID=t2.FMoEntryId
                                inner join T_PRD_MOENTRY_A t5 on t4.FENTRYID=t5.FENTRYID
                                left join T_BD_MATERIAL t6 on t6.FMATERIALID=t4.FMATERIALID
                                left join T_BD_MATERIAL_L t7 on t6.FMATERIALID=t7.FMATERIALID and t7.FLOCALEID = 2052
                                left join T_BD_MATERIAL t8 on t8.FMATERIALID=t2.FMATERIALID
                                left join T_BD_MATERIAL_L t9 on t8.FMATERIALID=t9.FMATERIALID and t9.FLOCALEID = 2052
                                left join T_ORG_ORGANIZATIONS t10 on t10.FORGID=t1.FPRDORGID 
                                left join T_ORG_ORGANIZATIONS_L t11 on t11.FORGID=t10.FORGID
                                where t1.FDOCUMENTSTATUS='C' {where} ";
                    DBServiceHelper.Execute(this.Context, sql);
                }
                //补料单
                if (string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_OrderType"])) || Convert.ToString(customFilter["F_Filter_OrderType"]).Equals("补料单"))
                {
                    sql = $@"/*dialect*/select t2.FMoId,t2.FMoBillNo,t2.FMoEntryId,t2.FMoEntrySeq,t1.FDATE,t4.FPlanDemandQty,t4.FQTY FProQty,FSTOCKINQUAAUXQTY FInStockQty,
                                t1.FBILLNO,t2.FSEQ,t22.FAppQty,t22.FActualQty,t22.FSELPRCDRETURNQTY FReturnQty,t2.FPRICE FCostPrice,
                                t6.FNUMBER FMasterItemCode,t7.FNAME FMasterItemName,t8.FNUMBER FItemCode,t9.FNAME FItemName,
                                t10.FNUMBER FProOrgCode,t11.FNAME FProOrgName,'补料单' FOrderType
                                into #BB
                                from T_PRD_FEEDMTRL t1
                                inner join T_PRD_FEEDMTRLDATA t2 on t1.FID=t2.FID
                                inner join T_PRD_FEEDMTRLDATA_Q t22 on t2.FENTRYID=t22.FENTRYID
                                inner join T_PRD_MO t3 on t3.FID=t2.FMoId
                                inner join T_PRD_MOENTRY t4 on t3.FID=t4.FID and t4.FENTRYID=t2.FMoEntryId
                                inner join T_PRD_MOENTRY_A t5 on t4.FENTRYID=t5.FENTRYID
                                left join T_BD_MATERIAL t6 on t6.FMATERIALID=t4.FMATERIALID
                                left join T_BD_MATERIAL_L t7 on t6.FMATERIALID=t7.FMATERIALID and t7.FLOCALEID = 2052
                                left join T_BD_MATERIAL t8 on t8.FMATERIALID=t2.FMATERIALID
                                left join T_BD_MATERIAL_L t9 on t8.FMATERIALID=t9.FMATERIALID and t9.FLOCALEID = 2052
                                left join T_ORG_ORGANIZATIONS t10 on t10.FORGID=t1.FPRDORGID 
                                left join T_ORG_ORGANIZATIONS_L t11 on t11.FORGID=t10.FORGID
                                where t1.FDOCUMENTSTATUS='C' {where} ";
                    DBServiceHelper.Execute(this.Context, sql);
                }
                if (string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_OrderType"])))
                {
                    sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from (
                        select * from #AA
                        union all
                        select * from #BB
                        ) datas ";
                }
                else if (Convert.ToString(customFilter["F_Filter_OrderType"]).Equals("领料单"))
                {
                    sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from #AA ";
                }
                else if (Convert.ToString(customFilter["F_Filter_OrderType"]).Equals("补料单"))
                {
                    sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from #BB ";
                }
                //排序
                string dataSort = Convert.ToString(filter.FilterParameter.SortString);
                if (dataSort != "")
                {
                    sql = string.Format(sql, dataSort);
                }
                else
                {
                    sql = string.Format(sql, " FDATE desc,FMoBillNo,FMoEntrySeq");
                }
                DBServiceHelper.Execute(this.Context, sql);
                cope.Complete();
            }
        }

        //设置汇总列信息
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FMoBillNo", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.COUNT));
            result.Add(new SummaryField("FPlanDemandQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FProQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FInStockQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAppQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FActualQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FReturnQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            return result;
        }
    }
}
