using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Kingdee.K3.Core.SCM.STK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.App.Core.StockManagement
{
    public class StockLockUnLockService : IStockLockUnLockService
    {
        public DynamicObjectCollection SaveLockInfo(Context ctx, StockLockUnLockEntity stockLock, string billtype)
        {
            List<LockStockArgs> list = new List<LockStockArgs>();
            string sSql = $@"select T.FID InvDetailID,
                            T.FSTOCKORGID StockOrgId,
                            T.FMATERIALID MaterialId,
                            T1.FPLANMODE PlanMode,
                            T.FBOMID BomId,
                            T.FAuxPropId AuxPropId,
                            T.FLOT,
                            T2.FNUMBER LotNo,
                            T.FMTONO MtoNo,
                            T.FPROJECTNO ProjectNo,
                            T.FPRODUCEDATE ProduceDate,
                            T.FEXPIRYDATE ExpiryDate,
                            T.FSTOCKID StockId,
                            T.FSTOCKLOCID StockLocId,
                            T.FSTOCKSTATUSID StockStatusId,
                            T.FOWNERTYPEID OwnerTypeId,
                            T.FOWNERID OwnerId,
                            T.FKEEPERTYPEID KeeperTypeId,
                            T.FKEEPERID KeeperId,
                            T.FSTOCKUNITID UnitID,
                            T.FBASEUNITID BaseUnitId,
                            T.FSECUNITID SecUnitId,
                            T.FBASEQTY,T.FBASEAVBQTY
                            from V_STK_INVENTORY_CUS T
                            LEFT JOIN T_BD_STOCK S on S.FSTOCKID=T.FSTOCKID
                            LEFT JOIN t_BD_MaterialPlan T1 ON T.FMATERIALID=T1.FMATERIALID
                            LEFT JOIN T_BD_LOTMASTER T2 ON T.FLOT=T2.FLOTID
                            where T.FSTOCKORGID={stockLock.StockOrgID} and T.FMATERIALID={stockLock.MaterialID}
                            order by S.FISOUTSOURCESTOCK desc,T.FBASEAVBQTY desc";
            DynamicObjectCollection DynamData = DBUtils.ExecuteDynamicObject(ctx, sSql);

            if (DynamData.Count > 0)
            {
                foreach (DynamicObject ite in DynamData)
                {
                    var FBASEAVBQTY = Convert.ToDecimal(ite["FBASEAVBQTY"]);
                    if (stockLock.FLockQTY <= FBASEAVBQTY)
                    {
                        list.Add(GetSubData(ite, stockLock, stockLock.FLockQTY, 1));
                        break;
                    }
                    if (stockLock.FLockQTY > FBASEAVBQTY)
                    {
                        list.Add(GetSubData(ite, stockLock, FBASEAVBQTY, 1));
                        stockLock.FLockQTY = stockLock.FLockQTY - FBASEAVBQTY;
                    }
                }
            }
            K3.SCM.App.Core.StockLockService lockService = new K3.SCM.App.Core.StockLockService();
            return lockService.SaveLockInfo(ctx, list, billtype, true);
        }
        public void SaveUnLockInfo(Context ctx, StockLockUnLockEntity stockLock)
        {
            List<LockStockArgs> list = new List<LockStockArgs>();
            string sSql = $@"select
                            T.FSTOCKORGID,
                            T.FMATERIALID,
                            T.finvdetailid InvDetailID,
                            T.fid FEntryID,
                            T.fbilldetailid as BillDetailID,
                            S1.FBILLTYPEID as BillTypeID,
                            T.FSTOCKID StockId,
                            T.fbillno as BillNo,
                            S.FSEQ as BillSEQ,
                            T.FLOT,
                            --T2.FNUMBER as LotNo,
                            {(stockLock.FLockQTY == 0 ? "T.fbaselockqty" : stockLock.FLockQTY.ToString())} as FLockQTY
                            from V_PLN_LOCKSTOCK T
                            inner join T_BD_MATERIAL M on T.FMATERIALID=M.FMATERIALID
                            --inner JOIN T_BD_LOTMASTER T2 ON T.FLOT=T2.FLOTID
                            inner join T_SAL_DELIVERYNOTICEENTRY S on T.fbilldetailid=S.FENTRYID
                            inner join t_SAL_DELIVERYNOTICE S1 on S.FID=S1.FID
                            where T.fobjectid='SAL_DELIVERYNOTICE'";
            if (!(stockLock.StockOrgID == 0))
            {
                sSql += $" AND T.FSTOCKORGID='{stockLock.StockOrgID}'";
            }
            if (!stockLock.BillNo.IsNullOrEmptyOrWhiteSpace())
            {
                sSql += $" AND T.FBILLNO='{stockLock.BillNo}'";
            }
            if (!(stockLock.MaterialID == 0))
            {
                sSql += $" AND M.FMATERIALID='{stockLock.MaterialID}'";
            }
            if (!stockLock.BillDetailID.IsNullOrEmptyOrWhiteSpace())
            {
                sSql += $" AND T.fbilldetailid='{stockLock.BillDetailID}'";
            }
            if (!stockLock.BillTypeID.IsNullOrEmptyOrWhiteSpace())
            {
                sSql += $" AND S1.FBILLTYPEID='{stockLock.BillTypeID}'";
            }
            if (!(stockLock.BillSEQ == 0))
            {
                sSql += $" AND S.FSEQ='{stockLock.BillSEQ}'";
            }
            if (!(stockLock.StockID == 0))
            {
                sSql += $" AND T.FSTOCKID='{stockLock.StockID}'";
            }

            DynamicObjectCollection DynamData = DBUtils.ExecuteDynamicObject(ctx, sSql);

            if (DynamData.Count > 0)
            {
                foreach (DynamicObject ite in DynamData)
                {
                    list.Add(GetSubData(ite, null, 0, 0));
                }
            }
            K3.SCM.App.Core.StockLockService lockService = new K3.SCM.App.Core.StockLockService();
            lockService.SaveUnLockInfo(ctx, list);
        }
        private LockStockArgs GetSubData(DynamicObject materialObj, StockLockUnLockEntity stockLock, decimal FQTY, int isNow)
        {
            LockStockArgs lockStockArgs = new LockStockArgs();
            if (isNow == 1)//锁库
            {
                lockStockArgs.ObjectId = stockLock.ObjectId;//单据对象内码
                lockStockArgs.BillId = stockLock.BillId;//单据内码
                lockStockArgs.BillDetailID = stockLock.BillDetailID;//单据明细内码
                lockStockArgs.BillTypeID = stockLock.BillTypeID;
                lockStockArgs.BillNo = stockLock.BillNo;
                lockStockArgs.BillSEQ = stockLock.BillSEQ;
                lockStockArgs.FInvDetailID = Convert.ToString(materialObj["InvDetailID"]);//即时库存明细内码
                lockStockArgs.StockOrgID = Convert.ToInt64(materialObj["StockOrgId"]);//库存组织
                lockStockArgs.DemandOrgId = Convert.ToInt64(materialObj["StockOrgId"]);//需求组织内码
                lockStockArgs.MaterialID = Convert.ToInt64(materialObj["MaterialID"]);//物料内码
                lockStockArgs.DemandMaterialId = Convert.ToInt64(materialObj["MaterialID"]);//需求物料内码
                lockStockArgs.DemandDateTime = DateTime.Now;//需求日期
                lockStockArgs.DemandPriority = "";//需求优先级
                if (materialObj["PlanMode"] != null && Convert.ToString(materialObj["PlanMode"]) == "1")
                {
                    lockStockArgs.IsMto = "1";//是否MTO
                }
                lockStockArgs.BOMID = Convert.ToInt64(materialObj["BomID"]);//BomID
                lockStockArgs.AuxPropId = Convert.ToInt64(materialObj["AuxPropId"]);//辅助属性
                if (materialObj["FLOT"] != null && Convert.ToInt64(materialObj["FLOT"]) > 0)
                {
                    lockStockArgs.Lot = Convert.ToInt64(materialObj["FLOT"]);////批号内码
                    lockStockArgs.LotNo = Convert.ToString(materialObj["LotNo"]);//批号编码
                }
                lockStockArgs.MtoNo = Convert.ToString(materialObj["MtoNo"]);//MTO号
                lockStockArgs.ProjectNo = Convert.ToString(materialObj["ProjectNo"]);//计划跟踪号
                if (materialObj["ProduceDate"] != null)
                {
                    lockStockArgs.ProduceDate = DateTime.Parse(Convert.ToString(materialObj["ProduceDate"]));//生成日期
                }

                if (materialObj["ExpiryDate"] != null)
                {
                    lockStockArgs.ExpiryDate = DateTime.Parse(Convert.ToString(materialObj["ExpiryDate"]));//有效期
                }

                lockStockArgs.STOCKID = Convert.ToInt64(materialObj["StockId"]);//仓库内码
                lockStockArgs.StockLocID = Convert.ToInt64(materialObj["StockLocID"]);//仓位内码
                lockStockArgs.StockStatusID = Convert.ToInt64(materialObj["StockStatusID"]);//仓库状态
                lockStockArgs.OwnerTypeID = Convert.ToString(materialObj["OwnerTypeId"]);//货主类型
                lockStockArgs.OwnerID = Convert.ToInt64(materialObj["OwnerID"]);//货主
                lockStockArgs.KeeperTypeID = Convert.ToString(materialObj["KeeperTypeId"]);//保管者类型
                lockStockArgs.KeeperID = Convert.ToInt64(materialObj["KeeperID"]);//保管者
                lockStockArgs.UnitID = Convert.ToInt64(materialObj["UnitID"]);//库存单位
                lockStockArgs.BaseUnitID = Convert.ToInt64(materialObj["BaseUnitID"]);//基本单位
                lockStockArgs.SecUnitID = Convert.ToInt64(materialObj["SecUnitID"]);//辅助单位
                lockStockArgs.LockQty = FQTY;//锁库数量
                lockStockArgs.LockBaseQty = FQTY;//锁库数量（基本）
                lockStockArgs.LockSecQty = 0;//锁库数量(辅助)
                //object obj2 = materialObj["ReserveDate"];
                //if (obj2 != null && !string.IsNullOrWhiteSpace(obj2.ToString()))
                //{
                lockStockArgs.ReserveDate = DateTime.Now;//锁库日期
                lockStockArgs.BaseQty = Convert.ToDecimal(materialObj["FBASEQTY"]);
                //}
                //lockStockArgs.ReserveDays = Convert.ToInt32(materialObj["ReserveDays"]);//锁库天数
                //obj2 = materialObj["ReleaseDate"];
                //if (obj2 != null && !string.IsNullOrWhiteSpace(obj2.ToString()))
                //{
                // lockStockArgs.ReLeaseDate = DateTime.Parse(obj2.ToString());//预计解锁日期
                //}
                //lockStockArgs.SupplyNote = Convert.ToString(materialObj["SupplyNote"]);//供应备注
                //lockStockArgs.RequestNote = lockStockArgs.SupplyNote;//需求备注
            }
            else//解锁
            {
                lockStockArgs.FInvDetailID = Convert.ToString(materialObj["InvDetailID"]);
                lockStockArgs.FEntryID = Convert.ToInt64(materialObj["FEntryID"]);
                lockStockArgs.BillDetailID = Convert.ToString(materialObj["BillDetailID"]);//单据明细内码
                lockStockArgs.BillTypeID = Convert.ToString(materialObj["BillTypeID"]);
                lockStockArgs.BillNo = Convert.ToString(materialObj["BillNo"]);
                lockStockArgs.BillSEQ = Convert.ToInt32(materialObj["BillSEQ"]);
                if (materialObj["FLOT"] != null && Convert.ToInt64(materialObj["FLOT"]) > 0)
                {
                    lockStockArgs.Lot = Convert.ToInt64(materialObj["FLOT"]);////批号内码
                    lockStockArgs.LotNo = Convert.ToString(materialObj["LotNo"]);//批号编码
                }
                lockStockArgs.STOCKID = Convert.ToInt64(materialObj["StockId"]);//仓库内码
                lockStockArgs.LockQty = 0;
                lockStockArgs.UnLockQty = Convert.ToDecimal(materialObj["FLockQTY"]);
                lockStockArgs.LockBaseQty = 0;
                lockStockArgs.UnLockBaseQty = Convert.ToDecimal(materialObj["FLockQTY"]);
                lockStockArgs.LockSecQty = 0;
                lockStockArgs.UnLockSecQty = 0;
                //lockStockArgs.ReserveDate = DateTime.Parse(item["ReserveDate"].ToString());
                //lockStockArgs.ReserveDays = Convert.ToInt32(item["ReserveDays"]);
                lockStockArgs.ReLeaseDate = DateTime.Now;
                lockStockArgs.UnLockNote = "";
                //lockStockArgs.FEntryID = Convert.ToInt64(item["Id"]);
                //lockStockArgs.FInvDetailID = item["InvDetailID"].ToString();
                //lockStockArgs.BillDetailID = item["BillDetailID"].ToString();
                //lockStockArgs.BillNo = Convert.ToString(item["BILLNO"]);
                //lockStockArgs.BillSEQ = Convert.ToInt32(item["BILLSEQ"]);
                //lockStockArgs.Lot = Convert.ToInt64(materialObj["FLOT"]);////批号内码
                //lockStockArgs.LotNo = Convert.ToString(materialObj["LotNo"]);//批号编码
                //lockStockArgs.LockQty = lockQty;
                //lockStockArgs.UnLockQty = unLockQty;
                //lockStockArgs.LockBaseQty = Convert.ToDecimal(item["BaseLcokQty"]);
                //lockStockArgs.UnLockBaseQty = num3;
                //lockStockArgs.LockSecQty = Convert.ToDecimal(item["SecLockQty"]);
                //lockStockArgs.UnLockSecQty = num2;
                //if (item["ReserveDate"] != null && !string.IsNullOrWhiteSpace(item["ReserveDate"].ToString()))
                //{
                //    lockStockArgs.ReserveDate = DateTime.Parse(item["ReserveDate"].ToString());
                //}

                //lockStockArgs.ReserveDays = Convert.ToInt32(item["ReserveDays"]);
                //if (item["ReleaseDate"] != null && !string.IsNullOrWhiteSpace(item["ReleaseDate"].ToString()))
                //{
                //    lockStockArgs.ReLeaseDate = DateTime.Parse(item["ReleaseDate"].ToString());
                //}

                //lockStockArgs.UnLockNote = Convert.ToString(item["UnLockNote"]);
                //if (flag)
                //{
                //    warnStockArgList.Add(lockStockArgs);
                //}
                //else
                //{
                //    stockArgList.Add(lockStockArgs);
                //}
            }
            return lockStockArgs;
        }
    }
}
