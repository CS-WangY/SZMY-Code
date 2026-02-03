using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    /// <summary>
    /// 库存验证
    /// </summary>
    public class StockValidator
    {
        public ValidateContext SalDeliveryNoticeValidator(ExtendedDataEntity dataEntitie, ValidateContext validateContext, Context ctx)
        {
            var itemlist = dataEntitie["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;
            foreach (var item in itemlist)
            {
                var material = item["MaterialID"] as DynamicObject;
                var soentryid = Convert.ToInt64(item["SOEntryId"]);

                if (itemlist.Where(x => Convert.ToInt64(x["SOEntryId"]) == soentryid).Count() > 1)
                {
                    validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                        string.Empty,
                        Convert.ToString(dataEntitie.DataEntity[0]),
                        dataEntitie.DataEntityIndex,
                        dataEntitie.RowIndex,
                        "V1",
                        $"物料[{material["Number"]}具有相同源单，不允许保存！]",
                        string.Empty,
                        ErrorLevel.FatalError
                             ));
                }
            }

            return validateContext;
        }
        public ValidateContext ExecuteStockValidatorN(ExtendedDataEntity dataEntitie, ValidateContext validateContext, Context ctx)
        {
            //如果供货组织是深圳则跳出
            //if (Convert.ToInt64(dataEntitie.DataEntity["DeliveryOrgID_Id"]) == 224428) return validateContext;
            //如果不是则跳出深圳224428广东蚂蚁669144
            long[] SalOrgList = new long[] { 224428, 669144 };
            if (!SalOrgList.Contains(Convert.ToInt64(dataEntitie.DataEntity["SaleOrgId_Id"])))
            {
                return validateContext;
            }
            //if (((DynamicObject)dataEntitie["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY") return validateContext;
            //已批核状态不验证(云仓储回传需要更新包装日期)
            if (dataEntitie["DocumentStatus"].ToString() == "C") return validateContext;
            var itemlist = dataEntitie["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;
            List<StockVerifyEntity> list = new List<StockVerifyEntity>();
            foreach (var item in itemlist)
            {
                var material = item["MaterialID"] as DynamicObject;
                var materialID = Convert.ToInt64(item["MaterialID_Id"]);
                var msterID = material["msterID"];
                var srcunitID = Convert.ToInt64(item["UnitID_Id"]);
                StockVerifyEntity stockVerify = new StockVerifyEntity();
                var srcbillid = "";
                var srcrowid = "";
                decimal resqty = 0;
                if (Convert.ToDecimal(item["Qty"]) <= 0)
                {
                    validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                        string.Empty,
                        Convert.ToString(dataEntitie.DataEntity[0]),
                        dataEntitie.DataEntityIndex,
                        dataEntitie.RowIndex,
                        "V1",
                        $"物料编号[{Convert.ToString(((DynamicObject)item["MaterialID"])["Number"])}]销售数量不能为0",
                        string.Empty,
                        ErrorLevel.FatalError
                             ));
                }
                else
                {
                    foreach (var itemlink in item["FEntity_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = Convert.ToString(itemlink["SBillId"]);
                        srcrowid = Convert.ToString(itemlink["SId"]);
                    }
                    //没有上游单据的不校验库存
                    if (srcrowid.IsNullOrEmptyOrWhiteSpace()) return validateContext;
                    //如果发货单类型是
                    string sSql = "";
                    if (((DynamicObject)dataEntitie["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY")
                    {
                        //取当前单据直发仓库预留数量
                        sSql = $@"/*dialect*/SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FBASEUNITID
                                FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                INNER JOIN T_BD_STOCK t3 ON t1.FSTOCKID=t3.FSTOCKID
                                WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                                AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                                AND t1.FSUPPLYFORMID='STK_Inventory'
                                AND t3.FISDIRSTOCK=1
                                GROUP BY t1.FBASEUNITID";
                    }
                    else
                    {
                        //取当前单据预留数量
                        sSql = $@"/*dialect*/SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FBASEUNITID
                                FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                INNER JOIN dbo.T_BD_STOCK t3 ON t1.FSTOCKID=t3.FSTOCKID
                                WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                                AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                                AND t1.FSUPPLYFORMID='STK_Inventory'
                                AND t3.FISDIRSTOCK=0 AND t3.FNOTALLOWDELIVERY=0 AND t1.FSUPPLYORGID={Convert.ToInt64(item["FSupplyTargetOrgId_Id"])}
                                GROUP BY t1.FBASEUNITID";
                    }

                    var resdata = DBUtils.ExecuteDynamicObject(ctx, sSql);
                    long resunitID = 0;
                    if (resdata.Count > 0)
                    {
                        resqty = Convert.ToDecimal(resdata[0]["FBASEQTY"]);
                        resunitID = Convert.ToInt64(resdata[0]["FBASEUNITID"]);
                    }
                    if (srcunitID != resunitID)
                    {
                        IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                        resqty = convertService.GetUnitTransQty(ctx, materialID, resunitID, srcunitID, resqty);
                    }


                    //if (resqty <= 0)
                    //{
                    //    validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                    //        string.Empty,
                    //        Convert.ToString(dataEntitie.DataEntity[0]),
                    //        dataEntitie.DataEntityIndex,
                    //        dataEntitie.RowIndex,
                    //        "V1",
                    //        $"第{item["Seq"]}行,校验出错,未能找到可调拨的库存数量,请检查MRP计划以及事业部库存!",
                    //        string.Empty,
                    //        ErrorLevel.FatalError
                    //             ));
                    //}
                    //if (validateContext.Error().Count > 0)
                    //{
                    //    return validateContext;
                    //}


                    stockVerify.msterID = Convert.ToInt64(((DynamicObject)item["MaterialID"])["msterID"]);
                    stockVerify.msterNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]);
                    stockVerify.stockorgid = Convert.ToInt64(item["FSupplyTargetOrgId_Id"]);
                    stockVerify.stockorgname = Convert.ToString(((DynamicObject)item["FSupplyTargetOrgId"])["Name"]);
                    stockVerify.Qty = Convert.ToDecimal(item["Qty"]);
                    stockVerify.LockQty = resqty;
                    list.Add(stockVerify);
                }

            }
            var varAllBills = list
                .GroupBy(o => new
                {
                    msterID = o.msterID,
                    msterNumber = o.msterNumber,
                    stockOrgID = o.stockorgid,
                    stockorgname = o.stockorgname,
                })
                .Select(g => new
                {
                    msterID = g.Key.msterID,
                    msterNumber = g.Key.msterNumber,
                    stockOrgID = g.Key.stockOrgID,
                    stockorgname = g.Key.stockorgname,
                    Qty = g.Sum(n => n.Qty),
                    LockQty = g.Sum(n => n.LockQty),
                }).ToList();

            foreach (var dnitem in varAllBills)
            {
                var materqtys = StockQuantityServiceHelper.InventoryQty(ctx, dnitem.msterID, new List<long> { dnitem.stockOrgID });
                decimal materqty = 0;
                if (materqtys.Count > 0)
                {
                    materqty = materqtys.GroupBy(p => p["FMATERIALID"]).Select(t => new
                    {
                        AVBQTY = t.Sum(s => (decimal)s["FBASEQTY"])
                    }).ToList()[0].AVBQTY;
                }

                if (materqty - (dnitem.Qty - dnitem.LockQty) < 0)
                {
                    validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                        string.Empty,
                        Convert.ToString(dataEntitie.DataEntity[0]),
                        dataEntitie.DataEntityIndex,
                        dataEntitie.RowIndex,
                        "V1",
                        $"物料编号[{dnitem.msterNumber}]可用库存不足,即时库存验证[{dnitem.stockorgname}]",
                        string.Empty,
                        ErrorLevel.FatalError
                             ));
                }
            }
            return validateContext;
        }
        /// <summary>
        /// 执行库存验证
        /// </summary>
        /// <param name="dataEntities"></param>
        /// <param name="validateContext"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ValidateContext ExecuteStockValidator(ExtendedDataEntity dataEntitie, ValidateContext validateContext, Context ctx)
        {
            if (((DynamicObject)dataEntitie["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY") return validateContext;
            //已批核状态不验证(云仓储回传需要更新包装日期)
            if (dataEntitie["DocumentStatus"].ToString() == "C") return validateContext;
            var billno = Convert.ToString(dataEntitie["BillNo"]);
            var list = dataEntitie["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;
            //物料集合
            List<long> materialList = new List<long>();
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            //发货组织
            long deliveryDetOrgID = Int64.Parse(dataEntitie["DeliveryOrgID_Id"].ToString());
            //先获取行数据
            foreach (var items in list)
            {
                //获取物料ID集合
                materialList.Add(Int64.Parse(items["MaterialID_Id"].ToString()));
                //强锁数量
                decimal forcedLockQty = 0;
                decimal sumDnQty = 0;

                //物料编码
                long materialID = Int64.Parse(items["MaterialID_Id"].ToString());
                //发货数量
                decimal qty = decimal.Parse(items["Qty"].ToString());
                string soId = "";
                string sodId = "";
                //上游单是销售订单的才需要验证
                if (Convert.ToString(items["SrcType"]).Equals("SAL_SaleOrder"))
                {
                    sodId = Convert.ToString(items["SOEntryId"]);
                    //获取销售订单ID
                    if (((DynamicObjectCollection)items["FEntity_Link"]).Count > 0)
                    {
                        soId = ((DynamicObjectCollection)items["FEntity_Link"])[0]["SBillId"].ToString();
                    }
                    //存在销售订单，获取销售订单的强锁数量
                    if (!string.IsNullOrWhiteSpace(soId))
                    {
                        forcedLockQty = GetForcedLockQty(soId, sodId, ctx);
                        //获取累计发货数量
                        sumDnQty = GetDnQty(soId, sodId, ctx);
                    }
                }
                orderDetailsList.Add(new OrderDetails
                {
                    DeliveryDetOrgID = deliveryDetOrgID,
                    DeliveryDetOrgName = Convert.ToString(((DynamicObject)items["DeliveryOrgID"])["Name"]),
                    MaterialID = materialID,
                    MaterialCode = Convert.ToString(((DynamicObject)items["MaterialID"])["Number"]),
                    Qty = qty,
                    Seq = Int32.Parse(Convert.ToString(items["seq"])),
                    LockStockQty = forcedLockQty,
                    SumDnQty = sumDnQty
                });

            }
            //获取物料库存(一个物料可能存在多个仓库，需要汇总)
            var stockQuantityList = StockQuantityServiceHelper.StockQuantityAction(ctx, null, materialList.Distinct().ToList(), -1);
            //根据物料ID和组织编号汇总
            var newStockQuantityList = stockQuantityList.GroupBy(p => new { p.ItemID, p.OrgId }).Select(t => new SumGroupStock
            {
                ItemID = t.Key.ItemID,
                OrgId = t.Key.OrgId,
                UsableQty = t.Sum(s => s.UsableQty)
            }).ToList();

            //开始验证库存
            foreach (var orderItem in orderDetailsList)
            {
                //获取实际可能库存
                var stock = newStockQuantityList.FirstOrDefault(o => o.ItemID.Equals(orderItem.MaterialID) && o.OrgId.Equals(orderItem.DeliveryDetOrgID));

                //强预留不足
                if ((orderItem.LockStockQty - orderItem.SumDnQty) < orderItem.Qty)
                {
                    if (stock == null)
                    {
                        validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                        string.Empty,
                        dataEntitie["Id"].ToString(),
                        dataEntitie.DataEntityIndex,
                        dataEntitie.RowIndex,
                        dataEntitie["Id"].ToString(),
                         $"序号[{orderItem.Seq}]的物料编号[{orderItem.MaterialCode}]可用库存不足",
                         $"即时库存验证[{orderItem.DeliveryDetOrgName}]{billno}",
                        ErrorLevel.FatalError));
                    }
                    else
                    {
                        //强预留有剩余，需要使用可用库存
                        if ((orderItem.LockStockQty - orderItem.SumDnQty) > 0 && (orderItem.LockStockQty - orderItem.SumDnQty) + stock.UsableQty >= orderItem.Qty)
                        {
                            stock.UsableQty = stock.UsableQty - (orderItem.Qty - (orderItem.LockStockQty - orderItem.SumDnQty));
                        }
                        else if (stock.UsableQty >= orderItem.Qty)//验证可用库存
                        {
                            stock.UsableQty = stock.UsableQty - orderItem.Qty;
                        }
                        else
                        {
                            validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                             string.Empty,
                             dataEntitie["Id"].ToString(),
                             dataEntitie.DataEntityIndex,
                             dataEntitie.RowIndex,
                             dataEntitie["Id"].ToString(),
                              $"序号[{orderItem.Seq}]的物料编号[{orderItem.MaterialCode}]可用库存不足",
                              $"即时库存验证[{orderItem.DeliveryDetOrgName}]{billno}",
                             ErrorLevel.FatalError));
                        }
                    }
                }
            }

            return validateContext;
        }
        /// <summary>
        /// 获取销售单强锁数量
        /// </summary>
        /// <returns></returns>
        private decimal GetForcedLockQty(string soId, string sodId, Context ctx)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SoId", KDDbType.String, soId), new SqlParam("@SodId", KDDbType.String, sodId) };
            var sql = $@"SELECT SUM(FBASEQTY) as FBASEQTY FROM T_PLN_RESERVELINKENTRY t1
                                LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                where FSUPPLYFORMID='STK_Inventory' and t2.FSRCINTERID=@SoId and t2.FSRCENTRYID=@SodId";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 获取累计发货数量
        /// </summary>
        /// <returns></returns>
        private decimal GetDnQty(string soId, string sodId, Context ctx)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SoId", KDDbType.String, soId), new SqlParam("@SodId", KDDbType.String, sodId) };
            var sql = $@"select SUM(case when b.FSumOutQty>0 then 0 else b.FQTY  end) Qty from T_SAL_DELIVERYNOTICE  a
                        inner join T_SAL_DELIVERYNOTICEENTRY b on a.FID=b.FID
                        inner join T_SAL_DELIVERYNOTICEENTRY_LK c on c.FENTRYID=b.FENTRYID
                        where a.FDocumentStatus in ('B','C') and c.FSBILLID=@SoId and c.FSID=@SodId and b.FSrcType='SAL_SaleOrder' ";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }


        /// <summary>
        /// 汇总实际可用库存
        /// </summary>
        public class SumGroupStock
        {
            /// <summary>
            /// 发货组织
            /// </summary>
            public long OrgId { get; set; }

            /// <summary>
            /// 物料ID
            /// </summary>
            public long ItemID { get; set; }

            /// <summary>
            /// 实际可用库存
            /// </summary>
            public decimal UsableQty { get; set; }

        }

        /// <summary>
        /// 订单明细
        /// </summary>
        public class OrderDetails
        {
            /// <summary>
            /// 发货通知单明细序号
            /// </summary>
            public int Seq { get; set; }

            /// <summary>
            /// 发货组织
            /// </summary>
            public long DeliveryDetOrgID { get; set; }
            /// <summary>
            /// 发货组织名称
            /// </summary>
            public string DeliveryDetOrgName { get; set; }
            /// <summary>
            /// 物料ID
            /// </summary>
            public long MaterialID { get; set; }

            /// <summary>
            /// 物料编号
            /// </summary>
            public string MaterialCode { get; set; }

            /// <summary>
            /// 发货数量
            /// </summary>
            public decimal Qty { get; set; }

            /// <summary>
            /// 锁库数量
            /// </summary>
            public decimal LockStockQty { get; set; }

            /// <summary>
            /// 累计发货数
            /// </summary>
            public decimal SumDnQty { get; set; }

        }

    }

    public class StockVerifyEntity
    {
        public long msterID { get; set; }
        public string msterNumber { get; set; }
        public long stockorgid { get; set; }
        public string stockorgname { get; set; }
        public decimal Qty { get; set; }
        public decimal LockQty { get; set; }
    }
}
