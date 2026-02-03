using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Kingdee.K3.Core.MFG.MFGBillTypeConst.PLN;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("客户过滤插件"), HotUpdate]
    public class CustomerListPlugIn : AbstractListPlugIn
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            var userinfo = ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
            long userId = userinfo.FEMPID;
            //客户列表不区分组织，和其他单据需要分开判断
            var formId = this.View.BillBusinessInfo.GetForm().Id;
            //客户列表
            if (formId.EqualsIgnoreCase("BD_Customer"))
            {
                //是否存在客户管理员
                string sql = $@"select 1 from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID}  and t3.FNUMBER = 'KHGLY'";
                if (!DBServiceHelper.ExecuteScalar<bool>(this.Context, sql, false))
                {
                    string filter = $@" 
                    exists(
                        select 1
                        from T_SAL_SCSALERCUST sg
	                        inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
	                        inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY'
	                        inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID
                        where staff.FEmpInfoId = {userId} and t0.FCUSTID=sg.FCUSTOMERID)";

                    e.FilterString = e.FilterString.IsNullOrEmptyOrWhiteSpace() ? filter : e.FilterString + " and " + filter;
                }
            }
            else
            {

                //是否存在客户管理员
                List<long> orgList = new List<long>();
                string sql = $@"select t2.FORGID from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID}  and t3.FNUMBER = 'KHGLY'";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                foreach (var item in datas)
                {
                    orgList.Add(Convert.ToInt64(item["FORGID"]));
                }
                //客户字段
                string custStr = "t0.FCUSTOMERID";
                //组织字段
                string orgStr = "t0.FSALEORGID";
                if (formId.EqualsIgnoreCase("SAL_SaleOrder") || formId.EqualsIgnoreCase("SAL_XORDER"))//销售订单
                {
                    custStr = "t0.FCustId";
                    orgStr = "t0.FSALEORGID";
                }
                else if (formId.EqualsIgnoreCase("SAL_DELIVERYNOTICE"))//发货通知单
                {
                    custStr = "t0.FCUSTOMERID";
                    orgStr = "t0.FDELIVERYORGID";
                }
                else if (formId.EqualsIgnoreCase("SAL_OUTSTOCK"))//销售出库单
                {
                    custStr = "t0.FCustomerID";
                    orgStr = "t0.FSTOCKORGID";
                }
                else if (formId.EqualsIgnoreCase("SAL_RETURNNOTICE"))//退货通知单
                {
                    custStr = "t0.FRetcustId";
                    orgStr = "t0.FRETORGID";
                }
                else if (formId.EqualsIgnoreCase("SAL_RETURNSTOCK"))//销售退货单
                {
                    custStr = "t0.FRetcustId";
                    orgStr = "t0.FSALEORGID";
                }
                else if (formId.EqualsIgnoreCase("AR_receivable") || formId.EqualsIgnoreCase("IV_SALESIC") || formId.EqualsIgnoreCase("IV_SALESOC"))//应收单 销售增值税发票,销售发票
                {
                    custStr = "t0.FCUSTOMERID";
                    orgStr = "t0.FSETTLEORGID";
                }
                else if (formId.EqualsIgnoreCase("AR_RECEIVEBILL") || formId.EqualsIgnoreCase("AR_REFUNDBILL")) //收款单  收款退款单
                {
                    custStr = "t0.FCONTACTUNIT";
                    orgStr = "t0.FPAYORGID";
                }
                else if (formId.EqualsIgnoreCase("PLN_FORECAST"))//预测单
                {
                    custStr = "t0.FPENYCustId";
                    orgStr = "t0.FFOREORGID";
                }
                string filter = "(";
                if (orgList.Count > 0)
                {
                    filter += $"{orgStr} in ({string.Join(",", orgList)}) or ";
                }
                string recFilter = "";
                if (formId.EqualsIgnoreCase("AR_RECEIVEBILL") || formId.EqualsIgnoreCase("AR_REFUNDBILL"))//收款单  收款退款单
                {
                    recFilter = " and t0.FCONTACTUNITTYPE = 'BD_Customer'";
                }

                filter += $@" 
                  (exists(
                      select 1
                      from T_SAL_SCSALERCUST sg
                       inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
                       inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY'
                       inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID
                      where staff.FEmpInfoId = {userId} and {custStr}=sg.FCUSTOMERID and {orgStr}=sg.FSALEORGID){recFilter}))";

                e.FilterString = e.FilterString.IsNullOrEmptyOrWhiteSpace() ? filter : e.FilterString + " and " + filter;

            }
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            switch (e.FieldKey)
            {
                case string key when key.EqualsIgnoreCase("FPENYCustId"):
                    var userinfo = ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
                    long userId = userinfo.FEMPID;
                    string sql = $@"select 1 from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID}  and t3.FNUMBER = 'KHGLY'";
                    if (!DBServiceHelper.ExecuteScalar<bool>(this.Context, sql, false))
                    {
                        string filter = $@" 
                        exists(
                            select 1
                            from T_SAL_SCSALERCUST sg
	                            inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
	                            inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY'
	                            inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID
                            where staff.FEmpInfoId = {userId} and t0.FCUSTID=sg.FCUSTOMERID)";
                        e.ListFilterParameter.Filter = e.ListFilterParameter.Filter.IsNullOrEmptyOrWhiteSpace() ? filter : e.ListFilterParameter.Filter + " and " + filter;
                    }
                    break;
            }
        }
    }
}
