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
	[Description("生产入库产值统计表"), HotUpdate]
	public class PrdInStockProdValueRpt : SysReportBaseService
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
					reportTitles.AddTitle("F_PENY_OrgId", GetBaseDataNameValue(Convert.ToString(customFilter["F_Filter_MulBaseOrgList"])));
				}
				reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["F_Filter_StartDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_StartDate"].ToString()).ToString("yyyy-MM-dd"));
				reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["F_Filter_EndDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_EndDate"].ToString()).ToString("yyyy-MM-dd"));
				reportTitles.AddTitle("F_PENY_MATERIALCode", Convert.ToString(customFilter["F_Filter_MATERIALCode_Text"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_MATERIALCode_Text"]));
				reportTitles.AddTitle("F_PENY_MATERIALName", Convert.ToString(customFilter["F_Filter_MATERIALName"]) == "" ? "全部" : Convert.ToString(customFilter["F_Filter_MATERIALName"]));
				reportTitles.AddTitle("F_PENY_WorkShopName", Convert.ToString(customFilter["F_Filter_WorkShopID"]) == "" ? "全部" : GetWorkShopName(Convert.ToString(customFilter["F_Filter_WorkShopID"])));

			}
			return reportTitles;
		}

		private string GetBaseDataNameValue(string dyobj)
		{
			string name = "";
			List<string> strList = new List<string>();
			if (!string.IsNullOrWhiteSpace(dyobj))
			{
				List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
				list2.Add(new SelectorItemInfo("FORGID"));
				list2.Add(new SelectorItemInfo("FNUMBER"));
				list2.Add(new SelectorItemInfo("FNAME"));

				string text = string.Format("{0} IN ({1})", "FORGID", dyobj);
				QueryBuilderParemeter para = new QueryBuilderParemeter
				{
					FormId = "ORG_Organizations",
					SelectItems = list2,
					FilterClauseWihtKey = text
				};
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, para, null);
				foreach (DynamicObject current in dynamicObjectCollection)
				{
					strList.Add(current["FName"].ToString());
				}
				name = string.Join("、", strList);
			}
			return name;
		}

		//获取生产组织
		private string GetWorkShopName(string id)
		{
			string sql = $@"SELECT top 1 t0_L.FFULLNAME ffullname
                            FROM T_BD_DEPARTMENT t0 
                            LEFT OUTER JOIN T_BD_DEPARTMENT_L t0_L ON (t0.FDEPTID = t0_L.FDEPTID AND t0_L.FLocaleId = 2052)
                            WHERE (t0.FISSTOCK = '1') and  t0.FMASTERID='{id}' ";
			return DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "");
		}

		//创建临时报表
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;
			//权限
			var userinfo = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				//生产订单条件
				string where = " ";

				//供货组织
				if (Convert.ToBoolean(customFilter["F_Filter_IsAllOrg"]))
				{
					if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_IsAllOrgID"])))
					{
						where += $" and t1.FSTOCKORGID in ({Convert.ToString(customFilter["F_Filter_IsAllOrgID"])}) ";
					}
					else
					{
						where += $" and t1.FSTOCKORGID =0  ";
					}
				}
				else
				{
					where += $" and t1.FSTOCKORGID in ({Convert.ToString(customFilter["F_Filter_MulBaseOrgList"])}) ";
				}

				//物料Code
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MATERIALCode_Text"])))
				{
					if (Convert.ToInt32(customFilter["F_Filter_CB_MATERIALCode"]) == 1)
					{
						where += $" and t5.FNUMBER like '%{Convert.ToString(customFilter["F_Filter_MATERIALCode_Text"]).Trim()}%' ";
					}
					else
					{
						where += $" and t5.FNUMBER='{Convert.ToString(customFilter["F_Filter_MATERIALCode_Text"]).Trim()}' ";
					}

				}
				//物料名称
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MATERIALName"])))
				{
					if (Convert.ToInt32(customFilter["F_Filter_CB_MATERIALName"]) == 1)
					{
						where += $" and t6.FNAME like '%{Convert.ToString(customFilter["F_Filter_MATERIALName"]).Trim()}%' ";
					}
					else
					{
						where += $" and t6.FNAME = '{Convert.ToString(customFilter["F_Filter_MATERIALName"]).Trim()}' "; ;
					}
				}

				//入库订单开始日期
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StartDate"])))
				{
					where += $" and t1.FDATE>='{Convert.ToString(customFilter["F_Filter_StartDate"])}' ";
				}

				//入库订单结束日期
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_EndDate"])))
				{
					where += $" and t1.FDATE<'{DateTime.Parse(Convert.ToString(customFilter["F_Filter_EndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
				}

				//车间
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_WorkShopID"])))
				{
					where += $" and t13.FMASTERID='{Convert.ToString(customFilter["F_Filter_WorkShopID"])}' ";
				}
				string strWorkShop = GetWorkShopPermission(userinfo.FUSERID);
				//判断车间权限
				if (!string.IsNullOrWhiteSpace(strWorkShop))
				{
					DataRule dataRule = XmlStringToEntity<DataRule>(strWorkShop);
					if (dataRule != null)
					{
						if (!string.IsNullOrEmpty(dataRule.FilterSetting))
						{
							List<WorkShopInfo> workShop = Kingdee.Mymooo.Core.Utils.JsonConvertUtils.DeserializeObject<List<WorkShopInfo>>(dataRule.FilterSetting);
							if (workShop.Count > 0)
							{
								string str = string.Join("','", workShop.Select(x => x.Value).ToList());
								where += $" and t4.FNAME in ('{str}') ";
							}
						}
					}
				}

				#region 生产订单(非华东五部)

				//获取正常下推生产订单数据
				string sql = $@"/*dialect*/select t1.FBILLNO,t1.FSTOCKORGID,t3.FNAME FSTOCKORGNAME,t4.FNAME FWORKSHOPNAME,t1.FDATE,gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,
                t5.FNUMBER FMATERIALCODE,t6.FNAME FMATERIALNAME,t7.FNAME FUNITNAME,t10.FBILLNO FPROBILLNO,t9.FSEQ FPROSEQ,t9.FQTY FPLNQTY,t2.FREALQTY FINQTY,
                isnull(t9.FPENYPrice,0) FPRICE,convert(decimal(23,10),0) FProdValue,t12.FSALEORDERNO,t12.FSALEORDERENTRYSEQ,'2' FBILLTYPE,
				t10.FID,t9.FENTRYID,t15.FNAME FSALEORGNAME,convert(nvarchar(50),'') FDemandType,t16.FNUMBER FCustNo,t17.FSTOCKINQUAAUXQTY,t9.FPLANDEMANDQTY,
				t11.FSBILLID,'否' FIsSemiProduction
                into #AA
                from T_PRD_INSTOCK t1
                inner join T_PRD_INSTOCKENTRY t2 on t1.FID=t2.FID
                left join T_ORG_ORGANIZATIONS_L t3 on t1.FSTOCKORGID=t3.FORGID
                left join T_BD_DEPARTMENT t13 on t2.FWORKSHOPID=t13.FDEPTID
                left join T_BD_DEPARTMENT_L t4 on t13.FDEPTID=t4.FDEPTID
                left join T_BD_MATERIAL t5 on t5.FMATERIALID=t2.FMATERIALID
                left join T_BD_MATERIAL_L t6 on t5.FMATERIALID=t6.FMATERIALID and t6.FLOCALEID = 2052
                left join T_BD_UNIT_L t7 on t7.FUNITID=t2.FUNITID
                inner join T_PRD_INSTOCKENTRY_LK t8 on t8.FENTRYID=t2.FENTRYID
                inner join T_PRD_MOENTRY t9 on t9.FID=t8.FSBILLID and t9.FENTRYID=t8.FSID
                inner join T_PRD_MO t10 on t10.FID=t9.FID 
                inner join T_PRD_MOENTRY_LK t11 on t11.FENTRYID=t9.FENTRYID 
                inner join T_PLN_PLANORDER_B t12 on t12.FID=t11.FSBILLID 
                left join T_BD_MATERIALGROUP_L gl on t2.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                left join T_BD_MATERIALGROUP_L gll on t2.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
				left join T_SAL_ORDER t14 on t14.FID=t9.FSaleOrderId
				left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
				left join T_BD_CUSTOMER t16 on t14.FCUSTID=t16.FCUSTID
				left join T_PRD_MOENTRY_A t17 on t17.FID=t9.FID and t17.FENTRYID=t9.FENTRYID
                where t1.FSTOCKORGID <> 7401803 and t1.FDOCUMENTSTATUS='C' {where} ";

				//存在组织间需求单或者存在销售订单，并且是成品
				sql += @" and (
				                exists (
                                select top 1 p1.FID from T_PLN_REQUIREMENTORDER p1
                                inner join T_BD_MATERIAL p2 on p1.FMATERIALID=p2.FMATERIALID
                                where p1.FDOCUMENTSTATUS='C' and p1.FSALEORDERNO=t12.FSALEORDERNO and p1.FSALEORDERENTRYSEQ=t12.FSALEORDERENTRYSEQ and t5.FNUMBER=p2.FNUMBER
                                ) 
				                or
				                exists (
                                select top 1 so2.FENTRYID from T_SAL_ORDER  so1
				                inner join T_SAL_ORDERENTRY so2 on so1.FID=so2.FID
                                inner join T_BD_MATERIAL so3 on so3.FMATERIALID=so2.FMATERIALID
                                where so1.FDOCUMENTSTATUS='C' and so1.FBILLNO=t12.FSALEORDERNO and so2.FSEQ=t12.FSALEORDERENTRYSEQ and t5.FNUMBER=so3.FNUMBER
                                ) 
				            ) ";
				DBServiceHelper.Execute(this.Context, sql);

				//手工做的生产订单
				sql = $@"/*dialect*/select t1.FBILLNO,t1.FSTOCKORGID,t3.FNAME FSTOCKORGNAME,t4.FNAME FWORKSHOPNAME,t1.FDATE,gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,
                t5.FNUMBER FMATERIALCODE,t6.FNAME FMATERIALNAME,t7.FNAME FUNITNAME,t10.FBILLNO FPROBILLNO,t9.FSEQ FPROSEQ,t9.FQTY FPLNQTY,t2.FREALQTY FINQTY,
                isnull(t9.FPENYPrice,0) FPRICE,convert(decimal(23,10),0) FProdValue,'' FSALEORDERNO,0 FSALEORDERENTRYSEQ,'2' FBILLTYPE,
				t10.FID,t9.FENTRYID,t15.FNAME FSALEORGNAME,convert(nvarchar(50),'') FDemandType,'' FCustNo,t17.FSTOCKINQUAAUXQTY,t9.FPLANDEMANDQTY,
				t11.FSBILLID,'否' FIsSemiProduction
                into #BB
                from T_PRD_INSTOCK t1
                inner join T_PRD_INSTOCKENTRY t2 on t1.FID=t2.FID
                left join T_ORG_ORGANIZATIONS_L t3 on t1.FSTOCKORGID=t3.FORGID
                left join T_BD_DEPARTMENT t13 on t2.FWORKSHOPID=t13.FDEPTID
                left join T_BD_DEPARTMENT_L t4 on t13.FDEPTID=t4.FDEPTID
                left join T_BD_MATERIAL t5 on t5.FMATERIALID=t2.FMATERIALID
                left join T_BD_MATERIAL_L t6 on t5.FMATERIALID=t6.FMATERIALID and t6.FLOCALEID = 2052
                left join T_BD_UNIT_L t7 on t7.FUNITID=t2.FUNITID
                inner join T_PRD_INSTOCKENTRY_LK t8 on t8.FENTRYID=t2.FENTRYID
                inner join T_PRD_MOENTRY t9 on t9.FID=t8.FSBILLID and t9.FENTRYID=t8.FSID
                inner join T_PRD_MO t10 on t10.FID=t9.FID 
                left join T_PRD_MOENTRY_LK t11 on t11.FENTRYID=t9.FENTRYID 
                left join T_BD_MATERIALGROUP_L gl on t2.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                left join T_BD_MATERIALGROUP_L gll on t2.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
				left join T_SAL_ORDER t14 on t14.FID=t9.FSaleOrderId
				left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
				left join T_PRD_MOENTRY_A t17 on t17.FID=t9.FID and t17.FENTRYID=t9.FENTRYID
                where t1.FSTOCKORGID <> 7401803 and t1.FDOCUMENTSTATUS='C' {where} ";

				//手工创建
				sql += @" and (t11.FENTRYID is null) ";
				DBServiceHelper.Execute(this.Context, sql);

				//计划合并的生产订单
				sql = $@"/*dialect*/select t1.FBILLNO,t1.FSTOCKORGID,t3.FNAME FSTOCKORGNAME,t4.FNAME FWORKSHOPNAME,t1.FDATE,gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,
                t5.FNUMBER FMATERIALCODE,t6.FNAME FMATERIALNAME,t7.FNAME FUNITNAME,t10.FBILLNO FPROBILLNO,t9.FSEQ FPROSEQ,t9.FQTY FPLNQTY,t2.FREALQTY FINQTY,
                isnull(t9.FPENYPrice,0) FPRICE,convert(decimal(23,10),0) FProdValue,'' FSALEORDERNO,0 FSALEORDERENTRYSEQ,'2' FBILLTYPE,
				t10.FID,t9.FENTRYID,t15.FNAME FSALEORGNAME,convert(nvarchar(50),'') FDemandType,'' FCustNo,t17.FSTOCKINQUAAUXQTY,t9.FPLANDEMANDQTY,
				t11.FSBILLID,'否' FIsSemiProduction
                into #CC
                from T_PRD_INSTOCK t1
                inner join T_PRD_INSTOCKENTRY t2 on t1.FID=t2.FID
                left join T_ORG_ORGANIZATIONS_L t3 on t1.FSTOCKORGID=t3.FORGID
                left join T_BD_DEPARTMENT t13 on t2.FWORKSHOPID=t13.FDEPTID
                left join T_BD_DEPARTMENT_L t4 on t13.FDEPTID=t4.FDEPTID
                left join T_BD_MATERIAL t5 on t5.FMATERIALID=t2.FMATERIALID
                left join T_BD_MATERIAL_L t6 on t5.FMATERIALID=t6.FMATERIALID and t6.FLOCALEID = 2052
                left join T_BD_UNIT_L t7 on t7.FUNITID=t2.FUNITID
                inner join T_PRD_INSTOCKENTRY_LK t8 on t8.FENTRYID=t2.FENTRYID
                inner join T_PRD_MOENTRY t9 on t9.FID=t8.FSBILLID and t9.FENTRYID=t8.FSID
                inner join T_PRD_MO t10 on t10.FID=t9.FID 
                left join T_PRD_MOENTRY_LK t11 on t11.FENTRYID=t9.FENTRYID 
                left join T_BD_MATERIALGROUP_L gl on t2.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                left join T_BD_MATERIALGROUP_L gll on t2.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
				left join T_SAL_ORDER t14 on t14.FID=t9.FSaleOrderId
				left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
				left join T_PRD_MOENTRY_A t17 on t17.FID=t9.FID and t17.FENTRYID=t9.FENTRYID
                where t1.FSTOCKORGID <> 7401803 and t1.FDOCUMENTSTATUS='C' {where} ";

				//需要关联需求单或者销售订单才能排除子产品
				sql += @" and (
					          exists (
					          select top 1 p4.FID from T_PLN_PLANORDER_LK  p1
					          inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
					          inner join T_PLN_REQUIREMENTORDER p4 on p4.FSALEORDERNO=p3.FSALEORDERNO and p4.FSALEORDERENTRYSEQ=p3.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p5 on p5.FMATERIALID=p4.FMATERIALID
					          where p1.FID =t11.FSBILLID and p5.FNUMBER=t5.FNUMBER and p4.FDOCUMENTSTATUS='C' 
					          )
					          or
							  exists (
							  --拆分单(组织间需求单)
							  select top 1 1 from T_PLN_PLANORDER p7
							  inner join T_PLN_PLANORDER p2 on  (case when CHARINDEX('-',p7.FBILLNO)=0 then p7.FBILLNO else substring(p7.FBILLNO,0,CHARINDEX('-',p7.FBILLNO)) end)=p2.FBILLNO
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
							  inner join T_PLN_PLANORDER_LK p4 on  p4.FID=p3.FID
							  inner join T_PLN_PLANORDER_B  p5 on  p4.FSBILLID=p5.FID
					          inner join T_PLN_REQUIREMENTORDER p6 on p6.FSALEORDERNO=p5.FSALEORDERNO and p6.FSALEORDERENTRYSEQ=p5.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p8 on p8.FMATERIALID=p6.FMATERIALID
					          where p7.FDATASOURCE=3  and p6.FDOCUMENTSTATUS='C'  and p7.FID =t11.FSBILLID and p8.FNUMBER=t5.FNUMBER
					          )
					          or
					          exists (
					          select top 1 p4.FID from T_PLN_PLANORDER_LK  p1
					          inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
					          inner join T_SAL_ORDER  p4 on p4.FBILLNO=p3.FSALEORDERNO
					          inner join T_SAL_ORDERENTRY p5 on p4.FID=p5.FID and p5.FSEQ=p3.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p6 on p6.FMATERIALID=p5.FMATERIALID
					          where p1.FID =t11.FSBILLID and p6.FNUMBER=t5.FNUMBER and p4.FDOCUMENTSTATUS='C' 
					          )
							  or
							  exists (
							  --拆分单(销售订单)
							  select top 1 1 from T_PLN_PLANORDER p7
							  inner join T_PLN_PLANORDER p2 on  (case when CHARINDEX('-',p7.FBILLNO)=0 then p7.FBILLNO else substring(p7.FBILLNO,0,CHARINDEX('-',p7.FBILLNO)) end)=p2.FBILLNO
							  inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
							  inner join T_PLN_PLANORDER_LK p4 on  p4.FID=p3.FID
							  inner join T_PLN_PLANORDER_B  p5 on  p4.FSBILLID=p5.FID
							  inner join T_SAL_ORDER  p6 on p6.FBILLNO=p5.FSALEORDERNO
							  inner join T_SAL_ORDERENTRY p8 on p8.FID=p6.FID and p8.FSEQ=p5.FSALEORDERENTRYSEQ
							  inner join T_BD_MATERIAL p9 on p9.FMATERIALID=p8.FMATERIALID
							  where p7.FDATASOURCE=3  and p6.FDOCUMENTSTATUS='C'  and p7.FID =t11.FSBILLID and p9.FNUMBER=t5.FNUMBER
							  )
				            ) ";
				DBServiceHelper.Execute(this.Context, sql);

				#endregion

				#region 华东五部生产入库
				var addSelectSql = @" ";
				if ((Convert.ToBoolean(customFilter["F_Filter_IsAllOrg"]) && Convert.ToString(customFilter["F_Filter_IsAllOrgID"]).Contains("7401803")) || Convert.ToString(customFilter["F_Filter_MulBaseOrgList"]).Contains("7401803"))
				{
					addSelectSql = @" union all  select * from #DD ";
					sql = $@"/*dialect*/select t1.FBILLNO,t1.FSTOCKORGID,t3.FNAME FSTOCKORGNAME,t4.FNAME FWORKSHOPNAME,t1.FDATE,gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,
							t5.FNUMBER FMATERIALCODE,t6.FNAME FMATERIALNAME,t7.FNAME FUNITNAME,t10.FBILLNO FPROBILLNO,t9.FSEQ FPROSEQ,t9.FQTY FPLNQTY,t2.FREALQTY FINQTY,
							isnull(t9.FPENYPrice,0) FPRICE,convert(decimal(23,10),0) FProdValue,
							isnull(t12.FSALEORDERNO,'') FSALEORDERNO,isnull(t12.FSALEORDERENTRYSEQ,0) FSALEORDERENTRYSEQ,isnull(t12.FDEMANDTYPE,'0') FBILLTYPE,
							t10.FID,t9.FENTRYID,t15.FNAME FSALEORGNAME,convert(nvarchar(50),'') FDemandType,t16.FNUMBER FCustNo,t17.FSTOCKINQUAAUXQTY,t9.FPLANDEMANDQTY,
							t11.FSBILLID,'否' FIsSemiProduction
							into #DD
							from T_PRD_INSTOCK t1
							inner join T_PRD_INSTOCKENTRY t2 on t1.FID=t2.FID
							left join T_ORG_ORGANIZATIONS_L t3 on t1.FSTOCKORGID=t3.FORGID
							left join T_BD_DEPARTMENT t13 on t2.FWORKSHOPID=t13.FDEPTID
							left join T_BD_DEPARTMENT_L t4 on t13.FDEPTID=t4.FDEPTID
							left join T_BD_MATERIAL t5 on t5.FMATERIALID=t2.FMATERIALID
							left join T_BD_MATERIAL_L t6 on t5.FMATERIALID=t6.FMATERIALID and t6.FLOCALEID = 2052
							left join T_BD_UNIT_L t7 on t7.FUNITID=t2.FUNITID
							inner join T_PRD_INSTOCKENTRY_LK t8 on t8.FENTRYID=t2.FENTRYID
							inner join T_PRD_MOENTRY t9 on t9.FID=t8.FSBILLID and t9.FENTRYID=t8.FSID
							inner join T_PRD_MO t10 on t10.FID=t9.FID 
							left join T_PRD_MOENTRY_LK t11 on t11.FENTRYID=t9.FENTRYID
							left join T_PLN_PLANORDER_B t12 on t12.FID=t11.FSBILLID 
							left join T_BD_MATERIALGROUP_L gl on t2.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t2.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
							left join T_SAL_ORDER t14 on t14.FID=t9.FSaleOrderId
							left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
							left join T_BD_CUSTOMER t16 on t14.FCUSTID=t16.FCUSTID
							left join T_PRD_MOENTRY_A t17 on t17.FID=t9.FID and t17.FENTRYID=t9.FENTRYID
							where t1.FSTOCKORGID= 7401803 and t1.FDOCUMENTSTATUS='C'  {where} ";
					DBServiceHelper.Execute(this.Context, sql);
					//更新来源销售订单的价格（先取组织间需求单），存在销售订单号的数据
					sql = $@"/*dialect*/update #DD set FPRICE=isnull((select top 1 F_PENY_PRICE from T_PLN_REQUIREMENTORDER t1
                        inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
                        where t1.FDOCUMENTSTATUS='C' and t1.F_PENY_PRICE>0 and t1.FSALEORDERNO=#DD.FSALEORDERNO and t1.FSALEORDERENTRYSEQ=#DD.FSALEORDERENTRYSEQ
                        and t2.FNUMBER=case when CHARINDEX('-W-1-',#DD.FMATERIALCODE)=0 then #DD.FMATERIALCODE
						else substring(#DD.FMATERIALCODE,0,CHARINDEX('-W-1-',#DD.FMATERIALCODE)) end 
                        and t1.FSUPPLYORGID=#DD.FSTOCKORGID order by t1.FAPPROVEDATE desc),0) where FBILLTYPE='1' and isnull(FPRICE,0)=0 ";
					DBServiceHelper.Execute(this.Context, sql);

					//更新来源销售订单的价格（取不到则取华东五部的销售订单），存在销售订单号的数据
					sql = $@"/*dialect*/update #DD set FPRICE=isnull((select top 1 t3.FTAXPRICE from T_SAL_ORDER t1
						inner join T_SAL_ORDERENTRY t2 on t1.FID=t2.FID
						inner join T_SAL_ORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
                        where t1.FDOCUMENTSTATUS='C' and t3.FTAXPRICE>0  and t1.FBILLNO=#DD.FSALEORDERNO and t2.FSEQ=#DD.FSALEORDERENTRYSEQ
                        and t4.FNUMBER=case when CHARINDEX('-W-1-',#DD.FMATERIALCODE)=0 then #DD.FMATERIALCODE
				        else substring(#DD.FMATERIALCODE,0,CHARINDEX('-W-1-',#DD.FMATERIALCODE)) end 
                        and t1.FSaleOrgId=7401803 and t2.FSupplyTargetOrgId=#DD.FSTOCKORGID order by t1.FAuditTime desc),0)
						where FBILLTYPE='1' and isnull(FPRICE,0)=0 ";
					DBServiceHelper.Execute(this.Context, sql);

                    //更新是否半成品生产
                    sql = $@"/*dialect*/update t1 set FIsSemiProduction='是' from #DD t1 where  (
					          exists (
					          select top 1 p4.FID from T_PLN_PLANORDER_LK  p1
					          inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
					          inner join T_PLN_REQUIREMENTORDER p4 on p4.FSALEORDERNO=p3.FSALEORDERNO and p4.FSALEORDERENTRYSEQ=p3.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p5 on p5.FMATERIALID=p4.FMATERIALID
					          where p1.FID =t1.FSBILLID and p5.FNUMBER<>t1.FMATERIALCODE and p4.FDOCUMENTSTATUS='C' 
					          )
					          or
							  exists (
							  --拆分单(组织间需求单)
							  select top 1 1 from T_PLN_PLANORDER p7
							  inner join T_PLN_PLANORDER p2 on  (case when CHARINDEX('-',p7.FBILLNO)=0 then p7.FBILLNO else substring(p7.FBILLNO,0,CHARINDEX('-',p7.FBILLNO)) end)=p2.FBILLNO
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
							  inner join T_PLN_PLANORDER_LK p4 on  p4.FID=p3.FID
							  inner join T_PLN_PLANORDER_B  p5 on  p4.FSBILLID=p5.FID
					          inner join T_PLN_REQUIREMENTORDER p6 on p6.FSALEORDERNO=p5.FSALEORDERNO and p6.FSALEORDERENTRYSEQ=p5.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p8 on p8.FMATERIALID=p6.FMATERIALID
					          where p7.FDATASOURCE=3  and p6.FDOCUMENTSTATUS='C'  and p7.FID =t1.FSBILLID and p8.FNUMBER<>t1.FMATERIALCODE
					          )
					          or
					          exists (
					          select top 1 p4.FID from T_PLN_PLANORDER_LK  p1
					          inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
					          inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
					          inner join T_SAL_ORDER  p4 on p4.FBILLNO=p3.FSALEORDERNO
					          inner join T_SAL_ORDERENTRY p5 on p4.FID=p5.FID and p5.FSEQ=p3.FSALEORDERENTRYSEQ
					          inner join T_BD_MATERIAL p6 on p6.FMATERIALID=p5.FMATERIALID
					          where p1.FID =t1.FSBILLID and p6.FNUMBER<>t1.FMATERIALCODE and p4.FDOCUMENTSTATUS='C' 
					          )
							  or
							  exists (
							  --拆分单(销售订单)
							  select top 1 1 from T_PLN_PLANORDER p7
							  inner join T_PLN_PLANORDER p2 on  (case when CHARINDEX('-',p7.FBILLNO)=0 then p7.FBILLNO else substring(p7.FBILLNO,0,CHARINDEX('-',p7.FBILLNO)) end)=p2.FBILLNO
							  inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
							  inner join T_PLN_PLANORDER_LK p4 on  p4.FID=p3.FID
							  inner join T_PLN_PLANORDER_B  p5 on  p4.FSBILLID=p5.FID
							  inner join T_SAL_ORDER  p6 on p6.FBILLNO=p5.FSALEORDERNO
							  inner join T_SAL_ORDERENTRY p8 on p8.FID=p6.FID and p8.FSEQ=p5.FSALEORDERENTRYSEQ
							  inner join T_BD_MATERIAL p9 on p9.FMATERIALID=p8.FMATERIALID
							  where p7.FDATASOURCE=3  and p6.FDOCUMENTSTATUS='C'  and p7.FID =t1.FSBILLID and p9.FNUMBER<>t1.FMATERIALCODE
							  )
							  or 
							  exists (
							   select top 1 p1.FID from T_PLN_REQUIREMENTORDER p1
							   inner join T_BD_MATERIAL p2 on p1.FMATERIALID=p2.FMATERIALID
							   where p1.FDOCUMENTSTATUS='C' and t1.FSALEORDERENTRYSEQ>0 and p1.FSALEORDERNO=t1.FSALEORDERNO and p1.FSALEORDERENTRYSEQ=t1.FSALEORDERENTRYSEQ and t1.FMATERIALCODE<>p2.FNUMBER
							   ) 
							   or
							   exists (
							   select top 1 so2.FENTRYID from T_SAL_ORDER  so1
							   inner join T_SAL_ORDERENTRY so2 on so1.FID=so2.FID
							   inner join T_BD_MATERIAL so3 on so3.FMATERIALID=so2.FMATERIALID
							   where so1.FDOCUMENTSTATUS='C' and t1.FSALEORDERENTRYSEQ>0 and so1.FBILLNO=t1.FSALEORDERNO and so2.FSEQ=t1.FSALEORDERENTRYSEQ and t1.FMATERIALCODE<>so3.FNUMBER
							   ) 
				            )  ";
                    DBServiceHelper.Execute(this.Context, sql);
                }
				#endregion

				//组合4种订单数据
				sql = $@"/*dialect*/select {KSQL_SEQ},FBILLNO,FSTOCKORGID,FSTOCKORGNAME,FWORKSHOPNAME,FDATE,FTYPEDESC,FGROUPDESC,FMATERIALCODE,FMATERIALNAME,FUNITNAME,FPROBILLNO,FPROSEQ,
FSALEORDERNO,FSALEORDERENTRYSEQ,FBILLTYPE,FID,FENTRYID,FSALEORGNAME,FDemandType,FCustNo
,CONVERT(INT,FPlanDemandQty) FPlanDemandQty,CONVERT(INT,FPLNQTY) FPLNQTY,CONVERT(INT,FINQTY) FINQTY,CONVERT(INT,FSTOCKINQUAAUXQTY) FSTOCKINQUAAUXQTY
,convert(decimal(18,6),FPRICE) FPRICE,convert(decimal(18,2),FProdValue) FProdValue,FSBILLID,FIsSemiProduction into {tableName} from (
                        select * from #AA
                        union all
                        select * from #BB
                        union all
                        select * from #CC
                        {addSelectSql}
                        ) datas ";

				//排序
				string dataSort = Convert.ToString(filter.FilterParameter.SortString);
				if (dataSort != "")
				{
					sql = string.Format(sql, dataSort);
				}
				else
				{
					sql = string.Format(sql, " FDATE desc,FPROBILLNO,FPROSEQ ");
				}
				DBServiceHelper.Execute(this.Context, sql);

				//（1、先取组织间需求单）
				sql = $@"/*dialect*/update {tableName} set FPRICE=isnull((select top 1 F_PENY_PRICE from T_PLN_REQUIREMENTORDER t1
                        inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
                        where t1.FDOCUMENTSTATUS='C' and t1.F_PENY_PRICE>0 
                        and t2.FNUMBER=case when CHARINDEX('-W-1-',{tableName}.FMATERIALCODE)=0 then {tableName}.FMATERIALCODE
						else substring({tableName}.FMATERIALCODE,0,CHARINDEX('-W-1-',{tableName}.FMATERIALCODE)) end 
                        and t1.FSUPPLYORGID={tableName}.FSTOCKORGID order by t1.FAPPROVEDATE desc),0) where  isnull(FPRICE,0)=0";
				DBServiceHelper.Execute(this.Context, sql);

				//（2、取不到则取华东五部的销售订单）
				sql = $@"/*dialect*/update {tableName} set FPRICE=isnull((select top 1 t3.FTAXPRICE from T_SAL_ORDER t1
						inner join T_SAL_ORDERENTRY t2 on t1.FID=t2.FID
						inner join T_SAL_ORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
                        where t1.FDOCUMENTSTATUS='C' and t3.FTAXPRICE>0  
                        and t4.FNUMBER=case when CHARINDEX('-W-1-',{tableName}.FMATERIALCODE)=0 then {tableName}.FMATERIALCODE
				        else substring({tableName}.FMATERIALCODE,0,CHARINDEX('-W-1-',{tableName}.FMATERIALCODE)) end 
                        and t1.FSaleOrgId=7401803 and t2.FSupplyTargetOrgId={tableName}.FSTOCKORGID order by t1.FAuditTime desc),0)
						where isnull(FPRICE,0)=0  ";
				DBServiceHelper.Execute(this.Context, sql);

                //3、FB华东半成品取计划的单价(MES接口回来没价格)
                sql = $@"/*dialect*/update {tableName} set FPRICE=isnull((select top 1 FPENYPRICE from T_PLN_PLANORDER_B t1
                        inner join T_PLN_PLANORDER t2 on t1.FID=t2.FID
						inner join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
                        where t2.FDOCUMENTSTATUS='C' and t2.FPENYPRICE>0  and t2.FSUPPLYORGID=7401803
						and {tableName}.FSALEORDERNO=t1.FSALEORDERNO and {tableName}.FSALEORDERENTRYSEQ=t1.FSALEORDERENTRYSEQ
                        and t3.FNUMBER={tableName}.FMATERIALCODE
                        and t2.FSUPPLYORGID={tableName}.FSTOCKORGID order by t2.FPENYPRICE desc),0) where  isnull(FPRICE,0)=0  ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新产值
                sql = $@"/*dialect*/update {tableName} set FProdValue=isnull(FPRICE,0)*FINQTY ";
				DBServiceHelper.Execute(this.Context, sql);

                //更新合并计划的销售组织(存在2个销售组织的为空)
                sql = $@"/*dialect*/update t1 set t1.FSALEORGNAME=case when (select count(distinct t15.FNAME) from T_PRD_MOENTRY_LK t11 
									inner join  T_PLN_PLANORDER_LK  p1 on p1.FID =t11.FSBILLID
									inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
									inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
									left join T_SAL_ORDER t14 on t14.FID=p3.FSALEORDERID
									left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
									where t11.FENTRYID=t1.FENTRYID and p3.FSALEORDERID>0)=1 then (select top 1 t15.FNAME from T_PRD_MOENTRY_LK t11 
									inner join  T_PLN_PLANORDER_LK  p1 on p1.FID =t11.FSBILLID
									inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
									inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
									left join T_SAL_ORDER t14 on t14.FID=p3.FSALEORDERID
									left join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
									where t11.FENTRYID=t1.FENTRYID and p3.FSALEORDERID>0) else '' end from {tableName} t1
									where t1.FSALEORGNAME is null ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新需求来源，分开更新
                //1.存在需求单，非合并
                sql = $@"/*dialect*/update t1 set FDemandType='组织间需求单' from {tableName} t1 where isnull(FDemandType,'')='' and exists (
						  select top 1 p1.FID from T_PLN_REQUIREMENTORDER p1
						  where p1.FDOCUMENTSTATUS='C' and p1.FSALEORDERNO=t1.FSALEORDERNO and p1.FSALEORDERENTRYSEQ=t1.FSALEORDERENTRYSEQ and Isnull(p1.FSALEORDERNO,'')<>'' ) ";
                DBServiceHelper.Execute(this.Context, sql);

                //2.存在需求单，计划合并
                sql = $@"/*dialect*/update t1 set FDemandType='组织间需求单' from {tableName} t1 where isnull(FDemandType,'')='' and exists (
							select top 1 p4.FID from 
							T_PRD_MOENTRY_LK t2 
							inner join  T_PLN_PLANORDER_LK  p1 on p1.FID =t2.FSBILLID
							inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
							inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
							inner join T_PLN_REQUIREMENTORDER p4 on p4.FSALEORDERNO=p3.FSALEORDERNO and p4.FSALEORDERENTRYSEQ=p3.FSALEORDERENTRYSEQ
							where t2.FENTRYID=t1.FENTRYID and p4.FDOCUMENTSTATUS='C' and Isnull(p4.FSALEORDERNO,'')<>''
							) ";
                DBServiceHelper.Execute(this.Context, sql);

                //3.存在销售，非合并
                sql = $@"/*dialect*/update t1 set FDemandType='直接销售订单' from {tableName} t1 where isnull(FDemandType,'')='' and exists (
						  select top 1 so2.FENTRYID from T_SAL_ORDER  so1
						  inner join T_SAL_ORDERENTRY so2 on so1.FID=so2.FID	
						  where so1.FDOCUMENTSTATUS='C' and so1.FBILLNO=t1.FSALEORDERNO and so2.FSEQ=t1.FSALEORDERENTRYSEQ)";
                DBServiceHelper.Execute(this.Context, sql);

                //4.存在销售，计划合并
                sql = $@"/*dialect*/update t1 set FDemandType=case when (select count(distinct t15.FNAME) from T_PRD_MOENTRY_LK t2 
						   inner join  T_PLN_PLANORDER_LK  p1 on p1.FID =t2.FSBILLID
						   inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
						   inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
						   inner join T_SAL_ORDER t14 on t14.FID=p3.FSALEORDERID
						   inner join T_ORG_ORGANIZATIONS_L t15 on t14.FSALEORGID=t15.FORGID and t15.FLOCALEID=2052
						   where t2.FENTRYID=t1.FENTRYID and p3.FSALEORDERID>0)=1 then
						  '直接销售订单' else '手工新建' end from {tableName} t1 where isnull(FDemandType,'')='' and exists (
						  select top 1 p4.FID from T_PRD_MOENTRY_LK t2 
						  inner join  T_PLN_PLANORDER_LK  p1 on p1.FID =t2.FSBILLID
						  inner join T_PLN_PLANORDER p2 on  p1.FSBILLID=p2.FID
						  inner join T_PLN_PLANORDER_B p3 on  p2.FID=p3.FID
						  inner join T_SAL_ORDER  p4 on p4.FBILLNO=p3.FSALEORDERNO
						  where t2.FENTRYID=t1.FENTRYID  and p4.FDOCUMENTSTATUS='C' ) ";
                DBServiceHelper.Execute(this.Context, sql);

                //5.销售组织是深圳蚂蚁的，默认组织间需求单，因为历史数据的组织间需求单序号没带过来
                sql = $@"/*dialect*/update t1 set FDemandType='组织间需求单' from {tableName} t1 where isnull(FSALEORGNAME,'') like '%有限公司%' ";
                DBServiceHelper.Execute(this.Context, sql);

				//目前是华东五部
                sql = $@"/*dialect*/update t1 set FDemandType='直接销售订单' from {tableName} t1 where isnull(FSALEORGNAME,'') not like '%有限公司%' and isnull(FSALEORGNAME,'')<>'' ";
                DBServiceHelper.Execute(this.Context, sql);

                //6.其他手工
                sql = $@"/*dialect*/update t1 set FDemandType='手工新建' from {tableName} t1 where isnull(FDemandType,'')='' or isnull(FSALEORGNAME,'')='' ";
                DBServiceHelper.Execute(this.Context, sql);

                //1.新增客户编码
                sql = $@"/*dialect*/update t1 set FCustNo=t3.FNUMBER from {tableName} t1,T_SAL_ORDER t2,T_BD_CUSTOMER t3 where t1.FSALEORDERNO=t2.FBILLNO and t2.FCUSTID=t3.FCUSTID ";
                DBServiceHelper.Execute(this.Context, sql);
                cope.Complete();
			}
		}

		//设置汇总列信息
		public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
		{
			var result = base.GetSummaryColumnInfo(filter);
			result.Add(new SummaryField("FSTOCKORGNAME", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.COUNT));
			result.Add(new SummaryField("FPLNQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			result.Add(new SummaryField("FINQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			result.Add(new SummaryField("FProdValue", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			return result;
		}
		/// <summary>
		/// 获取车间权限
		/// </summary>
		/// <returns></returns>
		public string GetWorkShopPermission(long userId)
		{
			string sql = $@"select top 1 d1.FDETAILXML
			from t_sec_FuncPermission p1 --授权
			inner join t_sec_funcPermissionEntry p2 on p1.FItemID= p2.FItemID --授权明细
			inner join T_SEC_PERMISSIONITEM p3 on p2.FPermissionItemID=p3.FItemID --权限项
			inner join t_sec_role r1 on p1.FRoleID = r1.FRoleID --角色
			inner join t_sec_userrolemap uor on uor.FROLEID= p1.FRoleID --用户角色关系
			inner join T_SEC_USERORG uo on uo.FENTITYID=uor.FENTITYID --用户组织关系
			inner join t_sec_User u on uo.FUserID=u.FUserID --用户
			inner join t_meta_objectType o1 on p1.FObjectTypeID=o1.FID and fdevtype!=2
			inner join t_meta_subsystem s1 on o1.FSUBSYSID = s1.FID --子系统
			inner join t_sec_DataRule d1 on d1.FITEMID=FDataRule
			 where FDevType<>2 and o1.FID='PRD_INSTOCK' and p3.FNUMBER='BOS_VIEW' and p2.FPERMISSIONSTATUS='0' and u.FFORBIDSTATUS='A' 
			 and CONVERT(nvarchar(2048),d1.FDETAILXML) like '%FWorkShopId%'
			 and u.FUserID={userId}";
			return DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "");
		}

		[XmlRoot(ElementName = "DataRule")]
		public class DataRule
		{
			[XmlElement(ElementName = "Id")]
			public string Id { get; set; }

			[XmlElement(ElementName = "FilterSetting")]
			public string FilterSetting { get; set; }
		}
		public T XmlStringToEntity<T>(string xmlString)
		{
			var serializer = new XmlSerializer(typeof(T));
			var reader = new StringReader(xmlString);
			return (T)serializer.Deserialize(reader);
		}

		public class WorkShopInfo
		{
			public string Value { get; set; }
		}
	}
}
