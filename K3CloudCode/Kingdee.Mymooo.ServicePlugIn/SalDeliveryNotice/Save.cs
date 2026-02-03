using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单保存验证可用库存插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FDOCUMENTSTATUS");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FDeliveryOrgID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FPENYNOTE");
            //锁库拆分分录id
            e.FieldKeys.Add("FSPLITLOCKENTRYID");
            e.FieldKeys.Add("FSrcType");
            e.FieldKeys.Add("FSOEntryId");
        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            SaveValidator isPoValidator = new SaveValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);

            //移除金额的校验
            var send = e.Validators.FirstOrDefault(p => p.GetType().FullName.EqualsIgnoreCase("Kingdee.K3.SCM.App.Sal.ServicePlugIn.DeliveryNotice.DNControlSendValidator"));
            if (send != null)
            {
                e.Validators.Remove(send);
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //操作按钮
            if (this.FormOperation.Operation.EqualsIgnoreCase("Save"))
            {
                foreach (var item in e.DataEntitys)
                {
                    var fId = Convert.ToInt64(item["Id"]);
                    var remarks = Convert.ToString(item["FPENYNOTE"]).Trim().TrimStart('\n').TrimEnd('\n');
                    if (!string.IsNullOrWhiteSpace(remarks))
                    {
                        DBUtils.Execute(this.Context, "update T_SAL_DELIVERYNOTICE set FPENYNOTE=@FREMARKS where FID=@FID",
                            new List<SqlParam>() {
                                new SqlParam("@FREMARKS", KDDbType.String, remarks),
                                new SqlParam("@FID", KDDbType.Int64, fId) });
                    }

                }
            }
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                try
                {
                    var billid = Convert.ToInt64(item["Id"]);
                    var orgid = Convert.ToInt64(item["SaleOrgId_Id"]);
                    //if (orgid == 224428 || orgid == 1043841)
                    //{
                    if (item["SalesManID"] is null)
                    {
                        continue;
                    }
                    var salemanid = Convert.ToString(((DynamicObject)item["SalesManID"])["EmpNumber"]);
                    //var salewxcode = GetUserWxCode(this.Context, salemanid);
                    //获取缓存企业微信信息
                    var apiinfo = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/AddressBook/GetLeveParent?usercode={salemanid}&level=1");
                    if (string.IsNullOrWhiteSpace(apiinfo))
                    {
                        continue;
                    }
                    var puserinfo = JsonConvertUtils.DeserializeObject<ParentUserInfo>(apiinfo);
                    var psalesid = BasicDataSyncServiceHelper.GetSaleId(this.Context, orgid, puserinfo.parentUserCode);
                    string sSql = $"/*dialect*/UPDATE T_SAL_DELIVERYNOTICE SET FPENYParentSalesID={psalesid} WHERE FID={billid}";
                    DBUtils.Execute(this.Context, sSql);
                    //}
                }
                catch
                { }
            }
        }
    }
}
