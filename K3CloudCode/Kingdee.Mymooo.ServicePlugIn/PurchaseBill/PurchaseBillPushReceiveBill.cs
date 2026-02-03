using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.List;
using System.Security.Cryptography;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
    [Description("采购订单下推采购收料通知单")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PurchaseBillPushReceiveBill : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");

            foreach (var headEntity in headEntitys)
            {
                //供应商信息集合
                var supplierDynamic = headEntity["SupplierId"] as DynamicObject;
                var supplierCode = "";
                if (supplierDynamic != null)
                {
                    supplierCode = supplierDynamic["Number"].ToString();
                }
                //获取明细数据
                var detDynamicObject = headEntity["PUR_ReceiveEntry"] as DynamicObjectCollection;
                long orgId = Convert.ToInt64(headEntity["STOCKORGID_Id"]);
                long fId = Convert.ToInt64(headEntity["Id"]);
                //免检供应商白名单的所有物料，并且当前SKU60天内无质量投诉类型客诉记录；
                long supplierId = Convert.ToInt64(((DynamicObject)headEntity["SupplierId"])["Id"]);
                foreach (var item in detDynamicObject)
                {
                    string itemNo = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]);
                    //全检的才需要验证
                    if (Convert.ToBoolean(item["CheckInComing"]))
                    {
                        if (orgId.Equals(7401780) || orgId.Equals(7401781))
                        {
                            //是否设置免检供应商
                            if (IsExistsSupplier(this.Context, orgId, supplierId))
                            {
                                //是否存在客诉（60天内）
                                if (!IsExistsComplaint(this.Context, itemNo))
                                {
                                    item["CheckInComing"] = false;
                                }
                            }
                            //非免检，判断评分严格度标准是否免检
                            if (Convert.ToBoolean(item["CheckInComing"]))
                            {
                                var stringencyId = 0;
                                if (item["FPARENTSMALLID"] as DynamicObject != null && item["FSMALLID"] as DynamicObject != null)
                                {
                                    stringencyId = GetStringencyId(this.Context, orgId, supplierId, Convert.ToInt64(item["FPARENTSMALLID_Id"]), Convert.ToInt64(item["FSMALLID_Id"]));
                                    //免检
                                    if (stringencyId == 5)
                                    {
                                        item["CheckInComing"] = false;
                                    }
                                }
                                //不免检的才需要放宽
                                if (Convert.ToBoolean(item["CheckInComing"]))
                                {
                                    //同一供应商同SKU(不区分组织)，前15天最新一笔检验单的SKU数量为全部合格
                                    if (IsPassRate(this.Context, supplierCode, itemNo))
                                    {
                                        //一年内没有质量投诉
                                        if (!IsExistsComplaintV2(this.Context, itemNo))
                                        {
                                            item["FCURRENTSTRINGENCY"] = 3;
                                        }
                                    }
                                }
                                //不是放宽检验的才需要其他严格度
                                if (!Convert.ToString(item["FCURRENTSTRINGENCY"]).Equals("3"))
                                {
                                    if (item["FPARENTSMALLID"] as DynamicObject != null && item["FSMALLID"] as DynamicObject != null)
                                    {
                                        item["FCURRENTSTRINGENCY"] = stringencyId;
                                    }
                                }
                            }
                        }
                        //非免检，严格度为空的，默认严格度为加严检
                        if (Convert.ToBoolean(item["CheckInComing"]) && (Convert.ToString(item["FCURRENTSTRINGENCY"]) == "" || Convert.ToString(item["FCURRENTSTRINGENCY"]).Equals("0")))
                        {
                            item["FCURRENTSTRINGENCY"] = 2;
                        }
                    }
                    item["FLABELQRCODETEXT"] = itemNo + "/" + supplierCode;
                    item["F_PENY_QRCode"] = itemNo + "/" + supplierCode;


                }
            }
        }
        //是否存在免检供应商
        public bool IsExistsSupplier(Context ctx, long orgId, long supplierId)
        {
            var sqlParams = new SqlParam[]
{
                        new SqlParam("@orgId",KDDbType.Int64,orgId),
                        new SqlParam("@supplierId",KDDbType.Int64,supplierId)
};
            var sSql = $@"/*dialect*/select count(1) from PENY_T_NoInspectSupplier where FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A' and FORGID=@orgId and FSUPPLIERID=@supplierId";
            return DBUtils.ExecuteScalar<int>(ctx, sSql, 0, sqlParams) > 0 ? true : false;
        }

        //是否存在客诉(免检供应商白名单的所有物料，并且当前SKU60天内无质量投诉类型客诉记录；)
        public bool IsExistsComplaint(Context ctx, string itemNo)
        {
            var sSql = $@"/*dialect*/select count(1) from PENY_T_CompanyComplaint where FComplaintDate>=DATEADD(DAY,-60,getdate()) and FAntComplaintModel=@itemNo ";
            return DBUtils.ExecuteScalar<int>(ctx, sSql, 0, new SqlParam("@itemNo", KDDbType.String, itemNo)) > 0 ? true : false;
        }

        //同一供应商同SKU(不区分组织)，前15天最新一笔检验单的SKU数量为全部合格（如果前15天没有数据，则不放宽）
        public bool IsPassRate(Context ctx, string supplierCode, string itemNo)
        {
            var sqlParams = new SqlParam[]
{
                        new SqlParam("@supplierCode",KDDbType.String,supplierCode),
                        new SqlParam("@itemNo",KDDbType.String,itemNo)
};
            var sSql = $@"/*dialect*/select count(1) from T_QM_INSPECTBILL t1
						inner join T_QM_INSPECTBILLENTRY t2 on t2.FID=t1.FID
						inner join T_QM_INSPECTBILLENTRY_A t3 on t2.FENTRYID=t3.FENTRYID
						inner join T_BD_MATERIAL t4 on t3.FMaterialId=t4.FMATERIALID
						inner join t_BD_Supplier t5 on t2.FSUPPLIERID=t5.FSUPPLIERID
						where t1.FDOCUMENTSTATUS='C' and t1.FDATE>=DATEADD(DAY,-15,getdate()) 
						and t5.FNUMBER=@supplierCode and t4.FNUMBER=@itemNo  ";
            if (DBUtils.ExecuteScalar<int>(ctx, sSql, 0, sqlParams) > 0)
            {
                sSql = $@"/*dialect*/select top 1 t3.FINSPECTRESULT,t2.FINSPECTQTY,t2.FQUALIFIEDQTY,t2.FUNQUALIFIEDQTY from T_QM_INSPECTBILL t1
						inner join T_QM_INSPECTBILLENTRY t2 on t2.FID=t1.FID
						inner join T_QM_INSPECTBILLENTRY_A t3 on t2.FENTRYID=t3.FENTRYID
						inner join T_BD_MATERIAL t4 on t3.FMaterialId=t4.FMATERIALID
						inner join t_BD_Supplier t5 on t2.FSUPPLIERID=t5.FSUPPLIERID
						where t1.FDOCUMENTSTATUS='C' and t1.FDATE>=DATEADD(DAY,-15,getdate()) 
						and t5.FNUMBER=@supplierCode and t4.FNUMBER=@itemNo
						order by t1.FDATE desc,t2.FSEQ desc ";
                var datas = DBUtils.ExecuteDynamicObject(ctx, sSql, paramList: sqlParams);
                //检验结果合格
                if (Convert.ToString(datas[0]["FINSPECTRESULT"]).Equals("1"))
                {
                    //不合格数
                    if (Convert.ToDecimal(datas[0]["FUNQUALIFIEDQTY"]) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //是否存在质量投诉(一年内)
        public bool IsExistsComplaintV2(Context ctx, string itemNo)
        {
            var sSql = $@"/*dialect*/select count(1) from PENY_T_CompanyComplaint where FComplaintDate>=DATEADD(YEAR,-1,getdate()) and FAntComplaintModel=@itemNo ";
            return DBUtils.ExecuteScalar<int>(ctx, sSql, 0, new SqlParam("@itemNo", KDDbType.String, itemNo)) > 0 ? true : false;
        }

        /// <summary>
        /// 获取当前严格度
        /// </summary>
        /// <returns></returns>
        public int GetStringencyId(Context ctx, long orgId, long supplierId, long parentSmallId, long smallId)
        {
            var sqlParams = new SqlParam[]
      {
                        new SqlParam("@orgId",KDDbType.Int64,orgId),
                        new SqlParam("@supplierId",KDDbType.Int64,supplierId),
                        new SqlParam("@parentSmallId",KDDbType.Int64,parentSmallId),
                        new SqlParam("@smallId",KDDbType.Int64,smallId)
      };
            var sSql = $@"/*dialect*/select top 1 FStringencyId from PENY_T_SupplierClassInspectScore where FORGID=@orgId and FSUPPLIERID=@supplierId 
            and FParentSmallId=@parentSmallId and FSmallId=@smallId ";
            return DBUtils.ExecuteScalar<int>(ctx, sSql, 0, sqlParams);
        }
    }
}
