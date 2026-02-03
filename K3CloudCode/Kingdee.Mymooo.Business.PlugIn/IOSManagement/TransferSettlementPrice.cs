using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.IOS;
using Kingdee.K3.SCM.App.IOS.Core;
using Kingdee.K3.SCM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.IOSManagement
{
	[Description("组织间结算获取价格插件"), HotUpdate]
	public class TransferSettlementPrice : AbstractSettlementPrice
	{
		private readonly Dictionary<string, string> _FormMetadatas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public TransferSettlementPrice()
		{
			_FormMetadatas["STK_TRANSFERIN"] = "T_STK_STKTRANSFERINENTRY1";
			_FormMetadatas["STK_TRANSFEROUT"] = "T_STK_STKTRANSFEROUTENTRY";
		}

		public override SettlementPrice ReturnSettlementPrice(Context ctx, SettlementInfo settlementInfo)
		{
			var sql = @"/*dialect*/
with salesorder as
(
	select FSID,FSTABLENAME,FTTABLENAME,FTID
	from T_BF_INSTANCEENTRY where FTTABLENAME = @FTTABLENAME and FTID = @FEntryId
	union all
	select t.FSID,t.FSTABLENAME,s.FTTABLENAME,s.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FSTABLENAME = t.FTTABLENAME and s.FSID = t.FTID
)
select ef.FTAXPRICE,ef.FPRICE,ef.FTAXRATE,ms.FAgentSalReduceRate
from salesorder o
	inner join T_SAL_ORDERENTRY_F ef on o.FSID = ef.FENTRYID
	inner join T_SAL_ORDERENTRY e on ef.FENTRYID = e.FENTRYID
	inner join T_BD_MATERIALSALE ms on e.FMATERIALID = ms.FMATERIALID
where FSTABLENAME = 'T_SAL_ORDERENTRY'";

			SqlParam[] sqlParams = new SqlParam[]
			{
				new SqlParam("@FTTABLENAME", KDDbType.String, _FormMetadatas[settlementInfo.FBizFormId]),
				new SqlParam("@FEntryId", KDDbType.Int64, settlementInfo.FBizEntryId)
			};
			var entryData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
			SettlementPrice settlementPrice = new SettlementPrice();
			settlementPrice.FCurrencyId = settlementInfo.FBizCurrencyId;
			settlementPrice.FPriceUnitID = settlementInfo.FUnitID;
			settlementPrice.FPriceQty = settlementInfo.FQty;
			settlementPrice.FIsIncludedTax = true;
			if (entryData != null)
			{
				settlementPrice.FPrice = entryData.GetValue<decimal>("FPRICE", 0) * (1 - entryData.GetValue<decimal>("FAgentSalReduceRate", 0) / 100);
				settlementPrice.FTaxPrice = entryData.GetValue<decimal>("FTAXPRICE", 0) * (1 - entryData.GetValue<decimal>("FAgentSalReduceRate", 0) / 100);
				settlementPrice.FTaxRate = entryData.GetValue<decimal>("FTAXRATE", 0);
			}
			else
			{
				//如果找不到销售订单,那么从组织间需求单上找,判断需求来源是否为销售订单,如果是,那么也是取销售订单的单价*0.9
				sql = @"/*dialect*/
with salesorder as
(
	select FSID,FSTABLENAME,FTTABLENAME,FTID
	from T_BF_INSTANCEENTRY where FTTABLENAME = @FTTABLENAME and FTID = @FEntryId
	union all
	select t.FSID,t.FSTABLENAME,s.FTTABLENAME,s.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FSTABLENAME = t.FTTABLENAME and s.FSID = t.FTID
)
select ef.FTAXPRICE,ef.FPRICE,ef.FTAXRATE,p.FDEMANDTYPE,ms.FAgentSalReduceRate
,p.FID AS PLNID,e.FMATERIALID as SalMaterial,p.FMATERIALID as PlnMaterial
from salesorder o
	inner join T_PLN_REQUIREMENTORDER p on o.FSID = p.FID
	left join T_SAL_ORDERENTRY_F ef on p.FSaleOrderEntryId = ef.FENTRYID and p.FDEMANDTYPE = '1'
	left join T_SAL_ORDERENTRY e on ef.FENTRYID = e.FENTRYID
	left join T_BD_MATERIALSALE ms on e.FMATERIALID = ms.FMATERIALID
where o.FSTABLENAME = 'T_PLN_REQUIREMENTORDER'";
				entryData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
				if (entryData != null && Convert.ToString(entryData["FDEMANDTYPE"]) == "1"
					&& Convert.ToString(entryData["SalMaterial"]) == Convert.ToString(entryData["PlnMaterial"]))
				{
					settlementPrice.FPrice = entryData.GetValue<decimal>("FPRICE", 0) * (1 - entryData.GetValue<decimal>("FAgentSalReduceRate", 0) / 100);
					settlementPrice.FTaxPrice = entryData.GetValue<decimal>("FTAXPRICE", 0) * (1 - entryData.GetValue<decimal>("FAgentSalReduceRate", 0) / 100);
					settlementPrice.FTaxRate = entryData.GetValue<decimal>("FTAXRATE", 0);
				}
				else if (entryData != null && Convert.ToString(entryData["SalMaterial"]) != Convert.ToString(entryData["PlnMaterial"]))
				{
					sql = @"/*dialect*/SELECT F_PENY_PRICE,F_PENY_AMOUNT,FSUPPLYORGID,t2.FTAXRATE FROM T_PLN_REQUIREMENTORDER tp
LEFT JOIN (
SELECT t2.FUSEORGID,t2.FTAXRULEID,t1.FVALUERANGEKEY,t2.FNUMBER,t3.FTAXRATE FROM T_BD_TAXRULECONDITION t1
INNER JOIN T_BD_TAXRULE t2 ON t1.FTAXRULEID=t2.FTAXRULEID
LEFT JOIN T_BD_TAXRATE t3 ON t2.FDEFAULTTAXMIXID=t3.FID
WHERE EXISTS (
    SELECT 1
    FROM T_META_OBJECTTYPE t2
	LEFT JOIN T_META_OBJECTTYPE_L t3 ON t2.FID=t3.FID AND t3.FLOCALEID=2052 
    WHERE t1.FVALUERANGEKEY LIKE '%' + t2.FID + '%'
	AND t2.FBASEOBJECTID='IOS_Settlement'
)
) t2 ON tp.FSUPPLYORGID=t2.FUSEORGID
                        WHERE FID=@FID";
					sqlParams = new SqlParam[]
					{
					new SqlParam("@FID", KDDbType.Int64, Convert.ToInt64(entryData["PLNID"]))
					};
					var entrydetails = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
					if (entrydetails != null)
					{
						settlementPrice.FPrice = entrydetails.GetValue<decimal>("F_PENY_PRICE", 0);
						settlementPrice.FTaxPrice = entrydetails.GetValue<decimal>("F_PENY_PRICE", 0);
						settlementPrice.FTaxRate = entrydetails.GetValue<decimal>("FTAXRATE", 0);
					}
				}
				else
				{
					//如果查不到对应的销售订单,那么就是内部的调拨,从组织间价目表中获取单价
					switch (settlementInfo.FBizFormId)
					{
						case "STK_TRANSFEROUT":
							sql = @"SELECT t3.FTaxPrice,t3.FPRICE,t3.FTAXRATE FROM T_STK_STKTRANSFEROUTENTRY t1 
INNER JOIN T_IOS_PRICELIST t2 ON t1.FOWNERINID=t2.FCREATEORGID AND t1.FOWNERID=t2.F_PENY_SUPPLYORGID
INNER JOIN T_IOS_PRICELISTENTRY t3 ON t2.FID=t3.FID
INNER JOIN T_BD_MATERIAL t4 ON t3.FMATERIALID=t4.FMATERIALID
WHERE t1.FENTRYID=@FENTRYID AND t4.FMASTERID=@FMASTERID";
							sqlParams = new SqlParam[]
							{
					new SqlParam("@FENTRYID", KDDbType.Int64, settlementInfo.FBizEntryId),
					new SqlParam("@FMASTERID", KDDbType.Int64, settlementInfo.FMasterId)
							};
							entryData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
							if (entryData != null)
							{
								settlementPrice.FPrice = entryData.GetValue<decimal>("FPRICE", 0);
								settlementPrice.FTaxPrice = entryData.GetValue<decimal>("FTaxPrice", 0);
								settlementPrice.FTaxRate = entryData.GetValue<decimal>("FTAXRATE", 0);
							}
							break;
						case "STK_TRANSFERIN":
							sql = @"SELECT t3.FTaxPrice,t3.FPRICE,t3.FTAXRATE FROM T_STK_STKTRANSFEROUTENTRY t1 
INNER JOIN T_IOS_PRICELIST t2 ON t1.FOWNERINID=t2.FCREATEORGID AND t1.FOWNERID=t2.F_PENY_SUPPLYORGID
INNER JOIN T_IOS_PRICELISTENTRY t3 ON t2.FID=t3.FID
INNER JOIN T_BD_MATERIAL t4 ON t3.FMATERIALID=t4.FMATERIALID
WHERE t1.FENTRYID=@FENTRYID AND t4.FMASTERID=@FMASTERID";
							sqlParams = new SqlParam[]
							{
					new SqlParam("@FENTRYID", KDDbType.Int64, settlementInfo.FBizEntryId),
					new SqlParam("@FMASTERID", KDDbType.Int64, settlementInfo.FMasterId)
							};
							entryData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
							if (entryData != null)
							{
								settlementPrice.FPrice = entryData.GetValue<decimal>("FPRICE", 0);
								settlementPrice.FTaxPrice = entryData.GetValue<decimal>("FTaxPrice", 0);
								settlementPrice.FTaxRate = entryData.GetValue<decimal>("FTAXRATE", 0);
							}
							break;
					}
					//                    sql = @"select e.FTaxPrice,e.FPRICE,e.FTAXRATE
					//from T_IOS_PRICELIST o
					//	inner join T_IOS_PRICELISTENTRY e on o.FID = e.FID
					//	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID 
					//where o.F_PENY_SupplyOrgId = @FSupplyOrgId or o.FCreateOrgId = @FSupplyOrgId and m.FMASTERID = @FMASTERID";
					//                    sqlParams = new SqlParam[]
					//                    {
					//                    new SqlParam("@FSupplyOrgId", KDDbType.Int64, settlementInfo.FAcctOrgId),
					//                    new SqlParam("@FMASTERID", KDDbType.Int64, settlementInfo.FMasterId)
					//                    };
					//                    entryData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams).FirstOrDefault();
					//                    if (entryData != null)
					//                    {
					//                        settlementPrice.FPrice = entryData.GetValue<decimal>("FPRICE", 0);
					//                        settlementPrice.FTaxPrice = entryData.GetValue<decimal>("FTaxPrice", 0);
					//                        settlementPrice.FTaxRate = entryData.GetValue<decimal>("FTAXRATE", 0);
					//                    }
				}
			}

			return settlementPrice;
		}
	}
}
