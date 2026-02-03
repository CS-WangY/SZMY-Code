using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
    [Description("供应商回复交期保存插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SaveSuplierDeliveryToSales : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FREMARKS");
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FTRACKINGID");
            e.FieldKeys.Add("FTRACKINGNUMBER");
        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            SaveSuplierDeliveryValidator isPoValidator = new SaveSuplierDeliveryValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }
        /// <summary>
        /// 供应商回复交期保存更新销售单插件
        /// </summary>
        /// <param name="e"></param>
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            var dataEntities = e.DataEntitys;
            foreach (var item in dataEntities)
            {
                var purchaseEntryId = Convert.ToInt64(item["FPurchaseEntryId"]);
                var date = Convert.ToDateTime(item["FDATE"]);
                var remarks = Convert.ToString(item["FREMARKS"]);
                var trackingNumber = Convert.ToString(item["FTrackingNumber"]).Trim().Replace(" ", "");
                if (item["FTrackingID"] != null)
                {
                    var trackingName = Convert.ToString(((DynamicObject)item["FTrackingID"])["FDataValue"].ToString());
                    remarks = remarks + $" ({trackingName}：{trackingNumber})";
                }
                var isBeOverdue = Convert.ToString(item["FIsBeOverdue"]);
                List<SqlParam> pars = new List<SqlParam>() {
                    new SqlParam("@ENTRYID", KDDbType.Int64, purchaseEntryId),
                    new SqlParam("@SUPPLIERDESCRIPTIONS", KDDbType.String, remarks),
                    new SqlParam("@SUPPLIERREPLYDATE", KDDbType.DateTime, date),};

                //更新对应的销售订单的供应商回复交期
                var updateSql = $@"/*dialect*/update T_SAL_ORDERENTRY set FSUPPLIERREPLYDATE=@SUPPLIERREPLYDATE,FSUPPLIERDESCRIPTIONS=@SUPPLIERDESCRIPTIONS where FENTRYID in (
	                                                                                select t2.FDEMANDBILLENTRYID
                                                                                    from
                                                                                    T_PUR_POORDERENTRY t1
                                                                                    left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
						                                                            where t2.FDEMANDBILLENTRYID>0  and t1.FENTRYID=@ENTRYID
							                                                        union
							                                                        select t8.FSALEORDERENTRYID FDEMANDBILLENTRYID
                                                                                    from T_PUR_POORDERENTRY t1
							                                                        left join T_PUR_POORDERENTRY_LK t5 on t1.FENTRYID=t5.FENTRYID
							                                                        left join T_PUR_REQENTRY_LK t6 on t6.FENTRYID=t5.FSID
							                                                        left join T_PLN_PLANORDER_LK t7 on t6.FSID=t7.FID
                                                                                    left JOIN T_PLN_PLANORDER_B t8 ON t7.FSBILLID=t8.FID
							                                                        where t8.FSALEORDERENTRYID>0  and t1.FENTRYID=@ENTRYID ) ";
                DBUtils.Execute(this.Context, updateSql, pars);
                if (isBeOverdue.EqualsIgnoreCase("True"))
                {
                    //获取采购订单信息
                    var poSql = $@"/*dialect*/select t3.FBILLNO+'-'+CONVERT(nvarchar(10),t1.FSEQ) PoNo,t2.FDEMANDBILLENTRYID
                                 from T_PUR_POORDERENTRY t1
                                 left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                                 left join T_PUR_POORDER t3 on t1.FID=t3.FID
                                 where t2.FDEMANDBILLENTRYID>0  and t1.FENTRYID={purchaseEntryId}
                                 union
                                 select  t2.FBILLNO+'-'+CONVERT(nvarchar(10),t1.FSEQ) PoNo,t8.FSALEORDERENTRYID FDEMANDBILLENTRYID
                                 from T_PUR_POORDERENTRY t1
                                 left join T_PUR_POORDER t2 on t1.FID=t2.FID
                                 left join T_PUR_POORDERENTRY_LK t5 on t1.FENTRYID=t5.FENTRYID
                                 left join T_PUR_REQENTRY_LK t6 on t6.FENTRYID=t5.FSID
                                 left join T_PLN_PLANORDER_LK t7 on t6.FSID=t7.FID
                                 left JOIN T_PLN_PLANORDER_B t8 ON t7.FSBILLID=t8.FID
                                 where t8.FSALEORDERENTRYID>0  and t1.FENTRYID={purchaseEntryId} ";
                    var poData = DBUtils.ExecuteDynamicObject(this.Context, poSql);
                    foreach (var poItem in poData)
                    {
                        var soEntryID = Convert.ToInt64(poItem["FDEMANDBILLENTRYID"]);
                        var PoNo = Convert.ToString(poItem["PoNo"]);
                        //获取销售订单信息
                        var soSql = $@"/*dialect*/select  t2.FBILLNO+'-'+CONVERT(nvarchar(10),t1.FSEQ) SoNo,t5.FNUMBER,t3.FWECHATCODE,t2.FPlatCreatorWXCode,t4.FDeliveryDate 
                                    from T_SAL_ORDERENTRY t1
                                    inner join T_SAL_ORDER t2 on t1.FID=t2.FID
                                    inner join V_BD_SALESMAN t3 on t2.FSalerId=t3.fid
                                    inner join T_SAL_ORDERENTRY_D t4 on t1.FENTRYID=t4.FENTRYID
                                    inner join T_BD_MATERIAL t5 on t1.FMATERIALID=t5.FMATERIALID
                                    where t1.FENTRYID={soEntryID} ";
                        var soData = DBUtils.ExecuteDynamicObject(this.Context, soSql);
                        foreach (var soItem in soData)
                        {
                            List<string> wxCode = new List<string>();
                            if (!string.IsNullOrEmpty(soItem["FWECHATCODE"].ToString()))
                            {
                                wxCode.Add(soItem["FWECHATCODE"].ToString());
                            }
                            if (!string.IsNullOrEmpty(soItem["FPlatCreatorWXCode"].ToString()))
                            {
                                wxCode.Add(soItem["FPlatCreatorWXCode"].ToString());
                            }
                            if (wxCode.Count > 0)
                            {
                                var wxContent = $"[ {soItem["SoNo"].ToString()} ]订单[ {soItem["FNUMBER"].ToString()} ]料号交期[ {Convert.ToDateTime(soItem["FDeliveryDate"]).ToString("yyyy-MM-dd")} ]，供应商回复交期[ {date.ToString("yyyy-MM-dd")} ]，已逾期请关注。【{PoNo}】";
                                SendTextMessageUtils.SendTextMessage(string.Join("|", wxCode), wxContent);
                            }
                        }

                    }
                }

            }
        }

        /// <summary>
        /// 保存调用更新备注拼接
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //操作按钮
            if (this.FormOperation.Operation.EqualsIgnoreCase("Save"))
            {
                foreach (var item in e.DataEntitys)
                {
                    var fId = Convert.ToInt64(item["Id"]);
                    var purchaseEntryId = Convert.ToInt64(item["FPurchaseEntryId"]);
                    var remarks = Convert.ToString(item["FRemarks"]);
                    var trackingNumber = Convert.ToString(item["FTrackingNumber"]).Trim().Replace(" ", "");
                    if (item["FTrackingID"] != null)
                    {
                        var trackingName = Convert.ToString(((DynamicObject)item["FTrackingID"])["FDataValue"].ToString());
                        var trackingCode = Convert.ToString(((DynamicObject)item["FTrackingID"])["FNumber"].ToString());
                        remarks = remarks + $" ({trackingName}：{trackingNumber})";
                        DBUtils.Execute(this.Context, "update T_PUR_SuplierDelivery set FREMARKS=@FREMARKS,FTrackingNumber=@FTrackingNumber where FID=@FID",
                            new List<SqlParam>() {
                                new SqlParam("@FREMARKS", KDDbType.String, remarks),
                                new SqlParam("@FID", KDDbType.Int64, fId),
                                new SqlParam("@FTrackingNumber", KDDbType.String, trackingNumber) });
                        DBUtils.Execute(this.Context, "update T_PUR_POORDERENTRY set FSUPPLIERDESCRIPTIONS=@FREMARKS,FTrackingNumber=@FTrackingNumber where FENTRYID=@FENTRYID",
                       new List<SqlParam>() {
                           new SqlParam("@FREMARKS", KDDbType.String, remarks),
                           new SqlParam("@FENTRYID", KDDbType.Int64, purchaseEntryId),
                           new SqlParam("@FTrackingNumber", KDDbType.String, trackingNumber) });
                    }

                }
            }
        }
    }
}
