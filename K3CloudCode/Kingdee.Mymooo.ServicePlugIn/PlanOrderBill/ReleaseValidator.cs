using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    public class ReleaseValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            foreach (var headEntity in dataEntities)
            {
                var dynamicObject = headEntity.DataEntity as DynamicObject;
                if (Convert.ToBoolean(dynamicObject["FIsSendCNCMES"]))
                {
                    validateContext.AddError(headEntity, new ValidationErrorInfo(
                                                                    string.Empty,
                                                                    headEntity["Id"].ToString(),
                                                                    headEntity.DataEntityIndex,
                                                                    headEntity.RowIndex,
                                                                    headEntity["Id"].ToString(),
                                                                    string.Format("单据编号[{0}]不允许投放!", headEntity["BillNo"]),
                                                                    $"投放[{headEntity["BillNo"]}]",
                                                                    ErrorLevel.FatalError));
                    continue;
                }
                //需求来源（销售订单）
                if (Convert.ToString(dynamicObject["DEMANDTYPE"]) == "1")
                {
                    var saleNo = Convert.ToString(headEntity["SaleOrderNo"]);
                    var saleEntSeq = Convert.ToInt32(headEntity["SaleOrderEntrySeq"]);
                    //正常下推
                    if (!string.IsNullOrWhiteSpace(saleNo) && saleEntSeq != 0)
                    {
                        List<SqlParam> pars = new List<SqlParam>() {
                                new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                                new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq) };
                        var sql = $@"/*dialect*/SELECT SALDES.FSupplyTargetOrgId,ORGL.FNAME OrgName,SALDES.FSUPPLIERID,SUPP.FNUMBER SupplierCode,SUPPL.FNAME SupplierName FROM T_SAL_ORDER SAL
                                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
										LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=SALDES.FSUPPLIERID 
										LEFT JOIN T_BD_SUPPLIER_L SUPPL ON SUPP.FSUPPLIERID=SUPPL.FSUPPLIERID AND SUPPL.FLOCALEID=2052
										LEFT JOIN T_ORG_ORGANIZATIONS_L ORGL ON ORGL.FORGID=SALDES.FSupplyTargetOrgId AND ORGL.FLOCALEID=2052
                                        WHERE SAL.FBILLNO=@FSALEORDERNO AND SALDES.FSEQ=@FSALEORDERENTRYSEQ ";
                        var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                        foreach (var data in datas)
                        {
                            if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                            {
                                //只有FA才需要验证是否已经分配供应商
                                if (Convert.ToInt64(data["FSupplyTargetOrgId"]) == 7401780)
                                {
                                    long purchaseOrgSupplierId = GetSupplierId(this.Context, Convert.ToInt64(data["FSupplyTargetOrgId"]), Convert.ToString(data["SupplierCode"]));
                                    if (purchaseOrgSupplierId == 0)
                                    {
                                        validateContext.AddError(data, new ValidationErrorInfo(
                                                                        string.Empty,
                                                                        headEntity["Id"].ToString(),
                                                                        headEntity.DataEntityIndex,
                                                                        headEntity.RowIndex,
                                                                        headEntity["Id"].ToString(),
                                                                        string.Format("单据编号[{0}]的组织[{1}]不含供应商[{2}]", headEntity["BillNo"], data["OrgName"], data["SupplierName"]),
                                                                        $"投放[{headEntity["BillNo"]}]",
                                                                        ErrorLevel.FatalError));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //判断合并订单
                        //计划订单合并逻辑：合并后，销售取（销售订单单号，行号，单价，备注），以销售价最低行数据匹配。
                        //供应取（供应商，供应商单价，供应商型号）：以供应商单价最低行匹配
                        if (((DynamicObjectCollection)dynamicObject["FBillHead_Link"]) != null)
                        {
                            //(计划订单FId)
                            var fid = Convert.ToString(dynamicObject["Id"]);
                            if (!string.IsNullOrWhiteSpace(fid))
                            {
                                //获取合并的订单信息
                                List<SqlParam> pars = new List<SqlParam>() {
                                new SqlParam("@FID", KDDbType.Int64, fid) };
                                var sql = $@"/*dialect*/SELECT TOP 1 SALDES.FSupplyTargetOrgId,ORGL.FNAME OrgName,SALDES.FSUPPLIERID,SUPP.FNUMBER SupplierCode,SUPPL.FNAME SupplierName 
                                            FROM T_SAL_ORDER SAL
                                            INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                                            LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=SALDES.FSUPPLIERID 
                                            LEFT JOIN T_BD_SUPPLIER_L SUPPL ON SUPP.FSUPPLIERID=SUPPL.FSUPPLIERID AND SUPPL.FLOCALEID=2052
                                            LEFT JOIN T_ORG_ORGANIZATIONS_L ORGL ON ORGL.FORGID=SALDES.FSupplyTargetOrgId AND ORGL.FLOCALEID=2052
                                            WHERE  SALDES.FSupplierUnitPrice>0 AND EXISTS (
                                            SELECT t2.FSALEORDERNO,t2.FSALEORDERENTRYSEQ FROM T_PLN_PLANORDER_LK t1
                                            INNER JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=t2.FID
                                            WHERE t1.FID=@FID AND SAL.FBILLNO=t2.FSALEORDERNO AND SALDES.FSEQ=t2.FSALEORDERENTRYSEQ
                                            ) ORDER BY FSupplierUnitPrice";
                                var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                                foreach (var data in datas)
                                {
                                    if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                                    {
                                        //只有FA才需要验证是否已经分配供应商
                                        if (Convert.ToInt64(data["FSupplyTargetOrgId"]) == 7401780)
                                        {
                                            long purchaseOrgSupplierId = GetSupplierId(this.Context, Convert.ToInt64(data["FSupplyTargetOrgId"]), Convert.ToString(data["SupplierCode"]));
                                            if (purchaseOrgSupplierId == 0)
                                            {
                                                validateContext.AddError(data, new ValidationErrorInfo(
                                                                                string.Empty,
                                                                                headEntity["Id"].ToString(),
                                                                                headEntity.DataEntityIndex,
                                                                                headEntity.RowIndex,
                                                                                headEntity["Id"].ToString(),
                                                                                string.Format("单据编号[{0}]的组织[{1}]不含供应商[{2}]", headEntity["BillNo"], data["OrgName"], data["SupplierName"]),
                                                                                $"投放[{headEntity["BillNo"]}]",
                                                                                ErrorLevel.FatalError));
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
            var sql = $@"select top 1 FSUPPLIERID from t_BD_Supplier where FUSEORGID=@UseOrgId and  FNUMBER=@SupplierCode ";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
