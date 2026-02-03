using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.DirectSaleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web.UI.WebControls.WebParts;

namespace Kingdee.Mymooo.Business.PlugIn.DirectSaleManagement
{
    public class DirectSaleBusiness
    {
        /// <summary>
        /// 根据销售订单明细获取采购单号信息和直发数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetSoToPoDirQty(Context ctx, reqSoPoDirQtyFilter req)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (req.FbdDetIdList == "" || req.SoNo == "")
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "请传销售订单相关信息";
                return response;
            }
            SoDirEntity soDirEntity = new SoDirEntity();
            //获取销售订单表头信息
            var sSql = $@"/*dialect*/select top 1 t1.FID from T_SAL_ORDER t1  where t1.FBILLNO='{req.SoNo}' and FDOCUMENTSTATUS='C' and  FCANCELSTATUS='A' ";
            var soFid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
            if (soFid == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = $"销售订单号[{req.SoNo}]无效";
                return response;
            }
            soDirEntity.SoFId = soFid;
            soDirEntity.SoNo = req.SoNo;

            //获取销售订单的可发数量
            List<SoDetDirEntity> sodList = new List<SoDetDirEntity>();
            List<GetDirPoEntity> poList = new List<GetDirPoEntity>();
            sSql = $@"/*dialect*/select t2.FENTRYID SoEntryId,t2.FSEQ SoSeqNo,t2.FOrderDetailId,t3.FNUMBER SoItemNo,t4.FBASECANOUTQTY,t2.FSUPPLYTARGETORGID,
						(select top 1 FSTOCKID from t_BD_Stock t5 where FUseOrgId=t2.FSUPPLYTARGETORGID and FISDIRSTOCK=1 and t5.FDOCUMENTSTATUS='C' ) FSTOCKID
                        from T_SAL_ORDER t1
                        INNER JOIN T_SAL_ORDERENTRY t2 on t1.FID=t2.FID
                        INNER JOIN T_BD_MATERIAL t3 on t3.FMATERIALID=t2.FMATERIALID
                        INNER JOIN T_SAL_ORDERENTRY_R  t4 on t2.FENTRYID=t4.FENTRYID
                        where t1.FID={soFid} and t2.FOrderDetailId in ({req.FbdDetIdList.Replace("|", ",")}) ";
            var soDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            List<string> FSRCENTRYIDList = new List<string>();
            foreach (var item in soDatas)
            {
                sodList.Add(new SoDetDirEntity
                {
                    SoEntryId = Convert.ToInt64(item["SoEntryId"]),
                    SoSeqNo = Convert.ToInt32(item["SoSeqNo"]),
                    FbdDetId = Convert.ToInt32(item["FOrderDetailId"]),
                    ItemNo = item["SoItemNo"].ToString(),
                    FBaseCanOutQty = Convert.ToDecimal(item["FBASECANOUTQTY"]),
                    SupplyOrgId = Convert.ToInt64(item["FSUPPLYTARGETORGID"]),
                    DirStockId = Convert.ToInt64(item["FSTOCKID"]),

                });
                FSRCENTRYIDList.Add(item["SoEntryId"].ToString());
            }
            if (sodList.Count > 0)
            {
                //获取对应的每个采购订单项的总可出数量
                sSql = $@"/*dialect*/select tt1.*,tt2.FSEQ PoSeqNo,(tt2.FQTY-tt3.FBASEJOINQTY) PoSumSyQty from (
                        SELECT t1.FSUPPLYINTERID PoFID,t1.FSUPPLYENTRYID PoEntryId,t3.FBILLNO PoNo
                        FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        INNER JOIN T_PUR_POORDER t3 on t3.FID= t1.FSUPPLYINTERID
                        WHERE t2.FSRCINTERID='{soFid}' 
                        AND t2.FSRCENTRYID in('{string.Join("','", FSRCENTRYIDList)}') 
                        AND t1.FSUPPLYFORMID='PUR_PurchaseOrder' and t3.FCANCELSTATUS='A' and t3.FDOCUMENTSTATUS = 'C'
                        group by t1.FSUPPLYINTERID,t1.FSUPPLYENTRYID,t3.FBILLNO
                        ) tt1 
                        INNER JOIN T_PUR_POORDERENTRY tt2 on tt2.FID= tt1.PoFID  and tt2.FENTRYID= tt1.PoEntryId 
                        inner join T_PUR_POORDERENTRY_R  tt3 on tt2.FENTRYID=tt3.FENTRYID";
                var poDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                foreach (var item in poDatas)
                {
                    poList.Add(new GetDirPoEntity
                    {
                        PoFID = Convert.ToInt64(item["PoFID"]),
                        PoNo = item["PoNo"].ToString(),
                        PoEntryId = Convert.ToInt64(item["PoEntryId"]),
                        PoSeqNo = Convert.ToInt32(item["PoSeqNo"]),
                        PoSumSyQty = Convert.ToDecimal(item["PoSumSyQty"])
                    });
                }
                //组装获取可直发数量
                foreach (var item in sodList)
                {
                    List<PoDirQtyDet> poDetList = new List<PoDirQtyDet>();
                    //获取预留对应的采购订单明细
                    sSql = $@"/*dialect*/SELECT distinct t1.FSUPPLYINTERID PoFID,t1.FSUPPLYENTRYID PoEntryId,t3.FBILLNO PoNo,t4.FSEQ PoSeqNo,t1.FBASEQTY,t3.FPURCHASEORGID
                                FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                INNER JOIN T_PUR_POORDER t3 on t3.FID= t1.FSUPPLYINTERID
                                INNER JOIN T_PUR_POORDERENTRY t4 on t4.FID= t1.FSUPPLYINTERID  and t4.FENTRYID= t1.FSUPPLYENTRYID
                                WHERE t2.FSRCINTERID='{soFid}' 
                                AND t2.FSRCENTRYID ='{item.SoEntryId}' 
                                AND t1.FSUPPLYFORMID='PUR_PurchaseOrder' and t3.FCANCELSTATUS='A' and t3.FDOCUMENTSTATUS = 'C' ";
                    poDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                    foreach (var poItem in poDatas)
                    {
                        //当前采购订单剩余的可出数
                        decimal poSyQty = Convert.ToDecimal(poList.Where(x => (x.PoFID.Equals(Convert.ToInt64(poItem["PoFID"])) && x.PoEntryId.Equals(Convert.ToInt64(poItem["PoEntryId"])))).FirstOrDefault()?.PoSumSyQty);
                        //直发数量
                        decimal dirQty = 0;
                        dirQty = Math.Min(Math.Min(item.FBaseCanOutQty, Convert.ToDecimal(poItem["FBASEQTY"])), poSyQty);
                        if (dirQty > 0)
                        {
                            poDetList.Add(new PoDirQtyDet
                            {
                                PoFId = Convert.ToInt64(poItem["PoFID"]),
                                PoEntryId = Convert.ToInt64(poItem["PoEntryId"]),
                                PoNo = Convert.ToString(poItem["PoNo"]),
                                PoSeqNo = Convert.ToInt32(poItem["PoSeqNo"]),
                                DirQty = dirQty,
                                ReqDirQty = 0,
                                PoOrgID = Convert.ToInt64(poItem["FPURCHASEORGID"]),
                                DirStockId = item.DirStockId
                            });
                            poList.Where(x => (x.PoFID.Equals(Convert.ToInt64(poItem["PoFID"])) && x.PoEntryId.Equals(Convert.ToInt64(poItem["PoEntryId"])))).FirstOrDefault().PoSumSyQty -= dirQty;
                        }
                    }
                    item.PoDet = poDetList;

                }
            }
            soDirEntity.SoDet = sodList;
            response.Data = soDirEntity;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            return response;
        }

        /// <summary>
        /// 采购订单下推收料通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> PoPushReceiveMaterials(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.PoPushReceiveMaterials(ctx, request);
        }

        /// <summary>
        /// 收料通知单下推采购入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> RmPushPurchasing(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.RmPushPurchasing(ctx, request);
        }

        /// <summary>
        /// 销售订单下推发货通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SoPushDelivery(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.SoPushDelivery(ctx, request);
        }
        /// <summary>
        /// 生成调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CreateDeliveryTransferOut(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.CreateDeliveryTransferOut(ctx, request);
        }
        /// <summary>
        /// 审核调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> AuditDeliveryTransferIn(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.AuditDeliveryTransferIn(ctx, request);
        }
        /// <summary>
        /// 发货通知单下推销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DnPushSalesOutStock(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.DnPushSalesOutStock(ctx, request);
        }

        /// <summary>
        /// 直发修改入库预留数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DirectEditReserved(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.DirectEditReserved(ctx, request);
        }

        /// <summary>
        /// 设置采购订单直发预留数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SetPoDirReServeQty(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            return DirectSaleServiecHelper.SetPoDirReServeQty(ctx, request);
        }
    }
}
