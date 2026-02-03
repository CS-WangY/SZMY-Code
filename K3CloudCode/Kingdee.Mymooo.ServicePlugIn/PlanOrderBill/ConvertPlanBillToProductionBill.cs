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
using System.Diagnostics;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    /// <summary>
    /// 计划转生产
    /// </summary>
    [Description("销售单计划订单转生产带采购备注插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ConvertPlanBillToProductionBill : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                foreach (var item in headEntity.DataEntity["TreeEntity"] as DynamicObjectCollection)
                {
                    //销售订单
                    if (item["ReqSrc"].ToString() == "1")
                    {
                        var saleNo = item["SaleOrderNo"].ToString();
                        var saleEntSeq = Convert.ToInt32(item["SaleOrderEntrySeq"]);

                        if (!string.IsNullOrWhiteSpace(saleNo))
                        {
                            List<SqlParam> pars = new List<SqlParam>() {
                        new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                        new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq) };
                            var sql = $@"SELECT FINSIDEREMARK FROM T_SAL_ORDER SAL
                                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                                        WHERE SAL.FBILLNO=@FSALEORDERNO AND SALDES.FSEQ=@FSALEORDERENTRYSEQ ";
                            var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                            foreach (var data in datas)
                            {
                                item["Memo"] = data["FINSIDEREMARK"];
                            }
                        }
                        long orgId = Convert.ToInt64(headEntity["PrdOrgId_id"]);
                        var itemNo = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"].ToString());
                        //如果客户为空，取该物料最近一笔组织间需求的客户
                        if (item["FPENYCustomerID"] == null)
                        {
                            item["FPENYCustomerID_Id"] = GetCustomerID(orgId, itemNo);
                        }
                        //如果含税单价为0，则取最近一笔销售订单价格
                        if (Convert.ToDecimal(item["FPENYPrice"]) == 0)
                        {
                            item["FPENYPrice"] = GetSoPrice(orgId, itemNo);
                        }
                    }
                    else if (item["ReqSrc"].ToString() == "2")
                    {
                        //预测单
                        var saleNo = item["SaleOrderNo"].ToString();
                        var saleEntSeq = Convert.ToInt32(item["SaleOrderEntrySeq"]);
                        if (!string.IsNullOrWhiteSpace(saleNo))
                        {
                            List<SqlParam> pars = new List<SqlParam>() {
                            new SqlParam("@FSALEORDERNO", KDDbType.String, saleNo),
                            new SqlParam("@FSALEORDERENTRYSEQ", KDDbType.Int32, saleEntSeq)
                            };
                            var sql = $@"/*dialect*/select t3.FDESCRIPTION from T_PLN_FORECAST t1
                                         inner join T_PLN_FORECASTENTRY t2 on t1.FID=t2.FID
                                         left join T_PLN_FORECASTENTRY_L t3 on t2.FENTRYID=t3.FENTRYID and t3.FLOCALEID=2052
                                         where t1.FBILLNO=@FSALEORDERNO and t2.FSEQ=@FSALEORDERENTRYSEQ ";
                            var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                            foreach (var data in datas)
                            {
                                item["Memo"] = data["FDESCRIPTION"];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 计划订单合并或者手工下生产订单，客户取不到就取该物料最近一笔组织间需求的客户
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        private long GetCustomerID(long orgId, string materialNo)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@OrgId", KDDbType.Int64, orgId), new SqlParam("@MaterialNo", KDDbType.String, materialNo) };
            var sql = $@"/*dialect*/select top 1 t1.FPENYCustomerID from T_PLN_REQUIREMENTORDER t1
                        inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
                        where t1.FSUPPLYORGID=@OrgId and t2.FNUMBER=@MaterialNo and t1.FDOCUMENTSTATUS='C' and t1.FPENYCustomerID>0
                        order by t1.FCREATEDATE desc";
            return DBUtils.ExecuteScalar<long>(this.Context, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 取销售含税单价
        /// </summary>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        private decimal GetSoPrice(long orgId, string materialNo)
        {
            //半成品取成品的价格
            var index = materialNo.LastIndexOf("-W-1-");
            if (index > 0)
            {
                materialNo = materialNo.Substring(0, index);
            }

            //1、先取组织间需求单
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@OrgId", KDDbType.Int64, orgId), new SqlParam("@MaterialNo", KDDbType.String, materialNo) };
            var sql = $@"/*dialect*/select top 1 F_PENY_PRICE from T_PLN_REQUIREMENTORDER t1
                            inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
                            where FSUPPLYORGID=@OrgId and t2.FNUMBER=@MaterialNo and t1.FDOCUMENTSTATUS='C' and F_PENY_PRICE>0
                            order by t1.FAPPROVEDATE desc ";
            decimal price = DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, paramList: pars.ToArray());
            if (price > 0)
            {
                return price;
            }
            //2、取不到则取华东五部的销售订单
            sql = $@"/*dialect*/SELECT TOP 1 TEF.FTAXPRICE FROM T_SAL_ORDER SAL
                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                        INNER JOIN T_SAL_ORDERENTRY_F TEF ON SALDES.FENTRYID=TEF.FENTRYID
                        INNER JOIN T_BD_MATERIAL MAT on SALDES.FMATERIALID=MAT.FMATERIALID
                        WHERE SAL.FSaleOrgId=7401803 and SALDES.FSUPPLYTARGETORGID=@OrgId AND MAT.FNUMBER=@MaterialNo 
                        AND SAL.FDOCUMENTSTATUS='C' AND TEF.FTAXPRICE>0  ORDER BY SAL.FAUDITTIME DESC ";
            return DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, paramList: pars.ToArray());
        }
    }
}
