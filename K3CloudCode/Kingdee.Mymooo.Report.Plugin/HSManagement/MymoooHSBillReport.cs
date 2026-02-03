using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.HS.App.Report;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.Report.Plugin.HSManagement
{
    [Description("核算单据数据查询报表"), HotUpdate]
	public class MymoooHSBillReport : HS_HSBillReport
	{
		public override ReportHeader GetReportHeaders(IRptParams filter)
		{
			var reportHeaders = base.GetReportHeaders(filter);

			reportHeaders.AddChild("FContactType", new LocaleValue("往来单位类型", base.Context.UserLocale.LCID));
			reportHeaders.AddChild("FContactName", new LocaleValue("往来单位", base.Context.UserLocale.LCID));

			return reportHeaders;
		}

		public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
		{
			base.BuilderReportSqlAndTempTable(filter, tableName);

			//填写往来单位类型和往来单位字段
			List<string> alterColumns = new List<string>()
			{
				$"/*dialect*/ alter table {tableName} add FContactType nvarchar(100)",
				$"/*dialect*/ alter table {tableName} add FContactName nvarchar(100)",
				$"/*dialect*/ create index #IDX_MymoooHSBillReport on {tableName}(FBillFromId,fbillidkey)"
			};

			DBUtils.ExecuteBatch(this.Context, alterColumns, 50);
			alterColumns = new List<string>()
			{
				$"/*dialect*/ update t set FContactType= '客户',FContactName = cl.FNAME from {tableName} t inner join T_SAL_OUTSTOCK u on t.fbillidkey = u.FID inner join T_BD_CUSTOMER_L cl on u.FCUSTOMERID = cl.FCUSTID where t.FBillFromId='SAL_OUTSTOCK'",
				$"/*dialect*/ update t set FContactType= '客户',FContactName = cl.FNAME from {tableName} t inner join T_SAL_RETURNSTOCK u on t.fbillidkey = u.FID inner join T_BD_CUSTOMER_L cl on u.FRETCUSTID = cl.FCUSTID where t.FBillFromId='SAL_RETURNSTOCK'",
				$"/*dialect*/ update t set FContactType= '供应商',FContactName = cl.FNAME from {tableName} t inner join t_STK_InStock u on t.fbillidkey = u.FID inner join T_BD_SUPPLIER_L cl on u.FSupplierId = cl.FSUPPLIERID where t.FBillFromId='STK_InStock'" ,
				$"/*dialect*/ update t set FContactType= '供应商',FContactName = cl.FNAME from {tableName} t inner join t_PUR_MRB u on t.fbillidkey = u.FID inner join T_BD_SUPPLIER_L cl on u.FSupplierId = cl.FSUPPLIERID where t.FBillFromId='PUR_MRB'" ,
				$"/*dialect*/ update t set FContactType= '组织机构',FContactName = ol.FNAME from {tableName} t inner join T_STK_STKTRANSFEROUT u on t.fbillidkey = u.FID inner join T_ORG_ORGANIZATIONS_L ol on u.FStockInOrgID = ol.FORGID where t.FBillFromId='STK_TRANSFEROUT'" ,
				$"/*dialect*/ update t set FContactType= '组织机构',FContactName = ol.FNAME from {tableName} t inner join T_STK_STKTRANSFERIN u on t.fbillidkey = u.FID inner join T_ORG_ORGANIZATIONS_L ol on u.FStockOutOrgID = ol.FORGID where t.FBillFromId='STK_TRANSFERIN' and u.FOBJECTTYPEID = 'STK_TRANSFERIN'" ,
				$"/*dialect*/ update t set FContactType= '组织机构',FContactName = ol.FNAME from {tableName} t inner join T_STK_STKTRANSFERIN u on t.fbillidkey = u.FID inner join T_ORG_ORGANIZATIONS_L ol on u.FStockOrgId = ol.FORGID where t.FBillFromId='STK_TransferDirect'  and t.FInOutIndexFlag = '0' and u.FOBJECTTYPEID = 'STK_TransferDirect'",
				$"/*dialect*/ update t set FContactType= '组织机构',FContactName = ol.FNAME from {tableName} t inner join T_STK_STKTRANSFERIN u on t.fbillidkey = u.FID inner join T_ORG_ORGANIZATIONS_L ol on u.FStockOutOrgID = ol.FORGID where t.FBillFromId='STK_TransferDirect'  and t.FInOutIndexFlag = '1' and u.FOBJECTTYPEID = 'STK_TransferDirect'"
			};
			DBUtils.ExecuteBatch(this.Context, alterColumns, 50);
		}
	}
}
