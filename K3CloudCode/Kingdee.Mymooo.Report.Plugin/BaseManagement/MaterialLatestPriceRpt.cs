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

namespace Kingdee.Mymooo.Report.Plugin.BaseManagement
{
    [Description("物料最新采购价报表"), HotUpdate]
    public class MaterialLatestPriceRpt : SysReportBaseService
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
                reportTitles.AddTitle("F_PENY_MATERIALCode", customFilter["FMATERIALID"] == null ? "全部" : ((DynamicObject)customFilter["FMATERIALID"])["Number"].ToString());
                reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["FStartDate"]) == "" ? "全部" : DateTime.Parse(customFilter["FStartDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["FEndDate"]) == "" ? "全部" : DateTime.Parse(customFilter["FEndDate"].ToString()).ToString("yyyy-MM-dd"));
            }
            return reportTitles;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            string where = "";
            string jdWhere = "";
            //物料Code
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FMATERIALID"])))
            {
                where = $" and pod.ITEM_NO='{((DynamicObject)customFilter["FMATERIALID"])["Number"]}' ";
                jdWhere = $" and mat.FNUMBER='{((DynamicObject)customFilter["FMATERIALID"])["Number"]}' ";
            }

            //采购订单日期开始日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FStartDate"])))
            {
                where += $" and po.PO_DATE>='{Convert.ToString(customFilter["FStartDate"])}' ";
                jdWhere += $" and m.FDATE>='{Convert.ToString(customFilter["FStartDate"])}' ";
            }

            //采购订单日期结束日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FEndDate"])))
            {
                where += $" and po.PO_DATE<'{DateTime.Parse(Convert.ToString(customFilter["FEndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
                jdWhere += $" and m.FDATE<'{DateTime.Parse(Convert.ToString(customFilter["FEndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
            }

            string sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from (
                        select ROW_NUMBER() OVER (PARTITION BY FMaterialNo ORDER BY PO_DATE desc) rn,FMaterialNo,FMaterialName,isnull(FPoPrice,0) FPoPrice  from (
                        select 
                        po.PO_DATE,pod.ITEM_NO FMaterialNo,pod.ITEM_DESC FMaterialName,pod.VAT_PRICE FPoPrice,VDR_NAMEC from M_POD_DET pod
                        inner join M_PO_MSTR po on pod.New_PO_NO=po.New_PO_NO
                        where po.VDR_INTERIOR=0 and po.COMP_CODE in('MYMO','MYCA','MYZZ') and pod.VAT_PRICE>0 {where}
                        union all
                        select m.FDATE,mat.FNUMBER,matl.FNAME,f.FTAXPRICE,ven_l.FNAME VDR_NAMEC from  t_PUR_POOrderEntry d
                        inner join t_PUR_POOrder m on m.FID=d.FID
                        inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                        inner join  t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID and ven.FCorrespondOrgId=0
                        left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID
                        left join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
                        left join  T_BD_MATERIAL_L matl on matl.FMATERIALID=mat.FMATERIALID and matl.FLOCALEID=2052
                        where m.FDOCUMENTSTATUS='C' and m.FCANCELSTATUS='A' and f.FTAXPRICE >0  {jdWhere}
                        ) t1 
                        ) datas where rn=1 ";
            //排序
            string dataSort = Convert.ToString(filter.FilterParameter.SortString);
            if (dataSort != "")
            {
                sql = string.Format(sql, dataSort);
            }
            else
            {
                sql = string.Format(sql, " FMaterialNo ");
            }
            DBUtils.Execute(this.Context, sql);


        }
    }
}
