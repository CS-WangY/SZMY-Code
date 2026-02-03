using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.PurchaseManagement
{

	[Description("验评分更新变化明细报表"), HotUpdate]
	public class InspectScoreChangeDetailsRpt : SysReportBaseService
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

		//创建临时报表
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			if (filter.CustomParams.ContainsKey("OpenParameter"))
			{
				Dictionary<string, object> OpenParameter = (Dictionary<string, object>)filter.CustomParams["OpenParameter"];
				if (OpenParameter.ContainsKey("FId"))
				{
					var fid = Convert.ToInt64(OpenParameter["FId"]);
					string sql = $@"/*dialect*/select {KSQL_SEQ},t1.FID,t2.FNAME FOrgName,t3.FNUMBER FSupplierCode,t4.FNAME FSupplierName,gll.FNAME FParentSmallName,gl.FNAME FSmallName,
							t1.FOldFraction,t1.FFraction,t1.FNewFraction,t5.FStringencyName FOldStringencyName,t6.FStringencyName FNewStringencyName,
							case when t1.FractionSource='Complaint' then '客诉' when t1.FractionSource='WeekPassRate' then '周合格率统计' else '' end FractionSource,
							t1.FNode,t1.FCREATEDATE
							into {tableName}
							from PENY_T_InspectScoreChangeDetails t1
							inner join T_ORG_ORGANIZATIONS_L t2 on t1.FORGID=t2.FORGID and t2.FLOCALEID=2052
							inner join t_BD_Supplier t3 on t1.FSUPPLIERID=t3.FSUPPLIERID
							inner join T_BD_SUPPLIER_L t4 on t3.FSUPPLIERID=t4.FSUPPLIERID and t4.FLOCALEID=2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
							left join PENY_T_StrictnessStandardsSet t5 on t1.FOldStringencyId=t5.FStringencyId
							left join PENY_T_StrictnessStandardsSet t6 on t1.FNewStringencyId=t6.FStringencyId
							where t1.FParentId={fid} ";
					sql = string.Format(sql, " t1.FCREATEDATE desc ");
					DBUtils.Execute(this.Context, sql);
				}
			}
		}

	}
}
