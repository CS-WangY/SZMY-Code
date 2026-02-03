using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.PurchaseManagement;
using System.Web.UI.WebControls;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS;
using System.Runtime.InteropServices;
using Kingdee.K3.SCM.Core;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.BOS.App.Data;
using System.Net;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("发货通知单物流方式变更申请列表插件"), HotUpdate]
    public class DnNoticeLogisticsChangesList : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            string dnNos = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DnNos"));
            if (!string.IsNullOrWhiteSpace(dnNos))
            {
                var sql = $@"/*dialect*/select CustName,Warehouse,SUM(AMOUNT) FBILLAMOUNT from (
                            select  t3.FNAME CustName,case when t2.FSUPPLYTARGETORGID=7401782 then '大岭山非标件仓库'
                                                        when t2.FSUPPLYTARGETORGID=7401803 then '惠山非标件仓库' 
                                                        else '大岭山标准仓库' end Warehouse
							                            ,(t2.FQTY*ISNULL(t6.FTAXNETPRICE,0)) AMOUNT
                                                        from  T_SAL_DELIVERYNOTICE t1 
                                                        inner join T_SAL_DELIVERYNOTICEENTRY t2 on t1.FID=t2.FID
                                                        inner join T_BD_CUSTOMER_L t3 on t1.FCUSTOMERID=t3.FCUSTID
                                                        inner join T_SAL_DELIVERYNOTICEENTRY_F t4 on t4.FENTRYID=t2.FENTRYID
														left join T_SAL_DELIVERYNOTICEENTRY_LK t5 on  t5.FENTRYID=t2.FENTRYID
														left join T_SAL_ORDERENTRY_F t6 on t5.FSBILLID=t6.FID and t5.FSID=t6.FENTRYID
                            where FBILLNO in ('{dnNos.Replace("、", "','")}')
                            ) datas
                            group  by CustName,Warehouse";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                foreach (var data in datas)
                {
                    this.Model.SetValue("F_PENY_ApplyDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    this.Model.SetValue("F_PENY_LogisticsType", "");
                    this.Model.SetValue("F_PENY_CustName", data["CustName"]);
                    this.Model.SetValue("F_PENY_OrderAmount", Convert.ToDecimal(data["FBILLAMOUNT"]));
                    this.Model.SetValue("F_PENY_DnNo", dnNos);
                    this.Model.SetValue("FPENYWarehouse", data["Warehouse"]);
                    this.Model.SetValue("F_PENY_Province", GetCustAddress(this.Context, dnNos.Split('、')[0]));
                    this.Model.SetValue("F_PENY_PaymentType", "");
                    this.View.UpdateView("F_PENY_LogisticsType");
                    this.View.UpdateView("F_PENY_PaymentType");
                    this.View.UpdateView("F_PENY_ApplyDate");
                    this.View.UpdateView("F_PENY_CustName");
                    this.View.UpdateView("F_PENY_OrderAmount");
                    this.View.UpdateView("F_PENY_DnNo");
                    this.View.UpdateView("FPENYWarehouse");
                    this.View.UpdateView("F_PENY_Province");
                }
            }
        }

        //按钮点击事件（查询）
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            if (e.Key.ToUpper().Equals("FOK"))
            {
                try
                {
                    var wxUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                    var userName = this.Context.UserName;
                    var applyDate = Convert.ToDateTime(this.View.Model.GetValue("F_PENY_ApplyDate"));
                    var logisticsType = Convert.ToString(this.View.Model.GetValue("F_PENY_LogisticsType"));
                    var custName = Convert.ToString(this.View.Model.GetValue("F_PENY_CustName"));
                    var orderAmount = Convert.ToDecimal(this.View.Model.GetValue("F_PENY_OrderAmount"));
                    var dnNos = Convert.ToString(this.View.Model.GetValue("F_PENY_DnNo"));
                    var warehouse = Convert.ToString(this.View.Model.GetValue("FPENYWarehouse"));
                    var province = Convert.ToString(this.View.Model.GetValue("F_PENY_Province"));
                    var paymentType = Convert.ToString(this.View.Model.GetValue("F_PENY_PaymentType"));
                    var remarks = Convert.ToString(this.View.Model.GetValue("F_PENY_Remarks"));
                    if (string.IsNullOrWhiteSpace(wxUserCode))
                    {
                        this.View.ShowErrMessage("请先配置您的微信Code");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(dnNos))
                    {
                        this.View.ShowErrMessage("单号不能为空");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(logisticsType))
                    {
                        this.View.ShowErrMessage("物流方式必填");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(paymentType))
                    {
                        this.View.ShowErrMessage("付费方式必填");
                        return;
                    }
                    //业务员信息
                    var salUser = GetSalUserWxCode(this.Context, dnNos, wxUserCode, userName);
                    //执行调用审批流
                    K3cloudDnLogisticsChangesRequest request = new K3cloudDnLogisticsChangesRequest();
                    request.DnNo = dnNos;
                    request.CustName = custName;
                    request.Warehouse = warehouse;
                    request.Province = province;
                    request.OrderAmount = orderAmount;
                    request.Remarks = remarks;
                    request.ApplyDate = applyDate;
                    request.LogisticsType = logisticsType;
                    request.PaymentType = paymentType;
                    request.Creator_userid = wxUserCode;
                    request.SendRabbitCode = "mymooo_weixin_Approval_UpDnLogisticsChangesInfo_" + ApigatewayUtils.ApigatewayConfig.EnvCode;
                    request.EnvCode = ApigatewayUtils.ApigatewayConfig.EnvCode;
                    request.SummaryInfo = new string[] { "订单单号：" + dnNos, "客户名称：" + custName, "订单金额：" + orderAmount };
                    //赋值抄送人
                    List<string> notifyer = salUser.Select(s => Convert.ToString(s["FWECHATCODE"])).ToList();
                    string[] strArray = new string[notifyer.Count];
                    for (int i = 0; i < notifyer.Count; i++)
                    {
                        strArray[i] = notifyer[i];
                    }
                    request.Notifyer = strArray;
                    //抄送人姓名
                    List<string> notifyerName = salUser.Select(s => Convert.ToString(s["FNAME"])).ToList();

                    var result = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3cloudDnLogisticsChangesApproval", JsonConvertUtils.SerializeObject(request));
                    var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(result);
                    if (!returninfo.Code.EqualsIgnoreCase("success"))
                    {
                        this.View.ShowErrMessage("发起审批流失败：" + returninfo.ErrorMessage);
                        return;
                    }
                    //执行写入审批日志
                    List<SqlParam> pars = new List<SqlParam>() {
                        new SqlParam("@FApplyDate", KDDbType.Date, applyDate),
                        new SqlParam("@FApprovalNo", KDDbType.String,returninfo.Data.Sp_no),//审批单号
                        new SqlParam("@FDnNo", KDDbType.String, dnNos),
                        new SqlParam("@FCustName", KDDbType.String, custName),
                        new SqlParam("@FWarehouse", KDDbType.String, warehouse),
                        new SqlParam("@FProvince", KDDbType.String, province),
                        new SqlParam("@FOrderAmount", KDDbType.String, orderAmount),
                        new SqlParam("@FRemarks", KDDbType.String, remarks),
                        new SqlParam("@FLogisticsType", KDDbType.String, logisticsType),
                        new SqlParam("@FPaymentType", KDDbType.String, paymentType),
                        new SqlParam("@FSpStatus", KDDbType.Int16, 1),
                        new SqlParam("@FCreateDate", KDDbType.DateTime, DateTime.Now),
                        new SqlParam("@FCreateUser", KDDbType.String,userName),
                        new SqlParam("@FNotifyer", KDDbType.String,string.Join("、",notifyerName))
                    };

                    var sql = $@"INSERT INTO T_SAL_DnLogisticsChanges
                                   (
                                    FApplyDate
                                   ,FApprovalNo
                                   ,FDnNo
                                   ,FCustName
                                   ,FWarehouse
                                   ,FProvince
                                   ,FOrderAmount
                                   ,FRemarks
                                   ,FLogisticsType
                                   ,FPaymentType
                                   ,FSpStatus
                                   ,FCreateDate
                                   ,FCreateUser
                                   ,FNotifyer)
                             VALUES
                                   (@FApplyDate
                                   ,@FApprovalNo
                                   ,@FDnNo
                                   ,@FCustName
                                   ,@FWarehouse
                                   ,@FProvince
                                   ,@FOrderAmount
                                   ,@FRemarks
                                   ,@FLogisticsType
                                   ,@FPaymentType
                                   ,@FSpStatus
                                   ,@FCreateDate
                                   ,@FCreateUser
                                   ,@FNotifyer)";
                    DBServiceHelper.Execute(this.Context, sql, paramList: pars);
                    this.View.Close();
                }
                catch (Exception ex)
                {
                    this.View.ShowErrMessage("发起审批失败：" + ex.Message);
                    return;
                }
            }
        }

        public class K3cloudDnLogisticsChangesRequest
        {
            /// <summary>
            /// DN单号
            /// </summary>
            public string DnNo { get; set; }

            /// <summary>
            /// 客户名称
            /// </summary>
            public string CustName { get; set; }

            /// <summary>
            /// 物流发货仓库
            /// </summary>
            public string Warehouse { get; set; }

            /// <summary>
            /// 发往省份
            /// </summary>
            public string Province { get; set; }

            /// <summary>
            /// 订单金额
            /// </summary>
            public decimal OrderAmount { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string Remarks { get; set; }

            /// <summary>
            /// 申请日期
            /// </summary>
            public DateTime ApplyDate { get; set; } = DateTime.Now;

            /// <summary>
            /// 物流方式
            /// </summary>
            public string LogisticsType { get; set; } = "";

            /// <summary>
            /// 付费方式
            /// </summary>
            public string PaymentType { get; set; } = "";

            /// <summary>
            /// 发起人
            /// </summary>
            public string Creator_userid { get; set; }

            /// <summary>
            /// 备注信息，请提供3行。
            /// </summary>
            public string[] SummaryInfo { get; set; }
            public string SendRabbitCode { get; set; }
            public string EnvCode { get; set; }
            /// <summary>
            /// 抄送人
            /// </summary>
            public string[] Notifyer { get; set; }
        }

        //获取销售员微信Code
        private DynamicObjectCollection GetSalUserWxCode(Context ctx, string dnNos, string userWxCode, string userName)
        {
            string sql = $@"/*dialect*/select  FWECHATCODE,t3.FNAME from  T_SAL_DELIVERYNOTICE t1 
            inner join V_BD_SALESMAN t2 on t1.FSalesManID=t2.fid
            inner join V_BD_SALESMAN_L t3 on t3.fid=t2.fid and t3.FLOCALEID=2052
            where t1.FBILLNO in ('{dnNos.Replace("、", "','")}')
            union  
            select '{userWxCode}','{userName}' ";
            return DBUtils.ExecuteDynamicObject(ctx, sql);
        }

        //获取一笔客户地址
        private string GetCustAddress(Context ctx, string dnNo)
        {
            string sql = $@"/*dialect*/select top 1 FRECEIVEADDRESS from T_SAL_DELIVERYNOTICE where FBILLNO='{dnNo}'";
            return DBUtils.ExecuteScalar<string>(ctx, sql, "");
        }
    }

    [Description("发货通知单-打开物流方式变更申请列表插件"), HotUpdate]
    public class OpenSalDeliveryNoticeList : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            DynamicFormShowParameter para = new DynamicFormShowParameter();

            switch (e.BarItemKey)
            {
                case "PENY_tbDnLogisticsChangesList":
                    //选择的行,获取所有信息,放在listcoll里面
                    ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;
                    //获取所选行的主键,赋值给一个数组listKey
                    //接收返回的数组值
                    string[] listKey = listcoll.GetPrimaryKeyValues();

                    if (listcoll.Count() == 0)
                    {
                        this.View.ShowErrMessage($"请勾选订单");
                        return;
                    }

                    List<string> dnNos = new List<string>();
                    var sql = "";
                    foreach (string key in listcoll.GroupBy(g => g.BillNo).Select(s => s.Key))
                    {
                        //判断订单是否审批
                        sql = $@"/*dialect*/select top 1 FDOCUMENTSTATUS from T_SAL_DELIVERYNOTICE where FBILLNO='{key}' ";
                        if (!DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "").EqualsIgnoreCase("C"))
                        {
                            this.View.ShowErrMessage($"订单[{key}]必须是已审批的才能发起申请");
                            return;
                        }
                        //判断是否已经出库
                        sql = $@"/*dialect*/select top 1 FBILLNO from T_SAL_OUTSTOCK where FBILLNO='{key}' ";
                        if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "")))
                        {
                            this.View.ShowErrMessage($"订单[{key}]已出库,不能申请");
                            return;
                        }
                        //判断是否已经存在审批中的任务
                        sql = $@"/*dialect*/select count(1) from  T_SAL_DnLogisticsChanges where FDnNo like '%{key}%' and FSpStatus=1";
                        if (DBServiceHelper.ExecuteScalar<int>(this.Context, sql, 0) > 0)
                        {
                            this.View.ShowErrMessage($"订单{key}已存在审批中的物流方式变更申请");
                            return;
                        }
                        dnNos.Add(key);
                    }

                    //判断是否存在2个客户的订单
                    sql = $@"/*dialect*/select distinct t2.FNAME from T_SAL_DELIVERYNOTICE t1
                            inner join T_BD_CUSTOMER_L t2 on t1.FCUSTOMERID=t2.FCUSTID
                            where FBILLNO in('{string.Join("','", dnNos)}')";
                    if (DBServiceHelper.ExecuteDynamicObject(this.Context, sql).Count() != 1)
                    {
                        this.View.ShowErrMessage($"不能勾选客户不一致的订单一起申请");
                        return;
                    }

                    //判断是否存在2个不同仓库的订单
                    sql = $@"/*dialect*/select distinct case when t2.FSUPPLYTARGETORGID=7401782 then '大岭山非标件仓库'
                            when t2.FSUPPLYTARGETORGID=7401803 then '惠山非标件仓库' 
                            else '大岭山标准仓库' end Warehouse
                            from  T_SAL_DELIVERYNOTICE t1 
                            inner join T_SAL_DELIVERYNOTICEENTRY t2 on t1.FID=t2.FID
                            where t1.FBILLNO in('{string.Join("','", dnNos)}')";
                    if (DBServiceHelper.ExecuteDynamicObject(this.Context, sql).Count() != 1)
                    {
                        this.View.ShowErrMessage($"不能勾选发货仓库不一致的订单一起申请");
                        return;
                    }

                    //判断是否存在地址不一致的订单
                    sql = $@"/*dialect*/select distinct t1.FRECEIVEADDRESS from T_SAL_DELIVERYNOTICE t1
                            where FBILLNO in('{string.Join("','", dnNos)}')";
                    if (DBServiceHelper.ExecuteDynamicObject(this.Context, sql).Count() != 1)
                    {
                        this.View.ShowErrMessage($"不能勾选地址不一致的订单一起申请");
                        return;
                    }

                    para.OpenStyle.ShowType = ShowType.Modal;
                    para.FormId = "PENY_LogisticsChangesList";
                    para.CustomParams["DnNos"] = string.Join("、", dnNos);
                    this.View.ShowForm(para);
                    break;
                default:
                    break;
            }
        }
    }

    [Description("发货通知单表单-打开物流方式变更申请列表插件"), HotUpdate]
    public class OpenSalDeliveryNotice : AbstractDynamicFormPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            DynamicFormShowParameter para = new DynamicFormShowParameter();
            switch (e.BarItemKey)
            {
                case "PENY_tbDnLogisticsChanges":
                    para.OpenStyle.ShowType = ShowType.Modal;
                    //唯一标识
                    var orderNo = this.View.Model.DataObject["BillNo"].ToString();
                    para.FormId = "PENY_LogisticsChangesList";
                    para.CustomParams["DnNos"] = orderNo;

                    //判断订单是否审批
                    var sql = $@"/*dialect*/select top 1 FDOCUMENTSTATUS from T_SAL_DELIVERYNOTICE where FBILLNO='{orderNo}' ";
                    if (!DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "").EqualsIgnoreCase("C"))
                    {
                        this.View.ShowErrMessage($"订单[{orderNo}]必须是已审批的才能发起申请");
                        return;
                    }
                    //判断是否已经出库
                    sql = $@"/*dialect*/select top 1 FBILLNO from T_SAL_OUTSTOCK where FBILLNO='{orderNo}' ";
                    if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "")))
                    {
                        this.View.ShowErrMessage($"订单[{orderNo}]已出库,不能申请");
                        return;
                    }
                    //判断是否已经存在审批中的任务
                    sql = $@"/*dialect*/select count(1) from  T_SAL_DnLogisticsChanges where  FDnNo like '%{orderNo}%' and FSpStatus=1";
                    if (DBServiceHelper.ExecuteScalar<int>(this.Context, sql, 0) > 0)
                    {
                        this.View.ShowErrMessage($"订单{orderNo}已存在审批中的物流方式变更申请");
                        return;
                    }
                    this.View.ShowForm(para);
                    break;
                default:
                    break;
            }
        }
    }
}
