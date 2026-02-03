using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Office.Excel;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.SaleOrderModel;
using mymooo.k3cloud.core.SqlSugarCore.Sales;

namespace mymooo.k3cloud.business.Services.Sales
{
    /// <summary>
    /// 销售服务
    /// </summary>
    /// <param name="kingdeeContent"></param>
    [AutoInject(InJectType.Scope)]
    public class SaleOrderService(KingdeeContent kingdeeContent, ExcelService excelService)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        private readonly ExcelService excelService = excelService;

        public byte[] GetSaleOrderExcel(List<long> EntryIds)
        {
            //var datas = _kingdeeContent.SqlSugar.Queryable<SALORDER>()
            //    .Includes(x => x.SaleOrderEntry)
            //    .Where(x => x.SaleOrderEntry.Any(y => EntryIds.Contains(y.Entryid)))
            //    .ToList();
            var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<SalOrderEntryListExport>
                (
                @"SELECT t2.FNUMBER AS MaterialNumber,t3.FNAME AS MaterialName,t3.FSPECIFICATION AS Specification
,unl.FNAME AS UnitName,r.FBASECANOUTQTY AS BaseCanOutQty,f.FTAXPRICE AS Taxprice
,d.FDELIVERYDATE AS Deliverydate,org.FNAME AS OrgName,ps.FNAME AS ParentSmallName,ss.FNAME AS SmallName
,t1.FCUSTITEMNO AS CustItemNo,t1.FCUSTITEMNAME AS CustItemName,t1.FNOTE AS Note
FROM dbo.T_SAL_ORDERENTRY t1
LEFT JOIN T_SAL_ORDERENTRY_F f ON t1.FENTRYID=f.FENTRYID
LEFT JOIN T_SAL_ORDERENTRY_D d ON t1.FENTRYID=d.FENTRYID
LEFT JOIN T_SAL_ORDERENTRY_R r ON t1.FENTRYID=r.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIAL_L t3 ON t2.FMATERIALID=t3.FMATERIALID
LEFT JOIN dbo.T_BD_UNIT_L unl ON t1.FUNITID=unl.FUNITID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L org ON t1.FSUPPLYTARGETORGID=org.FORGID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L ps ON t1.FPARENTSMALLID=ps.FID
LEFT JOIN dbo.T_BD_MATERIALGROUP_L ss ON t1.FSMALLID=ss.FID
WHERE t1.FENTRYID IN (@ids)", new { ids = EntryIds.ToArray() }
                )
                ;

            return excelService.Export(datas, "");
        }

        public ResponseMessage<string> GetSalesOrderCostPriceRedisCache()
        {
            ResponseMessage<string> response = new();

            var sSql = $@"SELECT * FROM (
SELECT * FROM (
SELECT *,'min' AS PriceType
,row_number()over(PARTITION BY MaterialNumber ORDER by SupplierUnitPrice) rn FROM (
--销售最小
select mat.FNUMBER MaterialNumber,matL.FNAME MaterialName,salf.FTAXPRICE SalTaxPrice,salord.FQTY SalQTY,
supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
salord.FSupplierUnitPrice SupplierUnitPrice,
sal.FBILLNO,'SaleOrder' AS OrderType,
mat.FProductId ProductId
from T_SAL_ORDERENTRY salord
inner join T_SAL_ORDER sal on sal.FID=salord.FID
inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID
left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052
where sal.FDOCUMENTSTATUS='C'  and sal.FCANCELSTATUS='A' AND salord.FSupplierUnitPrice>0
AND (sal.FCancelStatus<>'B' OR EXISTS(
SELECT top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO))
union ALL
--组织间需求单最小
select 
mat.FNUMBER MaterialNumber,matL.FNAME MaterialName,pln.F_PENY_Price SalTaxPrice,pln.FDemandQty SalQTY
,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
pln.FSupplierUnitPrice SupplierUnitPrice,
pln.FBILLNO FBILLNO,'PlnReqOrder' OrderType,
mat.FProductId ProductId
from T_PLN_REQUIREMENTORDER pln
inner join T_BD_MATERIAL mat on pln.FMATERIALID=mat.FMATERIALID
inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = 2052
left join t_BD_Supplier sup on sup.FSUPPLIERID=pln.FSupplierId
left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052
where pln.FDemandType in (0,8) and pln.FDocumentStatus='C' AND pln.FSupplierUnitPrice>0
--and pln.FIsClosed='A' 
union ALL
--老ERP最小
select  
sod.ITEM_NO AS MaterialNumber,sod.ITEM_DESC AS MaterialName,sod.VAT_PRICE AS SalTaxPrice,sod.QTY AS SalQTY
,sod.SupplierName,sod.SupplierCode,sod.SupplierUnitPrice,
sod.SO_NO AS FBILLNO,'SalesOrder' OrderType,
sod.ProductId
from M_SOD_DET sod
inner join M_SO_MSTR so on sod.New_SO_NO = so.New_SO_NO
where (so.STATUS!='C' or so.isExistsDn=1) AND sod.SupplierUnitPrice>0
)t) t WHERE t.rn=1
UNION ALL
SELECT * FROM (
SELECT *,'max' AS PriceType
,row_number()over(PARTITION BY MaterialNumber ORDER by SupplierUnitPrice DESC) rn FROM (
--销售最大
select mat.FNUMBER MaterialNumber,matL.FNAME MaterialName,salf.FTAXPRICE SalTaxPrice,salord.FQTY SalQTY,
supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
salord.FSupplierUnitPrice SupplierUnitPrice,
sal.FBILLNO,'SaleOrder' AS OrderType,
mat.FProductId ProductId
from T_SAL_ORDERENTRY salord
inner join T_SAL_ORDER sal on sal.FID=salord.FID
inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID
left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052
where sal.FDOCUMENTSTATUS='C'  and sal.FCANCELSTATUS='A' AND salord.FSupplierUnitPrice>0
AND (sal.FCancelStatus<>'B' OR EXISTS(
SELECT top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO))
union ALL
--组织间需求单最大
select 
mat.FNUMBER MaterialNumber,matL.FNAME MaterialName,pln.F_PENY_Price SalTaxPrice,pln.FDemandQty SalQTY
,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
pln.FSupplierUnitPrice SupplierUnitPrice,
pln.FBILLNO FBILLNO,'PlnReqOrder' OrderType,
mat.FProductId ProductId
from T_PLN_REQUIREMENTORDER pln
inner join T_BD_MATERIAL mat on pln.FMATERIALID=mat.FMATERIALID
inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = 2052
left join t_BD_Supplier sup on sup.FSUPPLIERID=pln.FSupplierId
left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052
where pln.FDemandType in (0,8) and pln.FDocumentStatus='C' AND pln.FSupplierUnitPrice>0
--and pln.FIsClosed='A' 
union ALL
--老ERP最大
select  
sod.ITEM_NO AS MaterialNumber,sod.ITEM_DESC AS MaterialName,sod.VAT_PRICE AS SalTaxPrice,sod.QTY AS SalQTY
,sod.SupplierName,sod.SupplierCode,sod.SupplierUnitPrice,
sod.SO_NO AS FBILLNO,'SalesOrder' OrderType,
sod.ProductId
from M_SOD_DET sod
inner join M_SO_MSTR so on sod.New_SO_NO = so.New_SO_NO
where (so.STATUS!='C' or so.isExistsDn=1) AND sod.SupplierUnitPrice>0
)t) t WHERE t.rn=1
) s
ORDER BY s.MaterialNumber
";
            var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<SalesOrderCostPrice>(sSql).ToList();
            foreach (var data in datas)
            {
                _kingdeeContent.RedisCache.HashSet(data);
            }

            response.Code = ResponseCode.Success;
            return response;
        }
    }

}
