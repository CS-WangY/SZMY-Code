using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using System.Security.Cryptography;
using System.Linq;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Log;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.BOS.Core.Bill;
using Kingdee.Mymooo.ServiceHelper;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    [Description("采购订单-打开供应商回复列表插件"), HotUpdate]
    public class PurchaseList : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            ListShowParameter param = new ListShowParameter();
            switch (e.BarItemKey)
            {
                case "PENY_OpenSuppliersReplyList":
                    //选择的行,获取所有信息,放在listcoll里面
                    ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;
                    //获取所选行的主键,赋值给一个数组listKey
                    //接收返回的数组值
                    string[] listKey = listcoll.GetPrimaryKeyValues();
                    param.IsIsolationOrg = false;
                    param.FormId = "PENY_SuppliersReplyList";
                    param.PermissionItemId = PermissionConst.View;
                    param.ListFilterParameter.Filter = "FCGDDID='" + listKey[0] + "'";
                    param.CustomParams.Add("BILLID", listKey[0]);
                    param.Width = 800;
                    param.Height = 500;
                    this.View.ShowForm(param);
                    break;
                case "PENY_tbSuplierDeliveryBlukEdit":
                    //选择的行,获取所有信息,放在listcoll里面
                    ListSelectedRowCollection blukListcoll = this.ListView.SelectedRowsInfo;
                    if (blukListcoll.Count > 0)
                    {
                        var key = blukListcoll[0];
                        if (key.EntryEntityKey.EqualsIgnoreCase("FPOOrderEntry"))
                        {
                            var entityIds = blukListcoll.GetEntryPrimaryKeyValues();
                            DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                            formParameter.FormId = "PENY_PoSuplierDeliveryBlukEdit";
                            formParameter.CustomComplexParams["datas"] = string.Join(",", entityIds);
                            this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
                            {
                                this.ListView.Refresh();
                            }));
                        }
                        else
                        {
                            this.ListView.ShowMessage("请选择明细体过滤信息");
                        }
                    }
                    else
                    {
                        this.ListView.ShowMessage("请勾选行");
                    }
                    break;
                default:
                    break;
            }
        }
    }


    //[Description("供应商回复列表插件"), HotUpdate]
    //public class PurchaseSuppliersReplyList : AbstractListPlugIn
    //{
    //    //按钮点击事件（查询）
    //    public override void AfterButtonClick(AfterButtonClickEventArgs e)
    //    {
    //        base.AfterButtonClick(e);
    //        if (e.Key.ToUpper().Equals("Save"))
    //        {
    //            var remarks = Convert.ToString(this.View.Model.GetValue("FRemarks"));
    //            var trackingNumber = Convert.ToString(this.View.Model.GetValue("FTrackingNumber"));
    //            if (this.View.Model.GetValue("FTrackingID") != null)
    //            {
    //                var trackingName = Convert.ToString(((DynamicObject)this.View.Model.GetValue("FTrackingID"))["Name"].ToString());
    //                this.View.Model.SetValue("FRemarks", remarks + $" ({trackingName}：{trackingNumber})");
    //            }
    //        }
    //    }
    //}

    [Description("供应商回复批改列表插件"), HotUpdate]
    public class PoSuplierDeliveryBlukEditList : AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 初始化绑定
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var list = this.View.OpenParameter.GetCustomParameter("datas");

            this.Model.SetValue("FENTRYIDList", list);
            this.View.UpdateView("FENTRYIDList");
        }

        //按钮点击事件（查询）
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            if (e.Key.ToUpper().Equals("FOK"))
            {
                var entryidList = Convert.ToString(this.View.Model.GetValue("FENTRYIDList"));
                var date = Convert.ToString(this.View.Model.GetValue("FDate"));
                var remarks = Convert.ToString(this.View.Model.GetValue("FRemarks"));
                var isBeOverdue = Convert.ToString(this.View.Model.GetValue("FIsBeOverdue"));
                var trackingNumber = Convert.ToString(this.View.Model.GetValue("FTrackingNumber")).Trim().Replace(" ", "");
                var dynamicTrackingID = this.View.Model.GetValue("FTrackingID");
                var trackingID = "";
                var trackingCode = "";
                if (string.IsNullOrWhiteSpace(remarks) && string.IsNullOrWhiteSpace(trackingNumber))
                {
                    throw new Exception("回复说明或者快递单号不能为空。");
                }
                else if (!string.IsNullOrWhiteSpace(trackingNumber) && dynamicTrackingID == null)
                {
                    throw new Exception("存在快递单号，请选择对应的快递公司。");
                }
                else if (dynamicTrackingID != null)
                {
                    trackingID = Convert.ToString(((DynamicObject)dynamicTrackingID)["Id"].ToString());
                    trackingCode = Convert.ToString(((DynamicObject)dynamicTrackingID)["FNumber"].ToString());
                    if (string.IsNullOrWhiteSpace(trackingNumber) && !trackingCode.EqualsIgnoreCase("huolala") && !trackingCode.EqualsIgnoreCase("qita"))
                    {
                        throw new Exception("快递公司不为空，请填写准确的快递单号。");
                    }
                }
                // 采购订单下推采购回复交期
                var rules = ConvertServiceHelper.GetConvertRules(this.Context, "PUR_PurchaseOrder", "PENY_PUR_SuplierDelivery");
                var rule = rules.FirstOrDefault(t => t.IsDefault);
                if (rule == null)
                {
                    throw new Exception("没有从采购订单到采购回复交期的转换关系");
                }
                foreach (var item in entryidList.Split(','))
                {
                    var sql = $@"/*dialect*/select top 1 FID from T_PUR_POORDERENTRY where FENTRYID={item} ";
                    long FId = DBServiceHelper.ExecuteScalar<long>(this.Context, sql, 0);
                    if (FId > 0)
                    {
                        List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                        var view = FormMetadataUtils.CreateBillView(this.Context, "PUR_PurchaseOrder", FId);
                        var outEntrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FPOOrderEntry"));
                        var entry = outEntrys.FirstOrDefault(x => Int64.Parse(item) == Convert.ToInt64(x["Id"]));
                        if (entry != null)
                        {
                            selectedRows.Add(new ListSelectedRow(FId.ToString(), item, 0, "PUR_PurchaseOrder") { EntryEntityKey = "FPOOrderEntry" });
                            if (selectedRows.Count > 0)
                            {
                                PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                                {
                                    TargetBillTypeId = "",     // 请设定目标单据单据类型
                                    TargetOrgId = 0,            // 请设定目标单据主业务组织
                                };

                                //执行下推操作，并获取下推结果
                                var operationResult = ConvertServiceHelper.Push(this.Context, pushArgs, null);
                                var sdView = FormMetadataUtils.CreateBillView(this.Context, "PENY_PUR_SuplierDelivery");
                                if (operationResult.IsSuccess)
                                {
                                    foreach (var items in operationResult.TargetDataEntities)
                                    {
                                        sdView.Model.DataObject = items.DataEntity;
                                    }
                                    sdView.Model.SetValue("FIsBeOverdue", isBeOverdue);
                                    sdView.Model.SetValue("FDate", date);
                                    sdView.Model.SetValue("FRemarks", remarks);
                                    sdView.Model.SetValue("FTrackingID", trackingID);
                                    sdView.Model.SetValue("FTrackingNumber", trackingNumber);
                                    //保存批核
                                    var opers = MymoooBusinessDataServiceHelper.SaveBill(this.Context, sdView.BusinessInfo, sdView.Model.DataObject);
                                    if (opers.IsSuccess)
                                    {
                                        //更新对应的销售订单的供应商回复交期（供应商回复交期保存更新销售单插件已更新）
                                        //var upSoSql = $@"/*dialect*/update T_SAL_ORDERENTRY set FSUPPLIERREPLYDATE='{date}',FSUPPLIERDESCRIPTIONS=N'{remarks}' where FENTRYID in (
                                        //                select FDEMANDBILLENTRYID from (
                                        //                 select t1.FENTRYID,t2.FDEMANDBILLENTRYID
                                        //                                            from
                                        //                                            T_PUR_POORDERENTRY t1
                                        //                                            left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                                        //                          where t2.FDEMANDBILLENTRYID>0
                                        //                       union
                                        //                       select t1.FENTRYID,t8.FSALEORDERENTRYID FDEMANDBILLENTRYID
                                        //                                            from T_PUR_POORDERENTRY t1
                                        //                       left join T_PUR_POORDERENTRY_LK t5 on t1.FENTRYID=t5.FENTRYID
                                        //                       left join T_PUR_REQENTRY_LK t6 on t6.FENTRYID=t5.FSID
                                        //                       left join T_PLN_PLANORDER_LK t7 on t6.FSID=t7.FID
                                        //                                            left JOIN T_PLN_PLANORDER_B t8 ON t7.FSBILLID=t8.FID
                                        //                       where t8.FSALEORDERENTRYID>0
                                        //                ) datas where FENTRYID={item} ) ";
                                        //DBServiceHelper.Execute(this.Context, upSoSql);
                                    }
                                    else
                                    {
                                        if (opers.ValidationErrors.Count > 0)
                                        {
                                            throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                        }
                                        else
                                        {
                                            throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                        }
                                    }
                                }
                                //清除释放网控
                                sdView.CommitNetworkCtrl();
                                sdView.InvokeFormOperation(FormOperationEnum.Close);
                                sdView.Close();

                            }
                        }
                        //清除释放网控
                        view.CommitNetworkCtrl();
                        view.InvokeFormOperation(FormOperationEnum.Close);
                        view.Close();
                    }
                }
    
            }
        }

    }
}
