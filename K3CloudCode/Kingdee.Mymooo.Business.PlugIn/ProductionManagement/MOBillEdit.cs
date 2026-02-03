using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.ProductionManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    [Description("生产订单单据插件"), HotUpdate]
    public class MOBillEdit : AbstractBillPlugIn
    {
        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            if (e.BarItemKey.EqualsIgnoreCase("PENY_tbSplitButton") || e.BarItemKey.EqualsIgnoreCase("PENY_tbSendMes"))
            {
                if (!this.View.Model.GetValue("FDocumentStatus").ToString().EqualsIgnoreCase("C"))
                {
                    this.View.ShowMessage("单据未审核,不能发送MES");
                    return;
                }
                MakeRequest request = new MakeRequest();
                request.Id = Convert.ToInt64(this.View.Model.DataObject["Id"]);
                request.MakeNo = this.View.Model.DataObject["BillNo"].ToString();
                request.Date = Convert.ToDateTime(this.View.Model.DataObject["Date"]);
                var org = this.View.Model.DataObject["PrdOrgId"] as DynamicObject;
                request.PrdOrgCode = Convert.ToString(org["Number"]);
                request.PrdOrgName = Convert.ToString(org["Name"]);
                var planner = this.View.Model.DataObject["PlannerID"] as DynamicObject;
                if (planner != null)
                {
                    request.PlannerCode = Convert.ToString(planner["Number"]);
                    request.PlannerName = Convert.ToString(planner["Name"]);
                }

                var oper = ProductionMoServiceHelper.SendMakeDispatchForBill(this.Context, request);
                if (oper.IsSuccess)
                {
                    this.View.ShowMessage("生产信息已发送到mes,请稍后关注发送MES状态信息");
                }
            }
            else if (e.BarItemKey.EqualsIgnoreCase("PENY_tbCancelMES"))
            {
                //var mesStatus = this.View.Model.GetValue("FISDISPATCHMES", rowIndex).ToString();
                //if (mesStatus == "0" || string.IsNullOrWhiteSpace(mesStatus))
                //{
                //	this.View.ShowMessage("生产信息没有发送MES,不需要取消");
                //	return;
                //}
                //else if (mesStatus == "1")
                //{
                //	this.View.ShowMessage("生产信息正在同步中,不能取消");
                //	return;
                //}
                //else if (mesStatus == "3")
                //{
                //	this.View.ShowMessage("生产信息已在取消中,不能重复取消");
                //	return;
                //}
                //var entity = this.View.BusinessInfo.GetEntity("FTreeEntity");
                //var entry = this.View.Model.GetEntityDataObject(entity, rowIndex);
                //MakeDispatchRequest makeDispatchRequest = new MakeDispatchRequest();
                //makeDispatchRequest.MakeNo = this.View.Model.GetValue("FBillNo").ToString();
                //makeDispatchRequest.MakeSeq = Convert.ToInt32(entry["Seq"]);
                //makeDispatchRequest.EntryId = Convert.ToInt32(entry["Id"]);
                //makeDispatchRequest.SaleOrderNo = Convert.ToString(this.View.Model.GetValue("FSaleOrderNo", rowIndex));
                //var oper = ProductionMoServiceHelper.CancelMakeDispatch(this.Context, makeDispatchRequest);
                //if (oper.IsSuccess)
                //{
                //	this.View.ShowMessage("生产信息已发送到mes,请稍后关注发送MES状态信息");
                //	this.View.UpdateView("FTreeEntity", rowIndex);
                //}
            }
        }
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            //计划订单合并或者手工下生产订单，客户取不到就取该物料最近一笔组织间需求的客户
            if (e.Field.Key.EqualsIgnoreCase("FMaterialId"))
            {
                long orgId = Convert.ToInt64(((DynamicObject)this.View.Model.GetValue("FPrdOrgId"))["id"]);
                long materialId = Convert.ToInt32(e.NewValue);
                var supplierId = GetCustomerID(orgId, GetMaterialNo(materialId));
                this.View.Model.SetValue("FPENYCustomerID", supplierId, e.Row);
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
            return DBServiceHelper.ExecuteScalar<long>(this.Context, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 获取物料编码
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        private string GetMaterialNo(long materialId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MaterialId", KDDbType.Int64, materialId) };
            var sql = $@"/*dialect*/select top 1 FNUMBER from T_BD_MATERIAL where FMATERIALID=@MaterialId ";
            return DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "", paramList: pars.ToArray());
        }
    }
}
