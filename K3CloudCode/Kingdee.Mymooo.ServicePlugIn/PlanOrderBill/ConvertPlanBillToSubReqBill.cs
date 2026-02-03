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

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    /// <summary>
    /// 计划转委外
    /// </summary>
    [Description("销售单计划订单转委外带供应商")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ConvertPlanBillToSubReqBill : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                foreach (var item in headEntity.DataEntity["TreeEntity"] as DynamicObjectCollection)
                {

                    var saleNo = Convert.ToString(item["SALEORDERNO"]);
                    var saleEntSeq = Convert.ToInt32(item["SaleOrderEntrySeq"]);
                    //正常下推，委外不存在合并订单
                    if (!string.IsNullOrWhiteSpace(saleNo) && saleEntSeq != 0)
                    {
                        List<SqlParam> pars = new List<SqlParam>() {
                                new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                                new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq) };
                        var sql = $@"/*dialect*/SELECT SALDES.FSUPPLIERID,SUPP.FNUMBER SupplierCode FROM T_SAL_ORDER SAL
                                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
										LEFT JOIN t_BD_Supplier SUPP ON SUPP.FSUPPLIERID=SALDES.FSUPPLIERID 
                                        WHERE SAL.FBILLNO=@FSALEORDERNO AND SALDES.FSEQ=@FSALEORDERENTRYSEQ ";
                        var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                        foreach (var data in datas)
                        {
                            long purchaseOrgSupplierId = 0;
                            if (Convert.ToInt64(data["FSUPPLIERID"]) > 0)
                            {
                                purchaseOrgSupplierId = GetSupplierId(this.Context, Convert.ToInt64(item["PurorgId_Id"]), Convert.ToString(data["SupplierCode"]));
                            }
                            item["SupplierId_Id"] = purchaseOrgSupplierId;

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
