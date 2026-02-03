using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.Utils;

namespace Kingdee.Mymooo.Business.PlugIn.TransferOutManagement
{
    [Description("分步式调拨单-撤销反审核发货通知"), HotUpdate]
    public class TransferOutList : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            
            if (e.BarItemKey.Equals("PENY_Revocation"))
            {
                PermissionAuthResult iResult = PermissionServiceHelper.FuncPermissionAuth
                (this.Context, new BusinessObject() { Id = "STK_TransferOut" }, "65386ae0b64ba4");
                if (!iResult.Passed)
                {
                    this.View.ShowMessage("没有特结撤销的权限，不能操作！");
                    return;
                }

                var lv = this.View as IListView;
                if (lv == null)
                {
                    return;
                }
                var selectedRows = lv.SelectedRowsInfo;
                if (selectedRows == null || selectedRows.Count == 0)
                {
                    this.View.ShowErrMessage("当前没有选中行！");
                    return;
                }
                if (selectedRows.Count > 1)
                {
                    this.View.ShowErrMessage("不能选中多行处理！");
                    return;
                }
                string msg = "撤销功能说明，确认是否需要做此操作：" +
                            "\r\n1.将选中的调拨单撤销并且删除单据。" +
                            "\r\n2.反审核相关发货通知单。" +
                            "\r\n3.发送企业微信消息给相关发货制单人。" +
                            "\r\nTips:如果发货单号存在多笔调拨，可能会涉及到其他事业部调拨单，将一并处理删除，请提前确认好相关调拨单是否正确" +
                            "\r\n     此功能不会处理已审核的分步式调出单";
                this.View.ShowMessage(msg, MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                        {
                            foreach (var row in selectedRows.DistinctBy(x => x.PrimaryKeyValue))
                            {
                                var DNBillno = row.DataRow["FPENYDELIVERYNOTICE"].ToString();
                                //判断是否有多张调拨单
                                string sSql = $@"SELECT DISTINCT t1.FID,t1.FSTOCKORGID FROM dbo.T_STK_STKTRANSFEROUT t1
                                        INNER JOIN T_STK_STKTRANSFEROUTENTRY t2 ON t1.FID=t2.FID
                                        WHERE t2.FPENYDELIVERYNOTICE='{DNBillno}'";
                                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                                //if (datas.Where(x => Convert.ToInt64(x["FSTOCKORGID"]) != 7401803
                                //&& Convert.ToInt64(x["FSTOCKORGID"]) != 7401782).Count() > 0)
                                //{
                                //    throw new Exception("此发货单包含其他事业部调拨单，无法执行特结撤销处理！");
                                //}
                                List<KeyValuePair<object, object>> pkEntityIds = new List<KeyValuePair<object, object>>();

                                foreach (var item in datas)
                                {
                                    pkEntityIds.Add(new KeyValuePair<object, object>(item["FID"], ""));
                                }
                                FormMetadata meta = MetaDataServiceHelper.Load(this.Context, "STK_TRANSFEROUT") as FormMetadata;
                                // 构建操作可选参数对象
                                IOperationResult oper = new OperationResult();
                                OperateOption operateOption = OperateOption.Create();
                                operateOption.SetIgnoreWarning(true);
                                operateOption.SetIgnoreInteractionFlag(true);
                                operateOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
                                ISetStatusService setStatusService = ServiceFactory.GetSetStatusService(this.Context);

                                if (row.DataRow["FDOCUMENTSTATUS"].Equals("B"))
                                {
                                    // 调用撤销操作    
                                    oper = setStatusService.SetBillStatus(this.Context, meta.BusinessInfo, pkEntityIds, null, "CancelAssign", operateOption);
                                    if (!oper.IsSuccess)
                                    {
                                        if (oper.ValidationErrors.Count > 0)
                                        {
                                            throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                                        }
                                        else
                                        {
                                            throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                        }
                                    }
                                }
                                //调用删除
                                DeleteService deleteService = new DeleteService();
                                oper = deleteService.Delete(this.Context, meta.BusinessInfo, pkEntityIds.Select(x => (object)x.Key).ToArray(), operateOption);
                                if (!oper.IsSuccess)
                                {
                                    if (oper.ValidationErrors.Count > 0)
                                    {
                                        throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                                    }
                                    else
                                    {
                                        throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                    }
                                }
                                sSql = $@"SELECT FID FROM T_SAL_DELIVERYNOTICE WHERE FBILLNO='{DNBillno}'";
                                var dnid = DBServiceHelper.ExecuteScalar<Int64>(this.Context, sSql, 0);
                                pkEntityIds.Clear();
                                pkEntityIds.Add(new KeyValuePair<object, object>(dnid, ""));
                                //反审核发货通知单
                                meta = MetaDataServiceHelper.Load(this.Context, "SAL_DELIVERYNOTICE") as FormMetadata;
                                oper = setStatusService.SetBillStatus(this.Context, meta.BusinessInfo, pkEntityIds, null, "UnAudit", operateOption);
                                if (!oper.IsSuccess)
                                {
                                    if (oper.ValidationErrors.Count > 0)
                                    {
                                        throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                                    }
                                    else
                                    {
                                        throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                    }
                                }
                                //调用云仓储接口
                                sSql = $@"SELECT FSEQ FROM T_SAL_DELIVERYNOTICEENTRY WHERE FID={dnid}";
                                datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                                ArrayList arrList = new ArrayList();
                                foreach (var item in datas)
                                {
                                    var resultdn = new
                                    {
                                        itemId = DNBillno + "-" + item["FSEQ"],
                                        exWarehouseOrderNumber = DNBillno
                                    };
                                    arrList.Add(resultdn);
                                }
                                var requestData = JsonConvertUtils.SerializeObject(arrList);
                                AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockDeleteTempDeliveryArea", DNBillno);
                                //发送企业微信消息
                                var createUser = Convert.ToInt64(row.DataRow["FCreatorId_Id"]);
                                //创建人微信Code
                                var cUserWxCode = ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, createUser);
                                if (!string.IsNullOrWhiteSpace(cUserWxCode))
                                {
                                    SendTextMessageUtils.SendTextMessage(cUserWxCode, "您的发货通知单已被仓库特结反审核，请查阅修改：" + string.Join(",", DNBillno));
                                }
                            }
                            cope.Complete();
                        }
                        this.ListView.Refresh();
                    }
                }));
            }
        }
    }
}
