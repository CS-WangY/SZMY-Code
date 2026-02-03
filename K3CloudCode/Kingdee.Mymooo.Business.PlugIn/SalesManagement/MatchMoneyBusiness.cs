using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("匹配待查款动态表单插件"), HotUpdate]
    public class MatchMoneyBusiness : AbstractDynamicFormPlugIn
    {
        public static int BillEntityRowIndex;
        public static decimal EndAmount;
        public static MatchMoney MatchMoney;
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("FOrgID", this.Context.CurrentOrganizationInfo.ID);
            this.Model.SetValue("FOrgIDE2", this.Context.CurrentOrganizationInfo.ID);
            this.View.UpdateView("FOrgID");
            this.View.UpdateView("FOrgIDE2");
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            if (e.Key.EqualsIgnoreCase("FGetWorkbenchBill"))
            {
                this.Model.DeleteEntryData("FSaleBillEntity");
                this.Model.DeleteEntryData("FSKBillEntity");
                this.Model.DeleteEntryData("FYSKBillList");
                var url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/SalesOrder/QuerySalesOrderListByK3Cloud";
                var customerid = this.Model.GetValue("FCustomerID") as DynamicObject;
                if (customerid is null)
                {
                    this.View.ShowErrMessage("请选择往来单位！");
                    return;
                }
                var req = new
                {
                    companyCode = customerid["Number"].ToString()
                };
                var requestData = JsonConvertUtils.SerializeObject<dynamic>(req);
                ApiRequest request = JsonConvertUtils.DeserializeObject<ApiRequest>(ApigatewayUtils.InvokePostWebService(url, requestData));
                if (request.isSuccess)
                {
                    MatchMoney = JsonConvertUtils.DeserializeObject<MatchMoney>(request.data.ToString());
                    var orgnumber = GetOrgNumber(this.Context, this.Context.CurrentOrganizationInfo.ID);
                    foreach (var item in MatchMoney.OrderNews.Where(x => x.salesOrgCode == orgnumber))
                    {
                        int rowcount = this.Model.GetEntryRowCount("FSaleBillEntity");
                        this.Model.CreateNewEntryRow("FSaleBillEntity");
                        this.Model.SetItemValueByNumber("FCompanyID", MatchMoney.CompanyCode, rowcount);
                        this.Model.SetValue("FCompanyName", MatchMoney.CompanyName, rowcount);

                        this.Model.SetValue("FOrderNumber", item.OrderNumber, rowcount);
                        this.Model.SetValue("FTotalPrice", item.TotalPrice, rowcount);
                        this.Model.SetValue("FPaymentMethodCode", item.PaymentMethodCode, rowcount);

                        this.Model.SetValue("FPaymentMethodName", item.PaymentMethodName, rowcount);
                        this.Model.SetValue("FOrderTime", item.OrderTime, rowcount);
                        this.Model.SetValue("FSalesMan", item.SalesMan, rowcount);
                    }

                    this.View.UpdateView("FSaleBillEntity");
                }
                else
                {
                    this.View.ShowErrMessage(string.Join(",", request.errorMessage));
                }
            }
            if (e.Key.EqualsIgnoreCase("FSyncWorkbench"))
            {
                string sSql;

                List<SalesOrderK3CloudRequest> k3reques = new List<SalesOrderK3CloudRequest>();
                foreach (var item in MatchMoney.OrderNews)
                {
                    sSql = $"select * from PENY_t_MatchMoneyEntry where FSALBILLNO='{item.OrderNumber}' and FISCHECK=0";
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    if (datas.Count > 0)
                    {
                        var entje = datas.GroupBy(s => new
                        {
                            FSALBILLNO = Convert.ToString(s["FSALBILLNO"])
                        }).Select(g => new
                        {
                            FQty = g.Sum(x => Convert.ToDecimal(x["FAMOUNT"]))
                        }).ToList();
                        if (entje[0].FQty != item.TotalPrice)
                        {
                            this.View.ShowErrMessage("订单金额与核销匹配金额不一致,请重新获取订单信息重新分配金额！");
                            return;
                        }

                        SalesOrderK3CloudRequest SOKR = new SalesOrderK3CloudRequest();
                        SOKR.OrderCode = item.OrderNumber;
                        SOKR.OrderAmount = item.TotalPrice;

                        List<PlanEntry> plan = new List<PlanEntry>();
                        foreach (var drow in datas)
                        {
                            var planitem = new PlanEntry();
                            planitem.FMMEID = Convert.ToInt64(drow["FID"]);
                            planitem.AdvanceNo = Convert.ToString(drow["FADVANCENO"]);
                            planitem.ADVANCEID = Convert.ToInt64(drow["FADVANCEID"]);
                            planitem.AdvanceSeq = Convert.ToInt32(drow["FADVANCESEQ"]);
                            planitem.ADVANCEENTRYID = Convert.ToInt64(drow["FADVANCEENTRYID"]);
                            planitem.ActRecAmount = Convert.ToDecimal(drow["FAMOUNT"]);
                            planitem.RemainAmount = Convert.ToDecimal(drow["FREMAINAMOUNT"]);
                            planitem.PREMATCHAMOUNTFOR = 0;
                            planitem.SettleOrgId_Id = Convert.ToInt64(drow["FSETTLEORGID"]);
                            planitem.Seq = Convert.ToInt32(drow["FSEQ"]);
                            plan.Add(planitem);
                        }
                        SOKR.Planentries = plan;
                        k3reques.Add(SOKR);
                    }
                }

                if (k3reques.Count <= 0)
                {
                    this.View.ShowErrMessage("没有可同步的匹配记录！");
                    return;
                }
                string requestData = JsonConvertUtils.SerializeObject(k3reques);
                var url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/SalesOrder/AuditOrderByK3Cloud";
                var req = ApigatewayUtils.InvokePostWebService(url, requestData);
                //if (Convert.ToBoolean(JSONObject.Parse(req)["IsSuccess"])){}

                ((IDynamicFormViewService)this.View).ButtonClick("FGetWorkbenchBill", string.Empty);
            }
            if (e.Key.EqualsIgnoreCase("FMatching"))
            {
                if (EndAmount > 0)
                {
                    this.View.ShowErrMessage("匹配金额不足！");
                    return;
                }
                DynamicObjectCollection skEntity = this.Model.DataObject["FSKBillEntity"] as DynamicObjectCollection;
                foreach (var item in skEntity)
                {
                    if (Convert.ToBoolean(item["FISCheck"]))
                    {
                        if (Convert.ToDecimal(item["FAMOUNT"]) == 0) continue;

                        int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FSaleBillEntity");
                        DynamicObject selectedEntityObj = (this.View.Model.DataObject["FSaleBillEntity"] as DynamicObjectCollection)[rowIndex];
                        var FSalBillNo = Convert.ToString(selectedEntityObj["FOrderNumber"]);
                        var FSalBillSeq = "1";

                        var FREAMOUNT = Convert.ToDecimal(item["FREAMOUNT"]);
                        var FAMOUNT = Convert.ToDecimal(item["FAMOUNT"]);
                        var FADVANCENO = Convert.ToString(item["FSKBILLNO"]);
                        var FADVANCEID = Convert.ToString(item["FSKID"]);
                        var FADVANCESEQ = Convert.ToString(item["FSKSEQ"]);
                        var FADVANCEENTRYID = Convert.ToString(item["FSKENTRYID"]);
                        var FSETTLEORGID = Convert.ToString((item["FSETTLEORGID"] as DynamicObject)["Id"]);

                        var ids = Kingdee.BOS.ServiceHelper.DBServiceHelper.GetSequenceInt64(this.Context, "PENY_t_MatchMoneyEntry", 1);
                        string sSql = $@"INSERT INTO PENY_t_MatchMoneyEntry
                               (
		                       [FID]
                               --,[FFormId]
                               --,[FSEQ]
                               ,[FREMAINAMOUNT]
                               ,[FAMOUNT]
                               ,[FADVANCENO]
                               ,[FADVANCEID]
                               ,[FADVANCESEQ]
                               ,[FADVANCEENTRYID]
                               ,[FSETTLEORGID]
                               ,[FSalBillNo]
                               ,[FSALBILLSEQ]
		                       )
                         VALUES
                               (
		                       {ids.ElementAt(0)}
                               --,'ff'
                               --,1
                               ,{FREAMOUNT}
                               ,{FAMOUNT}
                               ,'{FADVANCENO}'
                               ,{FADVANCEID}
                               ,'{FADVANCESEQ}'
                               ,{FADVANCEENTRYID}
                               ,{FSETTLEORGID}
                               ,{FSalBillNo}
                               ,{FSalBillSeq}
		                       )";
                        DBServiceHelper.Execute(this.Context, sSql);
                    }
                }
                //this.View.UpdateView("FSKBillEntity");

                EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FSaleBillEntity");
                entryGrid.SetFocusRowIndex(BillEntityRowIndex);
            }
            if (e.Key.EqualsIgnoreCase("FDelete"))
            {
                DynamicObjectCollection skEntity = this.Model.DataObject["FYSKBillList"] as DynamicObjectCollection;
                foreach (var item in skEntity)
                {
                    if (Convert.ToBoolean(item["FCheckBoxE2"]))
                    {
                        var id = Convert.ToInt64(item["FID"]);
                        string sSql = $"delete PENY_t_MatchMoneyEntry where FID={id}";
                        DBServiceHelper.Execute(this.Context, sSql);
                    }
                }
                EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FSaleBillEntity");
                entryGrid.SetFocusRowIndex(BillEntityRowIndex);
            }
            if (e.Key.EqualsIgnoreCase("FGetSystemBill"))
            {
                var customerid = this.Model.GetValue("FCustomerIDE2") as DynamicObject;
                if (customerid is null)
                {
                    this.View.ShowErrMessage("请选择往来单位！");
                    return;
                }
                this.Model.DeleteEntryData("FSalEntity");
                this.Model.DeleteEntryData("FRECEntity");
                string sSql = $@"SELECT t1.FID,t1.FCUSTID,t1.FBILLNO,t2.FRECADVANCEAMOUNT,t1.FDATE,t1.FSALERID,t2.FRELBILLNO,t2.FRECAMOUNT
                                FROM T_SAL_ORDER t1 INNER JOIN T_SAL_ORDERPLAN t2 ON t1.FID=t2.FID
                                WHERE t2.FRECADVANCEAMOUNT>t2.FRECAMOUNT AND FCANCELSTATUS='A' AND t1.FCLOSESTATUS='A' AND t1.FSALEORGID={this.Context.CurrentOrganizationInfo.ID} AND t1.FCUSTID={customerid["Id"]}";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.DeleteEntryData("FSalEntity");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FSalEntity");
                    this.Model.CreateNewEntryRow("FSalEntity");
                    this.Model.SetItemValueByID("FCUSTID", item["FCUSTID"].ToString(), rowcount);
                    this.Model.SetValue("FBILLNO", item["FBILLNO"].ToString(), rowcount);
                    this.Model.SetValue("FSalAmount", item["FRECADVANCEAMOUNT"].ToString(), rowcount);
                    this.Model.SetValue("FSalDate", item["FDATE"].ToString(), rowcount);
                    this.Model.SetItemValueByID("FSALERID", item["FSALERID"].ToString(), rowcount);
                    this.Model.SetValue("FRELBILLNO", item["FRELBILLNO"].ToString(), rowcount);
                    this.Model.SetValue("FRECAMOUNT", item["FRECAMOUNT"].ToString(), rowcount);
                    this.Model.SetValue("FSalBillID", item["FID"].ToString(), rowcount);
                }
                this.View.UpdateView("FSalEntity");
            }
        }

        private string GetOrgNumber(Context ctx, long orgid)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@Orgid", KDDbType.Int64, orgid) };
            var sql = $@"/*dialect*/select top 1 FNUMBER from  t_org_organizations org 
             inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=2052
			 where org.FORGID=@Orgid";
            return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }

        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            if (e.Key.EqualsIgnoreCase("FSaleBillEntity"))
            {
                BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FSaleBillEntity");
                DynamicObject orgid = this.Model.GetValue("FOrgID") as DynamicObject;
                DynamicObject customerid = this.Model.GetValue("FCompanyID", e.Row) as DynamicObject;
                decimal salbillAmount = Convert.ToDecimal(this.Model.GetValue("FTotalPrice", e.Row));

                var FSalBillNo = this.Model.GetValue("FOrderNumber", e.Row).ToString();

                string sSql = $"select SUM(FAMOUNT) as FAMOUNT from PENY_t_MatchMoneyEntry where FSALBILLNO='{FSalBillNo}'";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                EndAmount = salbillAmount - Convert.ToDecimal(datas[0]["FAMOUNT"]);

                if (customerid != null)
                {
                    //收款单收款金额-已关联的金额-已匹配的金额-已核销的金额+销售订单禁用的剩余金额
                    sSql = $@"select t2.FBILLNO,t2.FCONTACTUNIT,t2.FDATE,t2.FBILLTYPEID,t2.FSETTLEORGID,t1.*
                            ,t1.FRECTOTALAMOUNTFOR-t1.FASSTOTALAMOUNTFOR-ISNULL(SK.FAMOUNT,0)-ISNULL(HX.FCURWRITTENOFFAMOUNTFOR, 0)-ISNULL(TK.FREFUNDAMOUNT,0) as FLINKAMOUNT
                            ,t1.FRECTOTALAMOUNTFOR AS SKA--收款金额
                            ,t1.FASSTOTALAMOUNTFOR AS GLA--关联金额
                            ,ISNULL(SK.FAMOUNT,0) AS PLA--匹配未生成金额
                            ,ISNULL(HX.FCURWRITTENOFFAMOUNTFOR,0) AS HXA--手工核销金额
                            --,ISNULL(GB.FSYAMOUNT,0) AS GBA--关闭订单的金额
                            ,ISNULL(TK.FREFUNDAMOUNT,0) AS TKA--退款核销金额
                            from T_AR_RECEIVEBILLENTRY t1
                            inner join T_AR_RECEIVEBILL t2 on t1.FID=t2.FID
                            left join (
                            select FADVANCEID,FADVANCEENTRYID,SUM(FAMOUNT) FAMOUNT from PENY_t_MatchMoneyEntry
                            where FISCHECK=0
                            group by FADVANCEID,FADVANCEENTRYID
                            ) SK on t1.FID=SK.FADVANCEID and t1.FENTRYID=SK.FADVANCEENTRYID 
                            LEFT JOIN (
                            SELECT t1.FSRCBILLID,t1.FSRCROWID,SUM(t1.FCURWRITTENOFFAMOUNTFOR) AS FCURWRITTENOFFAMOUNTFOR FROM T_AR_RECMACTHLOGENTRY t1
                            LEFT JOIN T_AR_RECMACTHLOG t2 on t1.FID=t2.FID
                            WHERE t2.FIsJoinMatch=0
                            GROUP BY t1.FSRCBILLID,t1.FSRCROWID
                            ) HX ON t1.FID=HX.FSRCBILLID AND t1.FENTRYID=HX.FSRCROWID
                            --LEFT JOIN (
                            --SELECT FADVANCEID,FADVANCEENTRYID,SUM(t1.FAMOUNT) AS FAMOUNT,SUM(t1.FPREMATCHAMOUNTFOR) AS FMATCHAMOUNT
                            --,SUM(t1.FAMOUNT)-SUM(t1.FPREMATCHAMOUNTFOR) AS FSYAMOUNT
                            --FROM T_SAL_ORDERPLANENTRY t1
                            --LEFT JOIN T_SAL_ORDERPLAN t2 ON t1.FENTRYID=t2.FENTRYID
                            --LEFT JOIN T_SAL_ORDER t3 ON t2.FID=t3.FID
                            --WHERE t3.FCLOSESTATUS='B'
                            --GROUP BY FADVANCEID,FADVANCEENTRYID
                            --) GB ON t1.FID=GB.FADVANCEID AND t1.FENTRYID=GB.FADVANCEENTRYID
                            LEFT JOIN (
                            SELECT t1.FSRCBILLID,t1.FSRCROWID,SUM(t1.FCURWRITTENOFFAMOUNTFOR) AS FREFUNDAMOUNT FROM T_AR_RECMACTHLOGENTRY t1
                            LEFT JOIN T_AR_RECMACTHLOG t2 on t1.FID=t2.FID
                            WHERE t1.FTARGETFROMID='AR_REFUNDBILL'
                            GROUP BY t1.FSRCBILLID,t1.FSRCROWID
                            ) TK ON t1.FID=TK.FSRCBILLID AND t1.FENTRYID=TK.FSRCROWID
                            where t1.FRECTOTALAMOUNTFOR-t1.FASSTOTALAMOUNTFOR-ISNULL(SK.FAMOUNT,0)-ISNULL(HX.FCURWRITTENOFFAMOUNTFOR, 0)-ISNULL(TK.FREFUNDAMOUNT,0) > 0
                            AND t2.FDOCUMENTSTATUS='C'
                            AND t2.FPAYORGID={orgid["Id"]}
                            AND t2.FCONTACTUNIT={customerid["Id"]}";
                    datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    this.Model.DeleteEntryData("FSKBillEntity");
                    foreach (var item in datas)
                    {
                        int rowcount = this.Model.GetEntryRowCount("FSKBillEntity");
                        this.Model.CreateNewEntryRow("FSKBillEntity");
                        this.Model.SetValue("FSKID", item["FID"].ToString(), rowcount);
                        this.Model.SetValue("FSKENTRYID", item["FENTRYID"].ToString(), rowcount);
                        this.Model.SetValue("FSKBILLNO", item["FBILLNO"].ToString(), rowcount);
                        this.Model.SetValue("FSKSEQ", item["FSEQ"].ToString(), rowcount);
                        this.Model.SetValue("FAMOUNT", 0, rowcount);
                        this.Model.SetValue("FASSAMOUNTFOR", item["FLINKAMOUNT"].ToString(), rowcount);
                        this.Model.SetValue("FREAMOUNT", item["SKA"].ToString(), rowcount);
                        this.Model.SetValue("FSETTLETYPEID", item["FSETTLETYPEID"].ToString(), rowcount);
                        this.Model.SetValue("FCONTACTUNIT", item["FCONTACTUNIT"].ToString(), rowcount);
                        this.Model.SetValue("FPayDate", item["FDATE"].ToString(), rowcount);
                        this.Model.SetValue("FBILLTYPEID", item["FBILLTYPEID"].ToString(), rowcount);
                        this.Model.SetValue("FSETTLEORGID", item["FSETTLEORGID"].ToString(), rowcount);
                    }
                    this.View.UpdateView("FSKBillEntity");
                }
                //加载已匹配
                sSql = $@"select * from PENY_t_MatchMoneyEntry where FSALBILLNO='{FSalBillNo}'";
                datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.DeleteEntryData("FYSKBillList");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FYSKBillList");
                    this.Model.CreateNewEntryRow("FYSKBillList");
                    this.Model.SetValue("FID", item["FID"].ToString(), rowcount);
                    this.Model.SetValue("FGLAMOUNT", item["FAMOUNT"].ToString(), rowcount);
                    this.Model.SetValue("FYSBILLNO", item["FADVANCENO"].ToString(), rowcount);
                    this.Model.SetValue("FYSSEQ", item["FADVANCESEQ"].ToString(), rowcount);
                    this.Model.SetValue("FSID", item["FADVANCEID"].ToString(), rowcount);
                    this.Model.SetValue("FSEID", item["FADVANCEENTRYID"].ToString(), rowcount);
                    this.Model.SetValue("FSALBILLNO", item["FSalBillNo"].ToString(), rowcount);
                    this.Model.SetValue("FSALBILLSEQ", item["FSALBILLSEQ"].ToString(), rowcount);
                    this.Model.SetValue("FCheckBox", item["FISCHECK"], rowcount);
                    this.Model.SetItemValueByID("FSETTLEORG", item["FSETTLEORGID"], rowcount);
                }
                this.View.UpdateView("FYSKBillList");
            }
            if (e.Key.EqualsIgnoreCase("FSalEntity"))
            {
                BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FSalEntity");
                DynamicObject orgid = this.Model.GetValue("FOrgIDE2") as DynamicObject;
                DynamicObject customerid = this.Model.GetValue("FCustomerIDE2") as DynamicObject;
                decimal salbillAmount = Convert.ToDecimal(this.Model.GetValue("FSalAmount", e.Row));
                decimal recAmount = Convert.ToDecimal(this.Model.GetValue("FRECAMOUNT", e.Row));
                EndAmount = salbillAmount - recAmount;
                if (customerid != null)
                {
                    //收款单收款金额-已关联的金额-已匹配的金额-已核销的金额+销售订单禁用的剩余金额
                    string sSql = $@"select t2.FBILLNO,t2.FCONTACTUNIT,t2.FDATE,t2.FBILLTYPEID,t2.FSETTLEORGID,t1.*
                            ,t1.FRECTOTALAMOUNTFOR-t1.FASSTOTALAMOUNTFOR-ISNULL(SK.FAMOUNT,0)-ISNULL(HX.FCURWRITTENOFFAMOUNTFOR, 0)-ISNULL(TK.FREFUNDAMOUNT,0) as FLINKAMOUNT
                            ,t1.FRECTOTALAMOUNTFOR AS SKA--收款金额
                            ,t1.FASSTOTALAMOUNTFOR AS GLA--关联金额
                            ,ISNULL(SK.FAMOUNT,0) AS PLA--匹配未生成金额
                            ,ISNULL(HX.FCURWRITTENOFFAMOUNTFOR,0) AS HXA--手工核销金额
                            --,ISNULL(GB.FSYAMOUNT,0) AS GBA--关闭订单的金额
                            ,ISNULL(TK.FREFUNDAMOUNT,0) AS TKA--退款核销金额
                            from T_AR_RECEIVEBILLENTRY t1
                            inner join T_AR_RECEIVEBILL t2 on t1.FID=t2.FID
                            left join (
                            select FADVANCEID,FADVANCEENTRYID,SUM(FAMOUNT) FAMOUNT from PENY_t_MatchMoneyEntry
                            where FISCHECK=0
                            group by FADVANCEID,FADVANCEENTRYID
                            ) SK on t1.FID=SK.FADVANCEID and t1.FENTRYID=SK.FADVANCEENTRYID 
                            LEFT JOIN (
                            SELECT t1.FSRCBILLID,t1.FSRCROWID,SUM(t1.FCURWRITTENOFFAMOUNTFOR) AS FCURWRITTENOFFAMOUNTFOR FROM T_AR_RECMACTHLOGENTRY t1
                            LEFT JOIN T_AR_RECMACTHLOG t2 on t1.FID=t2.FID
                            WHERE t2.FIsJoinMatch=0
                            GROUP BY t1.FSRCBILLID,t1.FSRCROWID
                            ) HX ON t1.FID=HX.FSRCBILLID AND t1.FENTRYID=HX.FSRCROWID
                            --LEFT JOIN (
                            --SELECT FADVANCEID,FADVANCEENTRYID,SUM(t1.FAMOUNT) AS FAMOUNT,SUM(t1.FPREMATCHAMOUNTFOR) AS FMATCHAMOUNT
                            --,SUM(t1.FAMOUNT)-SUM(t1.FPREMATCHAMOUNTFOR) AS FSYAMOUNT
                            --FROM T_SAL_ORDERPLANENTRY t1
                            --LEFT JOIN T_SAL_ORDERPLAN t2 ON t1.FENTRYID=t2.FENTRYID
                            --LEFT JOIN T_SAL_ORDER t3 ON t2.FID=t3.FID
                            --WHERE t3.FCLOSESTATUS='B'
                            --GROUP BY FADVANCEID,FADVANCEENTRYID
                            --) GB ON t1.FID=GB.FADVANCEID AND t1.FENTRYID=GB.FADVANCEENTRYID
                            LEFT JOIN (
                            SELECT t1.FSRCBILLID,t1.FSRCROWID,SUM(t1.FCURWRITTENOFFAMOUNTFOR) AS FREFUNDAMOUNT FROM T_AR_RECMACTHLOGENTRY t1
                            LEFT JOIN T_AR_RECMACTHLOG t2 on t1.FID=t2.FID
                            WHERE t1.FTARGETFROMID='AR_REFUNDBILL'
                            GROUP BY t1.FSRCBILLID,t1.FSRCROWID
                            ) TK ON t1.FID=TK.FSRCBILLID AND t1.FENTRYID=TK.FSRCROWID
                            where t1.FRECTOTALAMOUNTFOR-t1.FASSTOTALAMOUNTFOR-ISNULL(SK.FAMOUNT,0)-ISNULL(HX.FCURWRITTENOFFAMOUNTFOR, 0)-ISNULL(TK.FREFUNDAMOUNT,0) > 0
                            AND t2.FDOCUMENTSTATUS='C'
                            AND t2.FPAYORGID={orgid["Id"]}
                            AND t2.FCONTACTUNIT={customerid["Id"]}";
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    this.Model.DeleteEntryData("FRECEntity");
                    foreach (var item in datas)
                    {
                        int rowcount = this.Model.GetEntryRowCount("FRECEntity");
                        this.Model.CreateNewEntryRow("FRECEntity");
                        this.Model.SetValue("FSKIDE2", item["FID"].ToString(), rowcount);
                        this.Model.SetValue("FSKENTRYIDE2", item["FENTRYID"].ToString(), rowcount);
                        this.Model.SetValue("FSKBILLNOE2", item["FBILLNO"].ToString(), rowcount);
                        this.Model.SetValue("FSKSEQE2", item["FSEQ"].ToString(), rowcount);
                        this.Model.SetValue("FAMOUNTE2", 0, rowcount);
                        this.Model.SetValue("FASSAMOUNTFORE2", item["FLINKAMOUNT"].ToString(), rowcount);
                        this.Model.SetValue("FREAMOUNTE2", item["SKA"].ToString(), rowcount);
                        this.Model.SetValue("FSETTLETYPEIDE2", item["FSETTLETYPEID"].ToString(), rowcount);
                        this.Model.SetValue("FCONTACTUNITE2", item["FCONTACTUNIT"].ToString(), rowcount);
                        this.Model.SetValue("FPayDateE2", item["FDATE"].ToString(), rowcount);
                        this.Model.SetValue("FBILLTYPEIDE2", item["FBILLTYPEID"].ToString(), rowcount);
                        this.Model.SetValue("FSETTLEORGIDE2", item["FSETTLEORGID"].ToString(), rowcount);

                    }
                    this.View.UpdateView("FRECEntity");
                }
            }
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.EqualsIgnoreCase("FISCheck"))
            {
                //取收款单信息
                //var rowIndex = this.View.Model.GetEntryCurrentRowIndex("FSKBillEntity");
                var selectedEntityObj = (this.View.Model.DataObject["FSKBillEntity"] as DynamicObjectCollection)[e.Row];
                var FSKID = Convert.ToString(selectedEntityObj["FSKID"]);
                var FSKENTRYID = Convert.ToString(selectedEntityObj["FSKENTRYID"]);
                var FASSAMOUNTFOR = Convert.ToDecimal(selectedEntityObj["FASSAMOUNTFOR"]);
                if (Convert.ToBoolean(selectedEntityObj["FISCheck"]))
                {
                    if (EndAmount - FASSAMOUNTFOR <= 0)
                    {
                        this.View.Model.SetValue("FAMOUNT", EndAmount, e.Row);
                        EndAmount = 0;
                    }
                    else
                    {
                        this.View.Model.SetValue("FAMOUNT", FASSAMOUNTFOR, e.Row);
                        EndAmount -= FASSAMOUNTFOR;
                    }
                }
                else
                {
                    EndAmount += Convert.ToDecimal(this.View.Model.GetValue("FAMOUNT", e.Row));
                    this.View.Model.SetValue("FAMOUNT", 0, e.Row);
                }
                this.View.UpdateView("FAMOUNT");
            }
            if (e.Field.Key.EqualsIgnoreCase("FISCheckE2"))
            {
                //取收款单信息
                //var rowIndex = this.View.Model.GetEntryCurrentRowIndex("FSKBillEntity");
                var selectedEntityObj = (this.View.Model.DataObject["FRECEntity"] as DynamicObjectCollection)[e.Row];
                var FSKID = Convert.ToString(selectedEntityObj["FSKIDE2"]);
                var FSKENTRYID = Convert.ToString(selectedEntityObj["FSKENTRYIDE2"]);
                var FASSAMOUNTFOR = Convert.ToDecimal(selectedEntityObj["FASSAMOUNTFORE2"]);
                if (Convert.ToBoolean(selectedEntityObj["FISCheckE2"]))
                {
                    if (EndAmount - FASSAMOUNTFOR <= 0)
                    {
                        this.View.Model.SetValue("FAMOUNTE2", EndAmount, e.Row);
                        EndAmount = 0;
                    }
                    else
                    {
                        this.View.Model.SetValue("FAMOUNTE2", FASSAMOUNTFOR, e.Row);
                        EndAmount = EndAmount - FASSAMOUNTFOR;
                    }
                }
                else
                {
                    EndAmount += Convert.ToDecimal(this.View.Model.GetValue("FAMOUNTE2", e.Row));
                    this.View.Model.SetValue("FAMOUNTE2", 0, e.Row);
                }
                this.View.UpdateView("FAMOUNTE2");
            }
        }

        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            if (e.BarItemKey.EqualsIgnoreCase("PENY_tbButton"))
            {
                //using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
                //{
                BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FSalEntity");
                var selectedEntityObj = (this.View.Model.DataObject["FSalEntity"] as DynamicObjectCollection)[BillEntityRowIndex];
                var FSalBillID = selectedEntityObj["FSalBillID"];
                var billView = FormMetadataUtils.CreateBillView(this.Context, "SAL_SaleOrder", FSalBillID);
                var planEntrys = billView.Model.DataObject["SaleOrderPlan"] as DynamicObjectCollection;
                DynamicObjectCollection skEntity = this.Model.DataObject["FRECEntity"] as DynamicObjectCollection;

                foreach (var item in skEntity)
                {
                    if (Convert.ToBoolean(item["FISCheckE2"]))
                    {
                        billView.Model.CreateNewEntryRow("FSaleOrderPlanEntry");
                        int rowcount = billView.Model.GetEntryRowCount("FSaleOrderPlanEntry");
                        billView.Model.SetValue("FSEQ", rowcount, rowcount - 1);
                        billView.Model.SetValue("FADVANCENO", item["FSKBILLNOE2"], rowcount - 1);
                        billView.Model.SetValue("FADVANCEID", item["FSKIDE2"], rowcount - 1);
                        billView.Model.SetValue("FADVANCESEQ", item["FSKSEQE2"], rowcount - 1);
                        billView.Model.SetValue("FADVANCEENTRYID", item["FSKENTRYIDE2"], rowcount - 1);
                        billView.Model.SetValue("FActRecAmount", item["FAMOUNTE2"], rowcount - 1);
                        //billView.Model.SetValue("FRemainAmount", item.RemainAmount, rowcount);
                        //billView.Model.SetValue("FPreMatchAmountFor", 0, row);
                        billView.Model.SetItemValueByID("FPESettleOrgId", item["FSETTLEORGIDE2_Id"], rowcount - 1);
                        //关联不会更新收款单已关联金额，sql更新
                        var FSalBillNo = Convert.ToString(selectedEntityObj["FBILLNO"]);
                        var FSalBillSeq = "1";

                        var FREAMOUNT = Convert.ToDecimal(item["FAMOUNTE2"]);
                        var FAMOUNT = Convert.ToDecimal(item["FAMOUNTE2"]);
                        var FADVANCENO = Convert.ToString(item["FSKBILLNOE2"]);
                        var FADVANCEID = Convert.ToString(item["FSKIDE2"]);
                        var FADVANCESEQ = Convert.ToString(item["FSKSEQE2"]);
                        var FADVANCEENTRYID = Convert.ToString(item["FSKENTRYIDE2"]);
                        var FSETTLEORGID = Convert.ToString((item["FSETTLEORGIDE2"] as DynamicObject)["Id"]);

                        var ids = Kingdee.BOS.ServiceHelper.DBServiceHelper.GetSequenceInt64(this.Context, "PENY_t_MatchMoneyEntry", 1);
                        string sSql = $@"INSERT INTO PENY_t_MatchMoneyEntry
                               (
		                       [FID]
                               --,[FFormId]
                               --,[FSEQ]
                               ,[FREMAINAMOUNT]
                               ,[FAMOUNT]
                               ,[FADVANCENO]
                               ,[FADVANCEID]
                               ,[FADVANCESEQ]
                               ,[FADVANCEENTRYID]
                               ,[FSETTLEORGID]
                               ,[FSalBillNo]
                               ,[FSALBILLSEQ]
		                       )
                         VALUES
                               (
		                       {ids.ElementAt(0)}
                               --,'ff'
                               --,1
                               ,{FREAMOUNT}
                               ,{FAMOUNT}
                               ,'{FADVANCENO}'
                               ,{FADVANCEID}
                               ,'{FADVANCESEQ}'
                               ,{FADVANCEENTRYID}
                               ,{FSETTLEORGID}
                               ,'{FSalBillNo}'
                               ,{FSalBillSeq}
		                       )";
                        DBServiceHelper.Execute(this.Context, sSql);
                    }
                }
                var soplan = planEntrys.FirstOrDefault();
                var relbillno = string.Join(",", ((DynamicObjectCollection)soplan["SAL_ORDERPLANENTRY"]).Select(x => x["AdvanceNo"]));

                soplan["RelBillNo"] = relbillno;
                soplan["RecAmount"] = ((DynamicObjectCollection)soplan["SAL_ORDERPLANENTRY"]).Sum(x => Convert.ToDecimal(x["ActRecAmount"]));
                //soplan["RecAdvanceRate"] = Math.Round(advanceRecAmount / request.SalesOrderVatNet * 100, 2);

                SaveService saveService = new SaveService();
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);


                var oper = saveService.Save(this.Context, new DynamicObject[] { billView.Model.DataObject }, operateOption);
                SaleSendMQ(Convert.ToInt64(FSalBillID), oper.Select(x => Convert.ToString(x["BillNo"])).FirstOrDefault());


                //清除释放网控
                billView.CommitNetworkCtrl();
                billView.InvokeFormOperation(FormOperationEnum.Close);
                billView.Close();

                this.View.Model.SetValue("FRELBILLNO", soplan["RelBillNo"], BillEntityRowIndex);
                this.View.Model.SetValue("FRECAMOUNT", soplan["RecAmount"], BillEntityRowIndex);
                EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FSalEntity");
                entryGrid.SetFocusRowIndex(BillEntityRowIndex);
                this.View.ShowMessage("匹配成功");

            }
        }

        private void SaleSendMQ(long SalBillID, string SalBillNo)
        {
            var planentrySql = @"SELECT 
CASE MAX(t4.FNUMBER)
WHEN 'WeChat' THEN 0
WHEN 'Alipay' THEN 1
ELSE 2 END PayType
,MAX(t5.FAPPROVEDATE) PaidDateTime
,SUM(t1.FAMOUNT) PaidAmount,tb.FBILLNO
FROM dbo.T_SAL_ORDERPLANENTRY t1
INNER JOIN T_SAL_ORDERPLAN t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_ORDER tb ON t2.FID=tb.FID
INNER JOIN T_AR_RECEIVEBILLENTRY t3 ON t1.FADVANCEENTRYID=t3.FENTRYID
INNER JOIN T_BD_SETTLETYPE t4 ON t3.FSETTLETYPEID=t4.FID
INNER JOIN T_AR_RECEIVEBILL t5 ON t3.FID=t5.FID
WHERE t2.FID=@FID
GROUP BY tb.FBILLNO";
            SqlParam sqlParam = new SqlParam("@FID", KDDbType.Int64, SalBillID);
            var planentrys = DBServiceHelper.ExecuteDynamicObject(this.Context, planentrySql, paramList: new SqlParam[] { sqlParam }).FirstOrDefault();
            var request = new SyncOrderPaymentRequest
            {
                OrderNumber = SalBillNo
            };
            if (planentrys != null)
            {
                request.PayType = planentrys.GetValue<int>("PayType", 0);
                request.PaidDateTime = planentrys.GetValue<DateTime?>("PaidDateTime", null);
                request.PaidAmount = planentrys.GetValue<decimal>("PaidAmount", 0);
                request.OrderNumber = planentrys.GetValue<string>("FBILLNO", string.Empty);

            }
            var sSql = $@"insert into RabbitMQMessage_{DateTime.Now:yyyyMM01}(MessageId,VirtualHost,Exchange,Routingkey,Message,Keyword,CreateDate,CreateUserId)
                        values(@MessageId,'/erp',@Exchange,@Routingkey,@Message,@Keyword,@CreateDate,@CreateUserId)";
            var result = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/GetMessageId?count=1");
            var messageIds = JsonConvertUtils.DeserializeObject<ResponseMessage<List<long>>>(result);
            if (!messageIds.IsSuccess)
            {
                throw new Exception(messageIds.Message ?? messageIds.ErrorMessage);
            }

            long messageId = messageIds.Data[0];

            List<SqlParam> sqlParams = new List<SqlParam>()
                {
                    new SqlParam("@MessageId", KDDbType.Int64, messageId),
                    new SqlParam("@Exchange", KDDbType.String, "mall-salesOrder"),
                    new SqlParam("@Routingkey", KDDbType.String, "SyncOrderPayStatus"),
                    new SqlParam("@Message", KDDbType.String, JsonConvertUtils.SerializeObject(request)),
                    new SqlParam("@Keyword", KDDbType.String, request.OrderNumber),
                    new SqlParam("@CreateDate", KDDbType.DateTime, DateTime.Now),
                    new SqlParam("@CreateUserId", KDDbType.Int64, this.Context.UserId),
                    new SqlParam("@EnvCode", KDDbType.String, ApigatewayUtils.ApigatewayConfig.EnvCode),
                };
            DBServiceHelper.Execute(this.Context, sSql, sqlParams);

            Task.Factory.StartNew(() =>
            {
                //晚5个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(5000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }
    }
}
