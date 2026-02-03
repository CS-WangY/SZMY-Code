using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Model.BussinessModel.K3Cloud;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.PurchaseModel;
using mymooo.k3cloud.core.RedisCacheModel;
using SqlSugar;

namespace mymooo.k3cloud.business.Services.Purchase
{
    /// <summary>
    /// 销售服务
    /// </summary>
    /// <param name="kingdeeContent"></param>
    [AutoInject(InJectType.Scope)]
    public class PurchaseOrderService(KingdeeContent kingdeeContent)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;

        public ResponseMessage<string> GetPurchaseOrder(K3CloudRabbitMQMessage<POOrder, POOrderEntryItem> request)
        {
            ResponseMessage<string> response = new();
            if (request.Head != null)
            {
                var sDate = System.DateTime.Now.AddYears(-1);
                var eDate = System.DateTime.Now;
                var materiallist = string.Join(",", request.Head.POOrderEntry.Select(x => x.MaterialId_Id));
                //获取订单物料采购最低价
                var sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,row_number()over(PARTITION BY t1.FNUMBER ORDER by t2.FPRICE,t2.FTAXPRICE) rn
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >='{sDate}' AND t4.FAPPROVEDATE<='{eDate}'
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FMATERIALID IN ({materiallist})
) t WHERE t.rn=1";
                var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql).ToList();

                foreach (var data in datas)
                {
                    var productid = new ProductIdFullNumber()
                    {
                        ProductId = data.FProductId,
                        ProductNumber = data.ProductNumber,
                        HistoryDate = data.AuditTime,
                    };
                    if (data.Price == 0)
                    {
                        _kingdeeContent.RedisCache.HashDelete(data, "history");
                        _kingdeeContent.RedisCache.HashDelete(productid, "full");
                    }
                    else
                    {
                        _kingdeeContent.RedisCache.HashSet(data);
                        _kingdeeContent.RedisCache.HashSet(productid);
                    }

                }
                //获取订单物料采购最高价
                sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,row_number()over(PARTITION BY t1.FNUMBER ORDER by t2.FPRICE,t2.FTAXPRICE DESC) rn
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >='{sDate}' AND t4.FAPPROVEDATE<='{eDate}'
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FMATERIALID IN ({materiallist})
) t WHERE t.rn=1";
                datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];

                foreach (var data in datas)
                {
                    var productid = new ProductIdFullNumber()
                    {
                        ProductId = data.FProductId,
                        ProductNumber = data.ProductNumber,
                        HistoryDate = data.AuditTime,
                    };
                    if (data.Price == 0)
                    {
                        _kingdeeContent.RedisCache.HashDelete(data, "history");
                        _kingdeeContent.RedisCache.HashDelete(productid, "full");
                    }
                    else
                    {
                        _kingdeeContent.RedisCache.HashSet(data);
                        _kingdeeContent.RedisCache.HashSet(productid);
                    }

                }


                //获取订单物料简易型号采购最低价
                sSql = $@"SELECT * FROM 
(
SELECT t1.FSHORTNUMBER AS productNumber,t1.FNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,row_number()over(PARTITION BY t1.FSHORTNUMBER ORDER by t2.FPRICE,t2.FTAXPRICE) rn
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >='{sDate}' AND t4.FAPPROVEDATE<='{eDate}'
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FSHORTNUMBER <> ''
and t1.FMATERIALID IN ({materiallist})
) t WHERE t.rn=1";
                datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];

                foreach (var data in datas)
                {
                    var productid = new ProductIdShortNumber()
                    {
                        ProductId = data.FProductId,
                        ProductNumber = data.ProductNumber,
                        HistoryDate = data.AuditTime,
                    };
                    if (data.Price == 0)
                    {
                        _kingdeeContent.RedisCache.HashDelete(data, "history");
                        _kingdeeContent.RedisCache.HashDelete(productid, "short");
                    }
                    else
                    {
                        _kingdeeContent.RedisCache.HashSet(data);
                        _kingdeeContent.RedisCache.HashSet(productid);
                    }

                }

                //获取订单物料简易型号采购最高价
                sSql = $@"SELECT * FROM 
(
SELECT t1.FSHORTNUMBER AS productNumber,t1.FNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,row_number()over(PARTITION BY t1.FSHORTNUMBER ORDER by t2.FPRICE,t2.FTAXPRICE DESC) rn
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >='{sDate}' AND t4.FAPPROVEDATE<='{eDate}'
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FSHORTNUMBER <> ''
and t1.FMATERIALID IN ({materiallist})
) t WHERE t.rn=1";
                datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];

                foreach (var data in datas)
                {
                    var productid = new ProductIdShortNumber()
                    {
                        ProductId = data.FProductId,
                        ProductNumber = data.ProductNumber,
                        HistoryDate = data.AuditTime,
                    };
                    if (data.Price == 0)
                    {
                        _kingdeeContent.RedisCache.HashDelete(data, "history");
                        _kingdeeContent.RedisCache.HashDelete(productid, "short");
                    }
                    else
                    {
                        _kingdeeContent.RedisCache.HashSet(data);
                        _kingdeeContent.RedisCache.HashSet(productid);
                    }

                }
            }
            response.Code = ResponseCode.Success;
            return response;
        }

        public ResponseMessage<string> FullPurchaseOrderHistoryPrice()
        {
            ResponseMessage<string> response = new();
            //获取所有价格最低的物料记录
            var sSql = $@"SELECT * FROM (
SELECT m.FNUMBER AS productNumber,m.FSHORTNUMBER,ISNULL(t1.FPRICE,0) AS Price,t1.FTAXPRICE AS TaxPrice,m.FMATERIALID AS ProductId
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName,t2.FQTY AS Qty,t3.FBILLNO AS POOrderNo,t3.FAPPROVEDATE AuditTime,ISNULL(m.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,row_number()over(PARTITION BY m.FNUMBER ORDER by t1.FPRICE,t1.FTAXPRICE) rn
FROM T_PUR_POORDERENTRY_F t1
INNER JOIN T_PUR_POORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_BD_MATERIAL m ON t2.FMATERIALID=m.FMATERIALID
INNER JOIN T_PUR_POORDER t3 ON t1.FID=t3.FID
AND t3.FDOCUMENTSTATUS = 'C'
LEFT JOIN T_BD_SUPPLIER t5 ON t3.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t WHERE t.rn=1";
            var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql).ToList();
            foreach (var data in datas)
            {
                var productid = new ProductIdFullNumber()
                {
                    ProductId = data.FProductId,
                    ProductNumber = data.ProductNumber,
                    HistoryDate = data.AuditTime,
                };
                _kingdeeContent.RedisCache.HashSet(data);
                _kingdeeContent.RedisCache.HashSet(productid);
            }
            //获取所有价格最高的物料记录
            sSql = $@"SELECT * FROM (
SELECT m.FNUMBER AS productNumber,m.FSHORTNUMBER,ISNULL(t1.FPRICE,0) AS Price,t1.FTAXPRICE AS TaxPrice,m.FMATERIALID AS ProductId
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName,t2.FQTY AS Qty,t3.FBILLNO AS POOrderNo,t3.FAPPROVEDATE AuditTime,ISNULL(m.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,row_number()over(PARTITION BY m.FNUMBER ORDER by t1.FPRICE,t1.FTAXPRICE) rn
FROM T_PUR_POORDERENTRY_F t1
INNER JOIN T_PUR_POORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_BD_MATERIAL m ON t2.FMATERIALID=m.FMATERIALID
INNER JOIN T_PUR_POORDER t3 ON t1.FID=t3.FID
AND t3.FDOCUMENTSTATUS = 'C'
LEFT JOIN T_BD_SUPPLIER t5 ON t3.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t WHERE t.rn=1";
            datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];
            foreach (var data in datas)
            {
                var productid = new ProductIdFullNumber()
                {
                    ProductId = data.FProductId,
                    ProductNumber = data.ProductNumber,
                    HistoryDate = data.AuditTime,
                };
                _kingdeeContent.RedisCache.HashSet(data);
                _kingdeeContent.RedisCache.HashSet(productid);
            }
            //获取所有价格最低的简易型号记录
            sSql = $@"SELECT * FROM (
SELECT m.FSHORTNUMBER AS productNumber,m.FNUMBER,ISNULL(t1.FPRICE,0) AS Price,t1.FTAXPRICE AS TaxPrice,m.FMATERIALID AS ProductId
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName,t2.FQTY AS Qty,t3.FBILLNO AS POOrderNo,t3.FAPPROVEDATE AuditTime,ISNULL(m.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,row_number()over(PARTITION BY m.FSHORTNUMBER ORDER by t1.FPRICE,t1.FTAXPRICE) rn
FROM T_PUR_POORDERENTRY_F t1
INNER JOIN T_PUR_POORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_BD_MATERIAL m ON t2.FMATERIALID=m.FMATERIALID
INNER JOIN T_PUR_POORDER t3 ON t1.FID=t3.FID
AND t3.FDOCUMENTSTATUS = 'C'
LEFT JOIN T_BD_SUPPLIER t5 ON t3.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
WHERE m.FSHORTNUMBER<>''
) t WHERE t.rn=1";
            datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];
            foreach (var data in datas)
            {
                var productid = new ProductIdShortNumber()
                {
                    ProductId = data.FProductId,
                    ProductNumber = data.ProductNumber,
                    HistoryDate = data.AuditTime,
                };
                _kingdeeContent.RedisCache.HashSet(data);
                _kingdeeContent.RedisCache.HashSet(productid);
                //_kingdeeContent.RedisCache.HashDelete(productid, "short");
            }

            //获取所有价格最高的简易型号记录
            sSql = $@"SELECT * FROM (
SELECT m.FSHORTNUMBER AS productNumber,m.FNUMBER,ISNULL(t1.FPRICE,0) AS Price,t1.FTAXPRICE AS TaxPrice,m.FMATERIALID AS ProductId
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName,t2.FQTY AS Qty,t3.FBILLNO AS POOrderNo,t3.FAPPROVEDATE AuditTime,ISNULL(m.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,row_number()over(PARTITION BY m.FSHORTNUMBER ORDER by t1.FPRICE,t1.FTAXPRICE) rn
FROM T_PUR_POORDERENTRY_F t1
INNER JOIN T_PUR_POORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_BD_MATERIAL m ON t2.FMATERIALID=m.FMATERIALID
INNER JOIN T_PUR_POORDER t3 ON t1.FID=t3.FID
AND t3.FDOCUMENTSTATUS = 'C'
LEFT JOIN T_BD_SUPPLIER t5 ON t3.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
WHERE m.FSHORTNUMBER<>''
) t WHERE t.rn=1";
            datas = [.. _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql)];
            foreach (var data in datas)
            {
                var productid = new ProductIdShortNumber()
                {
                    ProductId = data.FProductId,
                    ProductNumber = data.ProductNumber,
                    HistoryDate = data.AuditTime,
                };
                _kingdeeContent.RedisCache.HashSet(data);
                _kingdeeContent.RedisCache.HashSet(productid);
                //_kingdeeContent.RedisCache.HashDelete(productid, "short");
            }

            response.Code = ResponseCode.Success;
            return response;
        }

        public void UpdateOldPurchaseHistoryPrice()
        {
            var sDate = System.DateTime.Now.AddYears(-1);
            var eDate = System.DateTime.Now;
            //根据所有产品ID更新
            var redisKeys = _kingdeeContent.RedisCache.HashGetKeys<ProductIdFullNumber>();
            foreach (var redisKey in redisKeys)
            {
                var redisEntrys = _kingdeeContent.RedisCache.HashGetEntrys<ProductIdFullNumber>(redisKey, "full-*");
                foreach (var redisEntry in redisEntrys)
                {
                    if (Convert.ToDateTime(redisEntry.Value) < sDate)
                    {
                        var materialnumber = Convert.ToString(redisEntry.Name).Replace("full-", "");
                        var sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,FIRST_VALUE(t1.FNUMBER) OVER (ORDER BY t2.FPRICE ASC) AS LeastPrice
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >=@sDate AND t4.FAPPROVEDATE<=@eDate
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FNUMBER IN (@materialnumber)
) t
ORDER BY CASE WHEN t.POOrderNo IS NULL THEN 1 ELSE 0 END";
                        var data = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql,
                            new
                            {
                                sDate,
                                eDate,
                                materialnumber = new string[] { materialnumber },
                            }).FirstOrDefault();
                        var productid = new ProductIdFullNumber()
                        {
                            ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdFullNumber, long>(redisKey, p => p.ProductId),
                            ProductNumber = materialnumber
                        };
                        if (data == null || data.Price == 0)
                        {
                            var histdata = new PurchaseHistoryPrice()
                            {
                                ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdFullNumber, long>(redisKey, p => p.ProductId),
                                ProductNumber = materialnumber
                            };
                            _kingdeeContent.RedisCache.HashDelete(histdata, "history");
                            _kingdeeContent.RedisCache.HashDelete(productid, "full");
                        }
                        else
                        {

                            _kingdeeContent.RedisCache.HashSet(data);
                            productid.HistoryDate = data.AuditTime;
                            _kingdeeContent.RedisCache.HashDelete(productid);
                        }

                        sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,FIRST_VALUE(t1.FNUMBER) OVER (ORDER BY t2.FPRICE DESC) AS LeastPrice
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >=@sDate AND t4.FAPPROVEDATE<=@eDate
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FNUMBER IN (@materialnumber)
) t
ORDER BY CASE WHEN t.POOrderNo IS NULL THEN 1 ELSE 0 END";
                        data = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql,
                            new
                            {
                                sDate,
                                eDate,
                                materialnumber = new string[] { materialnumber },
                            }).FirstOrDefault();
                        productid = new ProductIdFullNumber()
                        {
                            ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdFullNumber, long>(redisKey, p => p.ProductId),
                            ProductNumber = materialnumber
                        };
                        if (data == null || data.Price == 0)
                        {
                            var histdata = new PurchaseHistoryPrice()
                            {
                                ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdFullNumber, long>(redisKey, p => p.ProductId),
                                ProductNumber = materialnumber
                            };
                            _kingdeeContent.RedisCache.HashDelete(histdata, "history");
                            _kingdeeContent.RedisCache.HashDelete(productid, "full");
                        }
                        else
                        {

                            _kingdeeContent.RedisCache.HashSet(data);
                            productid.HistoryDate = data.AuditTime;
                            _kingdeeContent.RedisCache.HashDelete(productid);
                        }
                    }
                }
            }
            //根据所有简易型号更新
            redisKeys = _kingdeeContent.RedisCache.HashGetKeys<ProductIdShortNumber>();
            foreach (var redisKey in redisKeys)
            {
                var redisEntrys = _kingdeeContent.RedisCache.HashGetEntrys<ProductIdShortNumber>(redisKey, "short-*");
                foreach (var redisEntry in redisEntrys)
                {
                    if (Convert.ToDateTime(redisEntry.Value) < sDate)
                    {
                        var materialnumber = Convert.ToString(redisEntry.Name).Replace("short-", "");
                        var sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'minpoprice' AS type,FIRST_VALUE(t1.FNUMBER) OVER (ORDER BY t2.FPRICE ASC) AS LeastPrice
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >=@sDate AND t4.FAPPROVEDATE<=@eDate
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FNUMBER IN (@materialnumber)
) t
ORDER BY CASE WHEN t.POOrderNo IS NULL THEN 1 ELSE 0 END";
                        var data = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql,
                            new
                            {
                                sDate,
                                eDate,
                                materialnumber = new string[] { materialnumber },
                            }).FirstOrDefault();
                        var productid = new ProductIdShortNumber()
                        {
                            ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdShortNumber, long>(redisKey, p => p.ProductId),
                            ProductNumber = materialnumber
                        };
                        if (data == null || data.Price == 0)
                        {
                            var histdata = new PurchaseHistoryPrice()
                            {
                                ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdShortNumber, long>(redisKey, p => p.ProductId),
                                ProductNumber = materialnumber
                            };
                            _kingdeeContent.RedisCache.HashDelete(histdata, "history");
                            _kingdeeContent.RedisCache.HashDelete(productid, "short");
                        }
                        else
                        {

                            _kingdeeContent.RedisCache.HashSet(data);
                            productid.HistoryDate = data.AuditTime;
                            _kingdeeContent.RedisCache.HashDelete(productid);
                        }

                        sSql = $@"SELECT * FROM 
(
SELECT t1.FNUMBER AS productNumber,t1.FSHORTNUMBER,ISNULL(t2.FPRICE,0) AS Price,t2.FTAXPRICE AS TaxPrice,t1.FMATERIALID AS ProductId
,SupplierCode,SupplierName,t2.FQTY AS Qty,t2.FBILLNO AS POOrderNo,t2.FAPPROVEDATE AuditTime,ISNULL(t1.FPRODUCTID,0) AS FProductId
,'maxpoprice' AS type,FIRST_VALUE(t1.FNUMBER) OVER (ORDER BY t2.FPRICE ASC) AS LeastPrice
FROM T_BD_MATERIAL t1
LEFT JOIN
(
SELECT t2.FMATERIALID,t2.FQTY,t3.FPRICE,t3.FTAXPRICE,t4.FBILLNO,t4.FAPPROVEDATE
,t5.FNUMBER AS SupplierCode,t6.FNAME AS SupplierName
FROM T_PUR_POORDERENTRY t2
INNER JOIN T_PUR_POORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
INNER JOIN dbo.T_PUR_POORDER t4 ON t4.FID = t2.FID
AND t4.FDOCUMENTSTATUS = 'C'
AND t4.FAPPROVEDATE >=@sDate AND t4.FAPPROVEDATE<=@eDate
LEFT JOIN T_BD_SUPPLIER t5 ON t4.FSUPPLIERID=t5.FSUPPLIERID
LEFT JOIN T_BD_SUPPLIER_L t6 ON t5.FSUPPLIERID=t6.FSUPPLIERID
) t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FNUMBER IN (@materialnumber)
) t
ORDER BY CASE WHEN t.POOrderNo IS NULL THEN 1 ELSE 0 END";
                        data = _kingdeeContent.SqlSugar.Ado.SqlQuery<PurchaseHistoryPrice>(sSql,
                            new
                            {
                                sDate,
                                eDate,
                                materialnumber = new string[] { materialnumber },
                            }).FirstOrDefault();
                        productid = new ProductIdShortNumber()
                        {
                            ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdShortNumber, long>(redisKey, p => p.ProductId),
                            ProductNumber = materialnumber
                        };
                        if (data == null || data.Price == 0)
                        {
                            var histdata = new PurchaseHistoryPrice()
                            {
                                ProductId = _kingdeeContent.RedisCache.HashGet<ProductIdShortNumber, long>(redisKey, p => p.ProductId),
                                ProductNumber = materialnumber
                            };
                            _kingdeeContent.RedisCache.HashDelete(histdata, "history");
                            _kingdeeContent.RedisCache.HashDelete(productid, "short");
                        }
                        else
                        {

                            _kingdeeContent.RedisCache.HashSet(data);
                            productid.HistoryDate = data.AuditTime;
                            _kingdeeContent.RedisCache.HashDelete(productid);
                        }
                    }
                }
            }
        }
    }
}
