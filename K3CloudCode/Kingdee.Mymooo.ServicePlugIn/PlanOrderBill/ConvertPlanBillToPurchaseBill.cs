using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Validation;
using System.Web.UI.WebControls;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    /// <summary>
    /// 计划转采购
    /// </summary>
    [Description("销售单计划订单转采购申请带采购备注插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ConvertPlanBillToPurchaseBill : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                foreach (var item in headEntity.DataEntity["ReqEntry"] as DynamicObjectCollection)
                {
                    var purOrgId = Convert.ToInt64(item["PurchaseOrgId_Id"]);
                    if (((DynamicObjectCollection)item["FEntity_Link"]) != null)
                    {
                        //预测单
                        if (item["DEMANDTYPE"].ToString() == "2")
                        {
                            var saleNo = Convert.ToString(item["DEMANDBILLNO"]);
                            var saleEntSeq = Convert.ToInt32(item["DEMANDBILLENTRYSEQ"]);
                            if (!string.IsNullOrWhiteSpace(saleNo))
                            {
                                List<SqlParam> pars = new List<SqlParam>() {
                                new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                                new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq)
                                };
                                var sql = $@"/*dialect*/select t3.FDESCRIPTION,FCUSTITEMNO,FCUSTITEMNAME from T_PLN_FORECAST t1
                                         inner join T_PLN_FORECASTENTRY t2 on t1.FID=t2.FID
                                         left join T_PLN_FORECASTENTRY_L t3 on t2.FENTRYID=t3.FENTRYID and t3.FLOCALEID=2052
                                         where t1.FBILLNO=@FSALEORDERNO and t2.FSEQ=@FSALEORDERENTRYSEQ ";
                                var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                                foreach (var data in datas)
                                {
                                    item["EntryNote"] = data["FDESCRIPTION"];
                                    item["FCustItemNo"] = Convert.ToString(data["FCUSTITEMNO"]);
                                    item["FCustItemName"] = Convert.ToString(data["FCUSTITEMNAME"]);
                                }
                            }
                        }
                        else
                        {
                            //需求单FID(计划订单FId)
                            var fid = Convert.ToString(((DynamicObjectCollection)item["FEntity_Link"])[0]["SId"]);
                            var itemNo = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"].ToString());
                            if (!string.IsNullOrWhiteSpace(fid))
                            {
                                //判断合并订单
                                //计划订单合并逻辑：合并后，销售取（销售订单单号，行号，单价，备注），以销售价最低行数据匹配。
                                //供应取（供应商，供应商单价，供应商型号）：以供应商单价最低行匹配
                                if (GetPlnMerge(this.Context, Int64.Parse(fid)) > 0)
                                {
                                    //获取合并的订单信息
                                    List<SqlParam> pars = new List<SqlParam>() {
                                new SqlParam("@FID", KDDbType.Int64, fid),
                                new SqlParam("@ItemNo", KDDbType.String, itemNo)};
                                    var sql = $@"/*dialect*/SELECT * FROM (
			                                    select top 1* from (
			                                          SELECT  SAL.FBILLNO,SALDES.FSEQ,FINSIDEREMARK,FTAXPRICE FROM T_SAL_ORDER SAL
			                                          INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
			                                          INNER JOIN T_SAL_ORDERENTRY_F TEF ON SALDES.FENTRYID=TEF.FENTRYID
													  INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
			                                          WHERE  EXISTS (
			                                          SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
			                                          INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=convert(nvarchar(20),t2.FID)
													  INNER JOIN T_PLN_PLANORDER t3 ON t3.FID=t1.FID
			                                          WHERE t1.FID=@FID AND SAL.FBILLNO=t2.FSALEORDERNO AND SALDES.FSEQ=t2.FSALEORDERENTRYSEQ and MAT.FNUMBER=@ItemNo)
			                                          union
			                                          SELECT  case when pln.FDEMANDTYPE='0' then  pln.FBILLNO else pln.FSALEORDERNO end FSALEORDERNO,
			                                          pln.FSALEORDERENTRYSEQ,
			                                          pln2.FINSIDEREMARK,pln.F_PENY_Price FTAXPRICE FROM T_PLN_REQUIREMENTORDER  pln
                                                      inner join T_PLN_REQUIREMENTORDER_L pln2 on pln.FID=pln2.FID
													  INNER JOIN T_BD_MATERIAL MAT on pln.FMATERIALID=MAT.FMATERIALID
			                                          WHERE  pln.FDEMANDTYPE in ('0','8')  AND EXISTS (
			                                          SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
			                                          INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=convert(nvarchar(20),t2.FID)
													  INNER JOIN T_PLN_PLANORDER t3 ON t3.FID=t1.FID
			                                          WHERE t1.FID=@FID 
			                                          AND (case when pln.FDEMANDTYPE='0' then  pln.FBILLNO else pln.FSALEORDERNO end=t2.FSALEORDERNO) 
			                                          AND pln.FSALEORDERENTRYSEQ=t2.FSALEORDERENTRYSEQ  and MAT.FNUMBER=@ItemNo)
				                                        ) tt1  ORDER BY FTAXPRICE
                                                      ) datas
                                                      LEFT JOIN ( 
			                                        select top 1* from (
				                                    SELECT  FSupplierProductCode,SALDES.FSUPPLIERID,SALDES.FSupplierUnitPrice,SUPP.FNUMBER SupplierCode FROM T_SAL_ORDER SAL
				                                    INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
				                                    LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=SALDES.FSUPPLIERID
													INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
				                                    WHERE  SALDES.FSupplierUnitPrice>0 AND EXISTS (
				                                    SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
				                                    INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=convert(nvarchar(20),t2.FID)
													INNER JOIN T_PLN_PLANORDER t3 ON t3.FID=t1.FID
				                                    WHERE t1.FID=@FID AND SAL.FBILLNO=t2.FSALEORDERNO AND SALDES.FSEQ=t2.FSALEORDERENTRYSEQ and MAT.FNUMBER=@ItemNo)	
				                                    union
				                                    SELECT pln.FSUPPLIERPRODUCTCODE,pln.FSUPPLIERID,pln.FSUPPLIERUNITPRICE,isnull(SUPP.FNUMBER,'') SupplierCode FROM T_PLN_REQUIREMENTORDER  pln
				                                    LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=pln.FSUPPLIERID
													INNER JOIN T_BD_MATERIAL MAT on pln.FMATERIALID=MAT.FMATERIALID
				                                    WHERE pln.FSUPPLIERUNITPRICE>0 and pln.FDEMANDTYPE in ('0','8')  AND EXISTS (
				                                    SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
				                                    INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=convert(nvarchar(20),t2.FID)
													INNER JOIN T_PLN_PLANORDER t3 ON t3.FID=t1.FID
				                                    WHERE t1.FID=@FID 
				                                    AND (case when pln.FDEMANDTYPE='0' then  pln.FBILLNO else pln.FSALEORDERNO end=t2.FSALEORDERNO) 
				                                    AND pln.FSALEORDERENTRYSEQ=t2.FSALEORDERENTRYSEQ and MAT.FNUMBER=@ItemNo)
				                                    ) datas ORDER BY FSupplierUnitPrice
                                                    ) datas2 ON 1=1
                                                    left join (
			                                          SELECT top 1 FCustItemNo,FCustItemName FROM T_SAL_ORDER SAL
			                                          INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
			                                          INNER JOIN T_SAL_ORDERENTRY_F TEF ON SALDES.FENTRYID=TEF.FENTRYID
													  INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
			                                          WHERE  EXISTS (
			                                          SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
			                                          INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=convert(nvarchar(20),t2.FID)
													  INNER JOIN T_PLN_PLANORDER t3 ON t3.FID=t1.FID
			                                          WHERE t1.FID=@FID AND SAL.FBILLNO=t2.FSALEORDERNO AND SALDES.FSEQ=t2.FSALEORDERENTRYSEQ and MAT.FNUMBER=@ItemNo)
                                                      ) datas3 ON 1=1

";
                                    var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                                    foreach (var data in datas)
                                    {
                                        long purchaseOrgSupplierId = 0;
                                        if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                                        {
                                            purchaseOrgSupplierId = GetSupplierId(this.Context, purOrgId, Convert.ToString(data["SupplierCode"]));
                                        }
                                        else
                                        {
                                            //找不到供应商，则取该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单）
                                            purchaseOrgSupplierId = GetNewSupplierId(this.Context, purOrgId, itemNo);
                                        }
                                        item["FSoNo"] = Convert.ToString(data["FBILLNO"]);
                                        item["FSoSeq"] = Convert.ToInt32(data["FSEQ"]);
                                        item["FSoUnitPrice"] = Convert.ToDecimal(data["FTAXPRICE"]);
                                        item["EntryNote"] = Convert.ToString(data["FINSIDEREMARK"]);
                                        item["SuggestSupplierId_Id"] = purchaseOrgSupplierId;
                                        item["FSupplierUnitPrice"] = Convert.ToDecimal(data["FSupplierUnitPrice"]);
                                        item["FSupplierProductCode"] = Convert.ToString(data["FSupplierProductCode"]);
                                        item["FCustItemNo"] = Convert.ToString(data["FCustItemNo"]);
                                        item["FCustItemName"] = Convert.ToString(data["FCustItemName"]);
                                    }
                                }
                                else
                                {
                                    var saleNo = Convert.ToString(item["DEMANDBILLNO"]);
                                    var saleEntSeq = Convert.ToInt32(item["DEMANDBILLENTRYSEQ"]);
                                    //需求来源(销售订单)
                                    if (Convert.ToString(item["DEMANDTYPE"]) == "1")
                                    {
                                        //正常下推
                                        if (!string.IsNullOrWhiteSpace(saleNo) && saleEntSeq != 0)
                                        {
                                            List<SqlParam> pars = new List<SqlParam>() {
                                        new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                                        new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq),
                                        new SqlParam("@ItemNo", KDDbType.String, itemNo)};
                                            var sql = $@"/*dialect*/SELECT FINSIDEREMARK,FSupplierProductCode,SALDES.FSUPPLIERID,SALDES.FSupplierUnitPrice,SAL.FBILLNO,
                                        SALDES.FSEQ,FTAXPRICE,SUPP.FNUMBER SupplierCode,FCustItemNo,FCustItemName FROM T_SAL_ORDER SAL
                                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                                        INNER JOIN T_SAL_ORDERENTRY_F TEF ON SALDES.FENTRYID=TEF.FENTRYID
										LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=SALDES.FSUPPLIERID
										INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
                                        WHERE SAL.FBILLNO=@FSALEORDERNO AND SALDES.FSEQ=@FSALEORDERENTRYSEQ AND MAT.FNUMBER=@ItemNo ";
                                            var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                                            foreach (var data in datas)
                                            {
                                                long purchaseOrgSupplierId = 0;
                                                if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                                                {
                                                    purchaseOrgSupplierId = GetSupplierId(this.Context, purOrgId, Convert.ToString(data["SupplierCode"]));
                                                }
                                                else
                                                {
                                                    //找不到供应商，则取该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单）
                                                    purchaseOrgSupplierId = GetNewSupplierId(this.Context, purOrgId, itemNo);
                                                }
                                                item["FSoNo"] = Convert.ToString(data["FBILLNO"]);
                                                item["FSoSeq"] = Convert.ToInt32(data["FSEQ"]);
                                                item["FSoUnitPrice"] = Convert.ToDecimal(data["FTAXPRICE"]);
                                                item["EntryNote"] = Convert.ToString(data["FINSIDEREMARK"]);
                                                item["SuggestSupplierId_Id"] = purchaseOrgSupplierId;
                                                item["FSupplierUnitPrice"] = Convert.ToDecimal(data["FSupplierUnitPrice"]);
                                                item["FSupplierProductCode"] = Convert.ToString(data["FSupplierProductCode"]);
                                                item["FCustItemNo"] = Convert.ToString(data["FCustItemNo"]);
                                                item["FCustItemName"] = Convert.ToString(data["FCustItemName"]);
                                            }
                                        }
                                    }
                                    else if (Convert.ToString(item["DEMANDTYPE"]) == "0" || Convert.ToString(item["DEMANDTYPE"]) == "8")//组织间需求手工或者报价单
                                    {
                                        //正常下推
                                        if (!string.IsNullOrWhiteSpace(saleNo))
                                        {
                                            List<SqlParam> pars = new List<SqlParam>() {
                                        new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                                        new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq),
                                        new SqlParam("@ItemNo", KDDbType.String, itemNo)};
                                            var sql = $@"/*dialect*/select case when t1.FDEMANDTYPE='0' then  t1.FBILLNO else t1.FSALEORDERNO end FBILLNO,
                                                                t1.FSALEORDERENTRYSEQ  FSEQ,
                                                                t1.F_PENY_Price FTAXPRICE,pln.FINSIDEREMARK,t1.FSUPPLIERID,t1.FSUPPLIERUNITPRICE,t1.FSUPPLIERPRODUCTCODE,
                                                                isnull(SUPP.FNUMBER,'') SupplierCode
                                                                from T_PLN_REQUIREMENTORDER  t1
                                                                LEFT join T_PLN_REQUIREMENTORDER_L pln on t1.FID=pln.FID
                                                                LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=t1.FSUPPLIERID
                                                                INNER JOIN T_BD_MATERIAL MAT on t1.FMATERIALID=MAT.FMATERIALID
                                                                where FDEMANDTYPE in ('0','8')  
                                                                and ((t1.FBILLNO=@FSALEORDERNO and t1.FSALEORDERENTRYSEQ=@FSALEORDERENTRYSEQ) 
                                                                or (t1.FSALEORDERNO=@FSALEORDERNO AND t1.FSALEORDERENTRYSEQ=@FSALEORDERENTRYSEQ)) AND MAT.FNUMBER=@ItemNo ";
                                            var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                                            foreach (var data in datas)
                                            {
                                                long purchaseOrgSupplierId = 0;

                                                if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                                                {
                                                    purchaseOrgSupplierId = GetSupplierId(this.Context, purOrgId, Convert.ToString(data["SupplierCode"]));
                                                }
                                                else
                                                {
                                                    //找不到供应商，则取该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单）
                                                    purchaseOrgSupplierId = GetNewSupplierId(this.Context, purOrgId, itemNo);
                                                }

                                                item["FSoNo"] = Convert.ToString(data["FBILLNO"]);
                                                item["FSoSeq"] = Convert.ToInt32(data["FSEQ"]);
                                                item["FSoUnitPrice"] = Convert.ToDecimal(data["FTAXPRICE"]);
                                                item["EntryNote"] = Convert.ToString(data["FINSIDEREMARK"]);
                                                item["SuggestSupplierId_Id"] = purchaseOrgSupplierId;
                                                item["FSupplierUnitPrice"] = Convert.ToDecimal(data["FSupplierUnitPrice"]);
                                                item["FSupplierProductCode"] = Convert.ToString(data["FSupplierProductCode"]);
                                            }
                                        }
                                    }
                                }

                                //华东五部的生产订单需要携带BOM备注，销售的采购备注+生产BOM备注
                                if (item["DEMANDTYPE"].ToString() == "5" && Convert.ToInt64(item["PurchaseOrgId_Id"]) == 7401803)
                                {
                                    item["EntryNote"] = GetPlnDescription(this.Context, Int64.Parse(fid));
                                }
                            }
                        }
                    }
                    //华东五部取消建议供应商
                    if (purOrgId == 7401803)
                    {
                        item["SuggestSupplierId_Id"] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 根据供应商编号获取供应商ID
        /// </summary>
        /// <param name="SupplierCode"></param>
        /// <returns></returns>
        private long GetSupplierId(Context ctx, long useOrgId, string supplierCode)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UseOrgId", KDDbType.Int64, useOrgId) ,
                new SqlParam("@SupplierCode", KDDbType.String, supplierCode) };
            var sql = $@"select top 1 FSUPPLIERID from t_BD_Supplier where FUSEORGID=@UseOrgId and  FNUMBER=@SupplierCode  and FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A' ";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 判断计划订单是否合并
        /// </summary>
        /// <param name="FID"></param>
        /// <returns></returns>
        private int GetPlnMerge(Context ctx, long FID)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, FID) };
            var sql = $@"select count(1) from T_PLN_PLANORDER_LK where FID=@FID";
            return DBUtils.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
        }


        /// <summary>
        /// 该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单）
        /// 携带的供应商没有才取这个
        /// </summary>
        /// <param name="purchaseOrgId"></param>
        /// <returns></returns>
        private long GetNewSupplierId(Context ctx, long purchaseOrgId, string itemNo)
        {
            //华东五部的不携带
            if (purchaseOrgId == 7401803)
            {
                return 0;
            }
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@PurchaseOrgId", KDDbType.Int64, purchaseOrgId),
            new SqlParam("@ItemNo", KDDbType.String, itemNo)};
            var sql = $@"/*dialect*/select top 1 t1.FSupplierId from T_PUR_POORDER t1
                        inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
                        inner join T_BD_MATERIAL t3  on t2.FMATERIALID=t3.FMATERIALID
                        where t1.FPurchaseOrgId=@PurchaseOrgId and t1.FDOCUMENTSTATUS='C' and t1.FDATE>=dateadd(year,-1,getdate()) and t3.FNUMBER=@ItemNo
                        order by t1.FDATE desc ";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 华东五部获取采购备注和BOM备注
        /// </summary>
        /// <param name="FID"></param>
        /// <returns></returns>
        private string GetPlnDescription(Context ctx, long FID)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, FID) };
            var sql = $@"/*dialect*/select top 1 case when isnull(FINSIDEREMARK,'')!='' then isnull(FINSIDEREMARK,'')+'；'+isnull(FDescription,'') else isnull(FDescription,'') end from T_PLN_PLANORDER_L where FID=@FID and FLOCALEID=2052";
            return DBUtils.ExecuteScalar<string>(ctx, sql, "", paramList: pars.ToArray());
        }
    }
}
