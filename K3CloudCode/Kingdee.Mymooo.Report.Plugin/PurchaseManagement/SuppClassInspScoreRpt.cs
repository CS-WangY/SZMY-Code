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
	[Description("供应商小类检验评分报表"), HotUpdate]
	public class SuppClassInspScoreRpt : SysReportBaseService
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

				reportTitles.AddTitle("F_PENY_SupplierName", Convert.ToString(customFilter["F_Filter_SupplierCode"]) == "" ? "全部" : ((DynamicObject)customFilter["F_Filter_SupplierCode"])["Name"].ToString());
				reportTitles.AddTitle("F_PENY_StringencyName", Convert.ToString(customFilter["F_Filter_StringencyName"]) == "" ? "全部" : GetStringencyName(Convert.ToString(customFilter["F_Filter_StringencyName"])));

			}
			return reportTitles;
		}
		//1、正常检验
		//2、加严检验
		//3、放宽检验
		//4、全检
		//5、免检
		public string GetStringencyName(string StringencyId)
		{
			string values = "";
			switch (StringencyId)
			{
				case "1":
					values = "正常检验";
					break;
				case "2":
					values = "加严检验";
					break;
				case "3":
					values = "放宽检验";
					break;
				case "4":
					values = "全检";
					break;
				case "5":
					values = "免检";
					break;

			}
			return values;
		}


		//创建临时报表
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			string where = " where 1=1 ";

			//供应商
			if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_SupplierCode"])))
			{
				where += $" and t3.FNUMBER='{Convert.ToString(((DynamicObject)customFilter["F_Filter_SupplierCode"])["Number"].ToString()).Trim()}' ";
			}

			//检验严格度
			if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StringencyName"])))
			{
				where += $" and t1.FStringencyId={Convert.ToInt16(customFilter["F_Filter_StringencyName"])} ";
			}

			string sql = $@"/*dialect*/select {KSQL_SEQ},t1.FID,t2.FNAME FOrgName,t3.FNUMBER FSupplierCode,t4.FNAME FSupplierName,gll.FNAME FParentSmallName,gl.FNAME FSmallName,
							t1.FFraction FFraction,t5.FStringencyName FStringencyName
							into {tableName}
							from PENY_T_SupplierClassInspectScore t1
							inner join T_ORG_ORGANIZATIONS_L t2 on t1.FORGID=t2.FORGID and t2.FLOCALEID=2052
							inner join t_BD_Supplier t3 on t1.FSUPPLIERID=t3.FSUPPLIERID
							inner join T_BD_SUPPLIER_L t4 on t3.FSUPPLIERID=t4.FSUPPLIERID and t4.FLOCALEID=2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
							left join PENY_T_StrictnessStandardsSet t5 on t1.FStringencyId=t5.FStringencyId
                            {where} ";

			//排序
			string dataSort = Convert.ToString(filter.FilterParameter.SortString);
			if (dataSort != "")
			{
				sql = string.Format(sql, dataSort);
			}
			else
			{
				sql = string.Format(sql, " t1.FMODIFYDATE desc ");
			}
			DBUtils.Execute(this.Context, sql);


		}

		//设置汇总列信息
		public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
		{
			var result = base.GetSummaryColumnInfo(filter);
			result.Add(new SummaryField("FID", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.COUNT));
			return result;
		}
	}
}
