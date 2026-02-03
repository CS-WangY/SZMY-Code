using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Model.SqlSugarCore;
using mymooo.k3cloud.core.Inventory;
using mymooo.k3cloud.core.Stock;
using SqlSugar;
using StackExchange.Redis;
using System.Collections.Generic;

namespace mymooo.k3cloud.business.Services
{
    [AutoInject(InJectType.Single)]
    public class InventoryService(RedisCache redisCache)
    {
        private readonly RedisCache _redisCache = redisCache;

        public void ReloadCahce(MymoooSqlSugar sqlSugar)
        {
            _redisCache.HashDelete<InventoryInfo>();
            ReloadStockCahce(sqlSugar);
            ReloadShipdSumQtyCahce(sqlSugar);
            ReloadOnOrderQtyCahce(sqlSugar);
            ReloadInspQtyCahce(sqlSugar);
        }

        public void ReloadLockQtyCache(MymoooSqlSugar sqlSugar)
        {
            var sql = @"SELECT m.FNUMBER MaterialNumber,o.FNUMBER StockOrgNumber,s.FNUMBER StockNumber,sum(t1.FBASEQTY) LockQty
FROM T_PLN_RESERVELINKENTRY t1
	INNER JOIN T_BD_MATERIAL m on t1.FMATERIALID = m.FMATERIALID
	inner join T_ORG_ORGANIZATIONS o on t1.FSUPPLYORGID = o.FORGID
	inner join T_BD_STOCK s on t1.FSTOCKID = s.FSTOCKID
WHERE t1.FSUPPLYFORMID='STK_Inventory'
group by m.FNUMBER,o.FNUMBER,s.FNUMBER";
            var stocks = sqlSugar.Ado.SqlQuery<InventoryInfo>(sql).ToArray();
            var keys = _redisCache.HashGetKeys<InventoryInfo>();
            foreach (var key in keys)
            {
                var materialNumber = _redisCache.HashGet<InventoryInfo, string>(key, p => p.MaterialNumber);
                RedisValue[] redisFields = _redisCache.HashGetEntrys<InventoryInfo>(key, "lockqty-*").Select(p => p.Name).ToArray();
                _redisCache.HashDelete<InventoryInfo>(key, redisFields);
                foreach (var stock in stocks.Where(p => p.MaterialNumber == materialNumber))
                {
                    _redisCache.HashSet(stock, p => p.LockQty);
                }
            }
        }

        public ResponseMessage<List<InventoryInfo>> UpdateInventory(InventoryBillInfo inventory)
        {
            ResponseMessage<List<InventoryInfo>> result = new() { Data = [] };
            foreach (var detail in inventory.Details)
            {
                //不启用库存 ,  直发仓库 不可用状态都不用记录
                if (!detail.Material.IsInventory || detail.Stock.IsDirStock || detail.StockStatus.Id != 10000)
                {
                    continue;
                }
                InventoryInfo inventoryInfo = new()
                {
                    MaterialNumber = detail.Material.Code,
                    MaterialName = detail.Material.Name,
                    StockNumber = detail.Stock.Code,
                    StockName = detail.Stock.Name,
                    StockOrgNumber = inventory.StockOrg.Code,
                    StockOrgName = inventory.StockOrg.Name,
                    UnitNumber = detail.BaseUnit.Code,
                    UnitName = detail.BaseUnit.Name
                };
                var cacheInventory = _redisCache.HashGet(inventoryInfo, "inventory");
                cacheInventory ??= inventoryInfo;
                if (inventory.AddQty)
                {
                    cacheInventory.BaseQty += detail.BaseQty;
                }
                else
                {
                    cacheInventory.BaseQty -= detail.BaseQty;
                }
                result.Data.Add(inventoryInfo);
                _redisCache.HashSet(cacheInventory, isSaveRedisValue: false);
            }
            return result;
        }

        /// <summary>
        /// 缓存在途出数
        /// </summary>
        private void ReloadInspQtyCahce(MymoooSqlSugar sqlSugar)
        {
            var sql = @"select SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKJOINQTY) InspQty,m.FNUMBER as MaterialNumber
from T_PUR_RECEIVEENTRY_S t1
	inner join T_PUR_ReceiveEntry t2 on t1.FENTRYID = t2.FENTRYID
	inner join T_BD_MATERIAL m on m.FMATERIALID = t2.FMATERIALID
	inner join T_PUR_Receive t3 on t1.FID = t3.FID
where t3.FDOCUMENTSTATUS = 'C' and t2.FCHECKINCOMING = 1
group by m.FNUMBER
having SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKJOINQTY) > 0";

            var stocks = sqlSugar.Ado.SqlQuery<InventoryInfo>(sql).ToArray();

            foreach (var stock in stocks)
            {
                _redisCache.HashSet(stock, s => s.InspQty);
            }
        }

        /// <summary>
        /// 缓存在途出数
        /// </summary>
        private void ReloadOnOrderQtyCahce(MymoooSqlSugar sqlSugar)
        {
            var sql = @"select SUM(FREMAINSTOCKINQTY) OnOrderQty,m.FNUMBER as MaterialNumber
from	T_PUR_POORDERENTRY_R t1
	inner join T_PUR_POORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
	inner join T_BD_MATERIAL m on m.FMATERIALID = t2.FMATERIALID
	inner join T_PUR_POORDER t3 on t1.FID = t3.FID
where t3.FDOCUMENTSTATUS = 'C' AND t3.FCLOSESTATUS='A' and t2.FMrpCloseStatus = 'A' and t2.FMrpFreezeStatus = 'A' and t2.FMrpTerminateStatus = 'A'
group by m.FNUMBER
having SUM(FREMAINSTOCKINQTY) > 0";

            var stocks = sqlSugar.Ado.SqlQuery<InventoryInfo>(sql).ToArray();

            foreach (var stock in stocks)
            {
                _redisCache.HashSet(stock, s => s.OnOrderQty);
            }
        }

        /// <summary>
        /// 缓存总待出数
        /// </summary>
        private void ReloadShipdSumQtyCahce(MymoooSqlSugar sqlSugar)
        {
            var sql = @"select SUM(FREMAINOUTQTY) UnQtyShipdSum, m.FNUMBER as MaterialNumber
from T_SAL_ORDERENTRY_R t1 
	inner join T_SAL_ORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID 
	inner join T_BD_MATERIAL m on m.FMATERIALID = t2.FMATERIALID
	inner join T_SAL_ORDER t3 on t1.FID = t3.FID
	where t3.FDOCUMENTSTATUS = 'C' AND t3.FCLOSESTATUS='A' and t2.FMrpCloseStatus = 'A' and t2.FMrpFreezeStatus = 'A' and t2.FMrpTerminateStatus = 'A'
	group by m.FNUMBER
having SUM(FREMAINOUTQTY) > 0";

            var stocks = sqlSugar.Ado.SqlQuery<InventoryInfo>(sql).ToArray();

            foreach (var stock in stocks)
            {
                _redisCache.HashSet(stock, s => s.UnQtyShipdSum);
            }
        }

        private void ReloadStockCahce(MymoooSqlSugar sqlSugar)
        {
            var sql = @"select org.FNUMBER as StockOrgNumber,orgl.FNAME as StockOrgName,sto.FNUMBER as StockNumber,stol.FNAME as StockName,m.FNUMBER as MaterialNumber,ml.FNAME as MaterialName,
u.FNUMBER as UnitNumber,ul.FNAME as UnitName,t1.FBASEQTY as BaseQty,t1.FAVBQTY as UsableQty,t1.FLOCKQTY as LockQty
from
	V_STK_INVENTORY_CUS t1
	inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID = org.FORGID
	inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID
	inner join T_BD_STOCK sto on t1.FSTOCKID = sto.FSTOCKID
	inner join T_BD_STOCK_L stol on sto.FSTOCKID = stol.FSTOCKID
	inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID = stos.FSTOCKSTATUSID
	inner join T_BD_MATERIAL m on t1.FMATERIALID = m.FMASTERID	and t1.FSTOCKORGID = m.FUSEORGID
	inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID
	inner join T_BD_UNIT u on t1.FBASEUNITID = u.FUNITID
	inner join T_BD_UNIT_L ul on t1.FBASEUNITID = ul.FUNITID
where stos.FSTOCKSTATUSID = 10000 and t1.FOWNERTYPEID = 'BD_OwnerOrg' and t1.FBASEQTY > 0 ";

            var stocks = sqlSugar.Ado.SqlQuery<InventoryInfo>(sql).ToArray();

            foreach (var stock in stocks)
            {
                _redisCache.HashSet(stock);
            }
        }
    }
}
