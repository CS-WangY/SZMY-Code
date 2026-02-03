using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Kingdee.BOS.Core.Report;
using System.Security.Cryptography;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
using System.Web.Util;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.Metadata;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单列表插件"), HotUpdate]
    public class SalesOrderListPlugIn : AbstractListPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);

            if (e.FieldKey.EqualsIgnoreCase("FMaterialName"))
            {
                var rowinfo = this.ListView.CurrentSelectedRowInfo;
                var MaterialId = Convert.ToString(rowinfo.DataRow["FMaterialId_Id"]);
                e.Cancel = true;
                SysReportShowParameter para = new SysReportShowParameter();
                para.OpenStyle.ShowType = ShowType.Modal;
                //唯一标识
                para.FormId = "PENY_SalOrderMaterial_Rpt";
                para.Width = 1250;
                para.Height = 500;
                para.IsShowFilter = false;
                para.CustomParams["FMATERIALID"] = MaterialId;
                this.View.ShowForm(para);
            }
        }
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            DynamicFormShowParameter showParam = new DynamicFormShowParameter();
            showParam.FormId = "PENY_BillClosedSubmitApproval";
            switch (e.BarItemKey)
            {
                case "PENY_BillClose":
                    //取销售员微信code
                    var saluserid = Convert.ToInt64(this.ListView.SelectedRowsInfo.Select(x => x.DataRow["FSalerId_Id"]).First());
                    var sSql = $@"SELECT t1.FWECHATCODE FROM V_BD_SALESMAN t1 INNER JOIN T_BD_STAFF t2 ON t1.FSTAFFID=t2.FSTAFFID
                                INNER JOIN dbo.T_HR_EMPINFO t3 ON t2.FPERSONID=t3.FPERSONID
                                WHERE t1.fid={saluserid} AND t3.FFORBIDSTATUS='A'";
                    var wxSalUserCode = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "");
                    if (string.IsNullOrWhiteSpace(wxSalUserCode))
                    {
                        wxSalUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                        if (string.IsNullOrWhiteSpace(wxSalUserCode))
                        {
                            throw new Exception("请先配置您的微信Code");
                        }
                    }

                    var rowinfo = this.ListView.SelectedRowsInfo.Select(x => x.PrimaryKeyValue).Distinct().ToArray();
                    if (rowinfo.Length > 1)
                    {
                        throw new Exception("不允许多张订单同时申请取消!");
                    }

                    sSql = $@"SELECT * FROM T_SAL_ORDERENTRY_R t1 INNER JOIN T_SAL_ORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
WHERE t2.FID IN ({string.Join(",", rowinfo)}) AND t1.FCANOUTQTY<>t1.FREMAINOUTQTY";
                    var canout= DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    if (canout.Count > 0)
                    {
                        throw new Exception("已有发货通知单不支持申请取消!");
                    }

                    this.View.ShowForm(showParam, new Action<FormResult>((result) =>
                    {
                        if (!result.ReturnData.IsNullOrEmpty())
                        {
                            K3CloudClosedSalOrderRequest request = new K3CloudClosedSalOrderRequest();
                            sSql = $@"SELECT t4.FNAME CustName,t2.FENTRYID,(t3.FTaxNetPrice*tr.FBASECANOUTQTY) AS FALLAMOUNT,tr.FBASECANOUTQTY,
                            CASE
                            WHEN t2.FSUPPLYTARGETORGID = 7401780 OR t2.FSUPPLYTARGETORGID = 7401781
							THEN '第一二事业部'
                            WHEN t2.FSUPPLYTARGETORGID = 7207688
							THEN '第三事业部'
                            WHEN t2.FSUPPLYTARGETORGID = 7401821
							THEN '华南事业部'
                            ELSE ''
                            END AS BusinessDivision
,t7.FNAME AS OrgName
,t1.FCREATEDATE,t1.FBILLNO,t1.FID,t5.FNUMBER,t6.FNAME
                                FROM dbo.T_SAL_ORDER t1
                                LEFT JOIN dbo.T_SAL_ORDERENTRY t2 ON t1.FID=t2.FID AND t2.FMRPCLOSESTATUS='A'
                                LEFT JOIN dbo.T_SAL_ORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
                                LEFT JOIN dbo.T_SAL_ORDERENTRY_R tr ON t2.FENTRYID=tr.FENTRYID
                                LEFT JOIN dbo.T_BD_CUSTOMER_L t4 ON t1.FCUSTID=t4.FCUSTID
                                LEFT JOIN dbo.T_BD_MATERIAL t5 ON t2.FMATERIALID=t5.FMATERIALID
                                LEFT JOIN dbo.T_BD_MATERIAL_L t6 ON t5.FMATERIALID=t6.FMATERIALID
                                LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t7 ON t2.FSUPPLYTARGETORGID=t7.FORGID
                                WHERE t1.FID IN ({string.Join(",", rowinfo)})";
                            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                            var varAllBills = datas.GroupBy(o => new
                            {
                                BusinessDivision = Convert.ToString(o["BusinessDivision"]),
                                FBILLNO = Convert.ToString(o["FBILLNO"])
                            }).Select(g => new
                            {
                                BusinessDivision = g.Key.BusinessDivision,
                                FBILLNO = g.Key.FBILLNO
                            });
                            foreach (var item in varAllBills)
                            {
                                var applist = datas.Where(x => x["BusinessDivision"].Equals(item.BusinessDivision)
                                && x["FBILLNO"].Equals(item.FBILLNO)
                                ).ToList();
                                request.OperationType = SalOrderClosedOperationType.Bill;
                                request.SaleOrderID = Convert.ToInt64(applist.Select(x => x["FID"]).First());
                                request.CustName = Convert.ToString(applist.Select(x => x["CustName"]).First());
                                request.ClosedAmount = applist.Sum(x => (decimal)x["FALLAMOUNT"]);
                                request.PlaceDate = Convert.ToDateTime(applist.Select(x => x["FCREATEDATE"]).First());
                                request.BillNo = Convert.ToString(applist.Select(x => x["FBILLNO"]).First());
                                request.Material = string.Join(",", applist.Select(x => x["FNUMBER"] + "/" + x["FNAME"]).ToArray());
                                request.OrderQty = applist.Sum(x => (decimal)x["FBASECANOUTQTY"]);
                                request.CustType = Convert.ToString("终端商");
                                request.IsRefund = "否";
                                //request.OrgID = Convert.ToString(((DynamicObject)dataEntity["SaleOrgId"])["Name"]);
                                request.OrgID = Convert.ToString(applist.Select(x => x["BusinessDivision"]).First());
                                request.OrgName = Convert.ToString(applist.Select(x => x["OrgName"]).First());

                                request.Remarks = result.ReturnData.ToString();
                                request.SaleOrderEntrys = applist.Select(x => Convert.ToInt64(x["FENTRYID"])).ToList();
                                request.SummaryInfo = new string[] {
                            "订单单号：" + Convert.ToString(applist.Select(x => x["FBILLNO"]).First()),
                            "客户名称：" + Convert.ToString(applist.Select(x => x["CustName"]).First()),
                            "备注：" + result.ReturnData.ToString() };
                                ValidatorApprove(request, wxSalUserCode);
                            }
                            this.ListView.Refresh();
                        }
                    }));
                    break;
                case "PENY_BillRowClose":
                    //取销售员微信code
                    saluserid = Convert.ToInt64(this.ListView.SelectedRowsInfo.Select(x => x.DataRow["FSalerId_Id"]).First());
                    sSql = $@"SELECT t1.FWECHATCODE FROM V_BD_SALESMAN t1 INNER JOIN T_BD_STAFF t2 ON t1.FSTAFFID=t2.FSTAFFID
                                INNER JOIN dbo.T_HR_EMPINFO t3 ON t2.FPERSONID=t3.FPERSONID
                                WHERE t1.fid={saluserid} AND t3.FFORBIDSTATUS='A'";
                    wxSalUserCode = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "");
                    if (string.IsNullOrWhiteSpace(wxSalUserCode))
                    {
                        wxSalUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                        if (string.IsNullOrWhiteSpace(wxSalUserCode))
                        {
                            throw new Exception("请先配置您的微信Code");
                        }
                    }

                    rowinfo = this.ListView.SelectedRowsInfo.Select(x => x.PrimaryKeyValue).Distinct().ToArray();
                    if (rowinfo.Count() > 1)
                    {
                        throw new Exception("不允许多张订单行同时申请取消!");
                    }
                    
                    var entityRowInfo = this.ListView.SelectedRowsInfo.Select(x => x.EntryPrimaryKeyValue).ToArray();

                    sSql = $@"SELECT * FROM T_SAL_ORDERENTRY_R t1 INNER JOIN T_SAL_ORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
WHERE t2.FENTRYID IN ({string.Join(",", entityRowInfo)}) AND t1.FCANOUTQTY<>t1.FREMAINOUTQTY";
                    canout = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    if (canout.Count > 0)
                    {
                        throw new Exception("已有发货通知单不支持申请取消!");
                    }

                    this.View.ShowForm(showParam, new Action<FormResult>((result) =>
                    {
                        if (!result.ReturnData.IsNullOrEmpty())
                        {
                            K3CloudClosedSalOrderRequest request = new K3CloudClosedSalOrderRequest();
                            sSql = $@"SELECT t4.FNAME CustName,t2.FENTRYID,(t3.FTaxNetPrice*tr.FBASECANOUTQTY) AS FALLAMOUNT,tr.FBASECANOUTQTY,
                            CASE
                            WHEN t2.FSUPPLYTARGETORGID = 7401780 OR t2.FSUPPLYTARGETORGID = 7401781
							THEN '第一二事业部'
                            WHEN t2.FSUPPLYTARGETORGID = 7207688
							THEN '第三事业部'
                            WHEN t2.FSUPPLYTARGETORGID = 7401821
							THEN '华南事业部'
                            ELSE ''
                            END AS BusinessDivision
                            ,t7.FNAME AS OrgName
                            ,t1.FCREATEDATE,t1.FBILLNO,t1.FID,t5.FNUMBER,t6.FNAME
                            FROM dbo.T_SAL_ORDER t1
                            LEFT JOIN dbo.T_SAL_ORDERENTRY t2 ON t1.FID=t2.FID
                            LEFT JOIN dbo.T_SAL_ORDERENTRY_F t3 ON t2.FENTRYID=t3.FENTRYID
                            LEFT JOIN dbo.T_SAL_ORDERENTRY_R tr ON t2.FENTRYID=tr.FENTRYID
                            LEFT JOIN dbo.T_BD_CUSTOMER_L t4 ON t1.FCUSTID=t4.FCUSTID
                            LEFT JOIN dbo.T_BD_MATERIAL t5 ON t2.FMATERIALID=t5.FMATERIALID
                            LEFT JOIN dbo.T_BD_MATERIAL_L t6 ON t5.FMATERIALID=t6.FMATERIALID
                            LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t7 ON t2.FSUPPLYTARGETORGID=t7.FORGID
                            WHERE t2.FENTRYID IN ({string.Join(",", entityRowInfo)})";
                            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                            var varAllBills = datas.GroupBy(o => new
                            {
                                BusinessDivision = Convert.ToString(o["BusinessDivision"]),
                                FBILLNO = Convert.ToString(o["FBILLNO"])
                            }).Select(g => new
                            {
                                BusinessDivision = g.Key.BusinessDivision,
                                FBILLNO = g.Key.FBILLNO
                            });
                            foreach (var item in varAllBills)
                            {
                                var applist = datas.Where(x => x["BusinessDivision"].Equals(item.BusinessDivision)
                                && x["FBILLNO"].Equals(item.FBILLNO)
                                ).ToList();
                                request.OperationType = SalOrderClosedOperationType.Entity;
                                request.SaleOrderID = Convert.ToInt64(applist.Select(x => x["FID"]).First());
                                request.CustName = Convert.ToString(applist.Select(x => x["CustName"]).First());
                                request.ClosedAmount = applist.Sum(x => (decimal)x["FALLAMOUNT"]);
                                request.PlaceDate = Convert.ToDateTime(applist.Select(x => x["FCREATEDATE"]).First());
                                request.BillNo = Convert.ToString(applist.Select(x => x["FBILLNO"]).First());
                                request.Material = string.Join(",", applist.Select(x => x["FNUMBER"] + "/" + x["FNAME"]).ToArray());
                                request.OrderQty = applist.Sum(x => (decimal)x["FBASECANOUTQTY"]);
                                request.CustType = Convert.ToString("终端商");
                                request.IsRefund = "否";
                                //if (!Convert.ToString(applist.Select(x => x["BusinessDivision"]).First()).IsNullOrEmptyOrWhiteSpace())
                                //{
                                request.OrgID = Convert.ToString(applist.Select(x => x["BusinessDivision"]).First());
                                request.OrgName= Convert.ToString(applist.Select(x => x["OrgName"]).First());
                                //}
                                //else
                                //{
                                //    request.OrgID = null;
                                //}
                                request.Remarks = result.ReturnData.ToString();
                                request.SaleOrderEntrys = applist.Select(x => Convert.ToInt64(x["FENTRYID"])).ToList();
                                request.SummaryInfo = new string[] {
                                                    "订单单号：" + Convert.ToString(applist.Select(x => x["FBILLNO"]).First()),
                                                    "客户名称：" + Convert.ToString(applist.Select(x => x["CustName"]).First()),
                                                    "备注：" + result.ReturnData.ToString() };
                                ValidatorApprove(request, wxSalUserCode);
                            }
                            this.View.Refresh();
                        }
                    }));
                    break;

            }
        }


        public void ValidatorApprove(K3CloudClosedSalOrderRequest request, string creatorid)
        {
            try
            {
                //赋值抄送人
                var wxUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                if (string.IsNullOrWhiteSpace(wxUserCode))
                {
                    throw new Exception("请先配置您的微信Code");
                }
                List<string> notifyerlist = new List<string>();
                notifyerlist.Add(creatorid);
                request.Notifyer = notifyerlist;

                //执行调用审批流
                //AttachmentMaterials
                request.Creator_userid = wxUserCode;
                request.SendRabbitCode = "mymooo_weixin_Approval_ClosedSalOrder_" + ApigatewayUtils.ApigatewayConfig.EnvCode;
                request.EnvCode = ApigatewayUtils.ApigatewayConfig.EnvCode;

                var result = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3CloudClosedSalBillApproval", JsonConvertUtils.SerializeObject(request));
                var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(result);
                if (!returninfo.Code.EqualsIgnoreCase("success"))
                {
                    throw new Exception("发起审批流失败：" + returninfo.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("发起审批流失败：" + ex.ToString());
            }
        }
    }

    [Description("销售订单表单行关闭插件"), HotUpdate]
    public class SalesOrderBusinessPlugIn : AbstractDynamicFormPlugIn
    {
        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            switch (e.BarItemKey)
            {
                case "PENY_RowMRPClose":
                    DynamicFormShowParameter showParam = new DynamicFormShowParameter();
                    showParam.FormId = "PENY_BillClosedSubmitApproval";

                    break;
            }
        }

        public void ValidatorApprove(K3CloudClosedSalOrderRequest request)
        {
            try
            {
                var wxUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);

                if (string.IsNullOrWhiteSpace(wxUserCode))
                {
                    throw new Exception("请先配置您的微信Code");
                }

                //执行调用审批流
                //AttachmentMaterials
                request.Creator_userid = wxUserCode;
                request.SendRabbitCode = "mymooo_weixin_Approval_ClosedSalOrder_" + ApigatewayUtils.ApigatewayConfig.EnvCode;
                request.EnvCode = ApigatewayUtils.ApigatewayConfig.EnvCode;

                var result = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3CloudClosedSalBillApproval", JsonConvertUtils.SerializeObject(request));
                var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(result);
                if (!returninfo.Code.EqualsIgnoreCase("success"))
                {
                    throw new Exception("发起审批流失败：" + returninfo.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("发起审批流失败：" + ex.ToString());
            }
        }
    }
}
