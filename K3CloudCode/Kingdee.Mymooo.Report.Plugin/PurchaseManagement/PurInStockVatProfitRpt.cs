using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.PurchaseManagement
{
	[Description("采购入库毛利统计表"), HotUpdate]
	public class PurInStockVatProfitRpt : SysReportBaseService
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
				reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["F_Filter_StartDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_StartDate"].ToString()).ToString("yyyy-MM-dd"));
				reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["F_Filter_EndDate"]) == "" ? "全部" : DateTime.Parse(customFilter["F_Filter_EndDate"].ToString()).ToString("yyyy-MM-dd"));
				reportTitles.AddTitle("F_PENY_SupplierName", Convert.ToString(customFilter["F_Filter_SupplierCode"]) == "" ? "全部" : ((DynamicObject)customFilter["F_Filter_SupplierCode"])["Name"].ToString());
				reportTitles.AddTitle("F_PENY_PurchaseOrgName", Convert.ToString(customFilter["F_Filter_PurchaseOrgCode"]) == "" ? "全部" : ((DynamicObject)customFilter["F_Filter_PurchaseOrgCode"])["Name"].ToString());
				reportTitles.AddTitle("F_PENY_MulBillType", GetBaseDataNameValue(Convert.ToString(customFilter["F_Filter_MulBillTypeList"])));
			}
			return reportTitles;
		}

		//获取生产组织
		private string GetBaseDataNameValue(string dyobj)
		{
			if (string.IsNullOrWhiteSpace(dyobj))
			{
				return "全部";
			}
			string sql = $@"/*dialect*/select STRING_AGG(FNAME, '、') from T_BAS_BILLTYPE_L 
					where FBILLTYPEID in ('{dyobj.Replace(",", "','")}') and FLOCALEID=2052 ";
			return DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "");
		}

		//创建临时报表
		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			DynamicObject customFilter = filter.FilterParameter.CustomFilter;

			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				string where = " where t2.FDOCUMENTSTATUS='C' ";

				//单据类型
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_MulBillTypeList"])))
				{
					where += $" and t10.FBILLTYPEID in ('{Convert.ToString(customFilter["F_Filter_MulBillTypeList"]).Replace(",", "','").Trim()}') ";
				}
				//供应商
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_SupplierCode"])))
				{
					where += $" and supp.FNUMBER='{Convert.ToString(((DynamicObject)customFilter["F_Filter_SupplierCode"])["Number"].ToString()).Trim()}' ";
				}

				//采购组织
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_PurchaseOrgCode"])))
				{
					where += $" and org.FNUMBER='{Convert.ToString(((DynamicObject)customFilter["F_Filter_PurchaseOrgCode"])["Number"].ToString()).Trim()}' ";
				}

				//入库订单开始日期
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_StartDate"])))
				{
					where += $" and t2.FDATE>='{Convert.ToString(customFilter["F_Filter_StartDate"])}' ";
				}

				//入库订单结束日期
				if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["F_Filter_EndDate"])))
				{
					where += $" and t2.FDATE<'{DateTime.Parse(Convert.ToString(customFilter["F_Filter_EndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
				}
				//采购入库单
				string sql = $@"/*dialect*/select t10.FNAME FBillType,t7.FBILLNO FPoNo,t8.FSEQ FPoSeq,supp.FNUMBER FSupplierCode,suppl.FNAME FSupplierName,mat.FNUMBER FMaterialCode,matl.FNAME FMaterialName,unl.FNAME FUNITNAME,
							gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,org.FNUMBER FPurchaseOrgCode,orgl.FNAME FPurchaseOrgName,
							t2.FBILLNO,t1.FSEQ,t2.FDATE,t1.FREALQTY FQTY,t9.FTAXPRICE,(t1.FREALQTY*t9.FTAXPRICE) FAllAmount,t8.FSoUnitPrice FSoTaxPrice,(t1.FREALQTY*t8.FSoUnitPrice) FSoAmount,
							(case when t8.FSoUnitPrice=0  then 0  else convert(decimal(18,2),(t8.FSoUnitPrice-t9.FTaxPrice)/t8.FSoUnitPrice*100) end) FVatProfitRate,t9.FTAXRATE FTaxRate,
							t8.FSoNo,t8.FSoSeq
						    into #AA
							from T_STK_INSTOCKENTRY t1
							inner join T_STK_INSTOCK t2 on t1.FID=t2.FID
							inner join T_STK_INSTOCKENTRY_LK t3 on t1.FENTRYID=t3.FENTRYID
							left join T_PUR_RECEIVEENTRY t4 on t3.FSBILLID=t4.FID and t3.FSID=t4.FENTRYID
							left join T_PUR_RECEIVEENTRY_LK t5 on t4.FENTRYID=t5.FENTRYID
							left join T_PUR_POORDERENTRY_R t6 on t5.FSBILLID=t6.FID and t5.FSID=t6.FENTRYID
							left join T_PUR_POORDER t7 on t6.FID=t7.FID
							left join T_PUR_POORDERENTRY t8 on t7.FID=t8.FID and t6.FENTRYID=t8.FENTRYID
							left join T_BD_MATERIAL mat on mat.FMATERIALID=t1.FMATERIALID
							left join T_BD_MATERIAL_L matl on mat.FMATERIALID=matl.FMATERIALID and matl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052  
							left join T_ORG_ORGANIZATIONS org on org.FORGID=t2.FPURCHASEORGID
							left join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID = 2052
							left join t_BD_Supplier supp on supp.FSUPPLIERID=t2.FSUPPLIERID
							left join T_BD_SUPPLIER_L suppl on supp.FSUPPLIERID=suppl.FSUPPLIERID
							left join T_BD_UNIT_L unl  on unl.FUNITID=t1.FUNITID
							left join T_STK_INSTOCKENTRY_F t9 on t1.FENTRYID=t9.FENTRYID
							left join T_BAS_BILLTYPE_L t10 on t2.FBILLTYPEID=t10.FBILLTYPEID and t10.FLOCALEID = 2052
                            {where} ";
				DBServiceHelper.Execute(this.Context, sql);

				//采购退料(收料通知单下推)
				 sql = $@"/*dialect*/select t10.FNAME FBillType,t7.FBILLNO FPoNo,t8.FSEQ FPoSeq,supp.FNUMBER FSupplierCode,suppl.FNAME FSupplierName,mat.FNUMBER FMaterialCode,matl.FNAME FMaterialName,unl.FNAME FUNITNAME,
							gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,org.FNUMBER FPurchaseOrgCode,orgl.FNAME FPurchaseOrgName,
							t2.FBILLNO,t1.FSEQ,t2.FDATE,t1.FRMREALQTY FQTY,t9.FTAXPRICE,(t1.FRMREALQTY*t9.FTAXPRICE) FAllAmount,0 FSoTaxPrice,0 FSoAmount,
							0 FVatProfitRate,t9.FTAXRATE FTaxRate,
							'' FSoNo,0 FSoSeq
							into #BB
							from T_PUR_MRBENTRY t1
							inner join t_PUR_MRB t2 on t1.FID=t2.FID
							left join T_PUR_MRBENTRY_LK t3 on t1.FENTRYID=t3.FENTRYID
							left join T_PUR_RECEIVEENTRY t4 on t3.FSBILLID=t4.FID and t3.FSID=t4.FENTRYID
							left join T_PUR_RECEIVEENTRY_LK t5 on t4.FENTRYID=t5.FENTRYID
							left join T_PUR_POORDERENTRY_R t6 on t5.FSBILLID=t6.FID and t5.FSID=t6.FENTRYID
							left join T_PUR_POORDER t7 on t6.FID=t7.FID
							left join T_PUR_POORDERENTRY t8 on t7.FID=t8.FID and t6.FENTRYID=t8.FENTRYID
							left join T_BD_MATERIAL mat on mat.FMATERIALID=t1.FMATERIALID
							left join T_BD_MATERIAL_L matl on mat.FMATERIALID=matl.FMATERIALID and matl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052  
							left join T_ORG_ORGANIZATIONS org on org.FORGID=t2.FPURCHASEORGID
							left join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID = 2052
							left join t_BD_Supplier supp on supp.FSUPPLIERID=t2.FSUPPLIERID
							left join T_BD_SUPPLIER_L suppl on supp.FSUPPLIERID=suppl.FSUPPLIERID
							left join T_BD_UNIT_L unl  on unl.FUNITID=t1.FUNITID
							left join T_PUR_MRBENTRY_F t9 on t1.FENTRYID=t9.FENTRYID
							left join T_BAS_BILLTYPE_L t10 on t2.FBILLTYPEID=t10.FBILLTYPEID and t10.FLOCALEID = 2052
                            {where} and t1.FSRCBILLTYPEID='PUR_ReceiveBill' ";
				DBServiceHelper.Execute(this.Context, sql);

				//采购退料(采购入库下推)
				sql = $@"/*dialect*/select t10.FNAME FBillType,t7.FBILLNO FPoNo,t8.FSEQ FPoSeq,supp.FNUMBER FSupplierCode,suppl.FNAME FSupplierName,mat.FNUMBER FMaterialCode,matl.FNAME FMaterialName,unl.FNAME FUNITNAME,
							gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,org.FNUMBER FPurchaseOrgCode,orgl.FNAME FPurchaseOrgName,
							t2.FBILLNO,t1.FSEQ,t2.FDATE,t1.FRMREALQTY FQTY,t9.FTAXPRICE,(t1.FRMREALQTY*t9.FTAXPRICE) FAllAmount,0 FSoTaxPrice,0 FSoAmount,
							0 FVatProfitRate,t9.FTAXRATE FTaxRate,
							'' FSoNo,0 FSoSeq
							into #CC
							from T_PUR_MRBENTRY t1
							inner join t_PUR_MRB t2 on t1.FID=t2.FID
							left join T_PUR_MRBENTRY_LK t3 on t1.FENTRYID=t3.FENTRYID
							left join T_STK_INSTOCKENTRY tt1 on  t3.FSBILLID=tt1.FID and t3.FSID=tt1.FENTRYID
							left join T_STK_INSTOCKENTRY_LK tt2 on  tt1.FENTRYID=tt2.FENTRYID
							left join T_PUR_RECEIVEENTRY t4 on tt2.FSBILLID=t4.FID and tt2.FSID=t4.FENTRYID
							left join T_PUR_RECEIVEENTRY_LK t5 on t4.FENTRYID=t5.FENTRYID
							left join T_PUR_POORDERENTRY_R t6 on t5.FSBILLID=t6.FID and t5.FSID=t6.FENTRYID
							left join T_PUR_POORDER t7 on t6.FID=t7.FID
							left join T_PUR_POORDERENTRY t8 on t7.FID=t8.FID and t6.FENTRYID=t8.FENTRYID
							left join T_BD_MATERIAL mat on mat.FMATERIALID=t1.FMATERIALID
							left join T_BD_MATERIAL_L matl on mat.FMATERIALID=matl.FMATERIALID and matl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052  
							left join T_ORG_ORGANIZATIONS org on org.FORGID=t2.FPURCHASEORGID
							left join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID = 2052
							left join t_BD_Supplier supp on supp.FSUPPLIERID=t2.FSUPPLIERID
							left join T_BD_SUPPLIER_L suppl on supp.FSUPPLIERID=suppl.FSUPPLIERID
							left join T_BD_UNIT_L unl  on unl.FUNITID=t1.FUNITID
							left join T_PUR_MRBENTRY_F t9 on t1.FENTRYID=t9.FENTRYID
							left join T_BAS_BILLTYPE_L t10 on t2.FBILLTYPEID=t10.FBILLTYPEID and t10.FLOCALEID = 2052
                            {where} and t1.FSRCBILLTYPEID='STK_InStock' ";
				DBServiceHelper.Execute(this.Context, sql);

				//采购退料(采购订单下推和手工)
				sql = $@"/*dialect*/select t10.FNAME FBillType,isnull(t7.FBILLNO,'') FPoNo,isnull(t8.FSEQ,0) FPoSeq,supp.FNUMBER FSupplierCode,suppl.FNAME FSupplierName,mat.FNUMBER FMaterialCode,matl.FNAME FMaterialName,unl.FNAME FUNITNAME,
							gl.FNAME FTYPEDESC,gll.FNAME FGROUPDESC,org.FNUMBER FPurchaseOrgCode,orgl.FNAME FPurchaseOrgName,
							t2.FBILLNO,t1.FSEQ,t2.FDATE,t1.FRMREALQTY FQTY,t9.FTAXPRICE,(t1.FRMREALQTY*t9.FTAXPRICE) FAllAmount,0 FSoTaxPrice,0 FSoAmount,
							0 FVatProfitRate,t9.FTAXRATE FTaxRate,
							'' FSoNo,0 FSoSeq
							into #DD
							from T_PUR_MRBENTRY t1
							inner join t_PUR_MRB t2 on t1.FID=t2.FID
							left join T_PUR_MRBENTRY_LK t3 on t1.FENTRYID=t3.FENTRYID
							left join T_PUR_POORDERENTRY_R t6 on t3.FSBILLID=t6.FID and t3.FSID=t6.FENTRYID
							left join T_PUR_POORDER t7 on t6.FID=t7.FID
							left join T_PUR_POORDERENTRY t8 on t7.FID=t8.FID and t6.FENTRYID=t8.FENTRYID
							left join T_BD_MATERIAL mat on mat.FMATERIALID=t1.FMATERIALID
							left join T_BD_MATERIAL_L matl on mat.FMATERIALID=matl.FMATERIALID and matl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
							left join T_BD_MATERIALGROUP_L gll on t1.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052  
							left join T_ORG_ORGANIZATIONS org on org.FORGID=t2.FPURCHASEORGID
							left join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID = 2052
							left join t_BD_Supplier supp on supp.FSUPPLIERID=t2.FSUPPLIERID
							left join T_BD_SUPPLIER_L suppl on supp.FSUPPLIERID=suppl.FSUPPLIERID
							left join T_BD_UNIT_L unl  on unl.FUNITID=t1.FUNITID
							left join T_PUR_MRBENTRY_F t9 on t1.FENTRYID=t9.FENTRYID
							left join T_BAS_BILLTYPE_L t10 on t2.FBILLTYPEID=t10.FBILLTYPEID and t10.FLOCALEID = 2052
                            {where} and (t1.FSRCBILLTYPEID='PUR_PurchaseOrder' or t1.FSRCBILLTYPEID='') ";
				DBServiceHelper.Execute(this.Context, sql);

				//组合4种订单数据
				sql = $@"/*dialect*/select {KSQL_SEQ},* into {tableName} from (
                        select * from #AA
                        union all
                        select * from #BB
                        union all
                        select * from #CC
                        union all
                        select * from #DD
                        ) datas ";
				//排序
				string dataSort = Convert.ToString(filter.FilterParameter.SortString);
				if (dataSort != "")
				{
					sql = string.Format(sql, dataSort);
				}
				else
				{
					sql = string.Format(sql, " FDATE desc ");
				}
				DBUtils.Execute(this.Context, sql);
				cope.Complete();
			}
		}

		//设置汇总列信息
		public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
		{
			var result = base.GetSummaryColumnInfo(filter);
			result.Add(new SummaryField("FPoNo", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.COUNT));
			result.Add(new SummaryField("FQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			result.Add(new SummaryField("FAllAmount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			result.Add(new SummaryField("FSoAmount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
			return result;
		}
	}
}
