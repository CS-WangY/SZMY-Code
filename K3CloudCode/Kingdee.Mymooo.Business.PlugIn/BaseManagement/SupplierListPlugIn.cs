using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.PLN.Business.PlugIn.Bill;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("供应商过滤插件"), HotUpdate]
    public class SupplierListPlugIn : AbstractListPlugIn
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            var userinfo = ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
            long userId = userinfo.FEMPID;
            var parentFormId = this.View.ParentFormView.BillBusinessInfo.GetForm().Id;
            //供应商列表
            if (parentFormId.EqualsIgnoreCase("BOS_MainConsoleNewSutra"))
            {
                List<long> orgList = new List<long>();
                string sql = $@"select t2.FORGID from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID}  and t3.FNUMBER = 'SCM15_SYS'";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                foreach (var item in datas)
                {
                    orgList.Add(Convert.ToInt64(item["FORGID"]));
                }
                string filter = "(";
                if (orgList.Count > 0)
                {
                    filter += $"t0.FUSEORGID in ({string.Join(",", orgList)}) or ";
                }
                filter += $@" exists(
                                select 1 from T_BD_PurchaseSmall t1 
                                left join T_BD_SupplierSmall t2 on t1.FMATERIALGROUP=t2.FMATERIALGROUP
				                left join t_BD_Supplier t3 on t2.FSUPPLIERID=t3.FSUPPLIERID
				                left join t_BD_Supplier t4 on t3.FMASTERID=t4.FMASTERID
                                where t0.FSUPPLIERID=t4.FSUPPLIERID and t1.FPURCHASEID={userId}))";
                e.FilterString = e.FilterString.IsNullOrEmptyOrWhiteSpace() ? filter : e.FilterString + " and " + filter;
            }
            else
            {
                //其他弹窗选供应商
                string sql = $@"select 1 from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID} and t2.FORGID={this.Context.CurrentOrganizationInfo.ID} and t3.FNUMBER='SCM15_SYS' ";
                if (!DBServiceHelper.ExecuteScalar<bool>(this.Context, sql, false))
                {
                    string filter = $@" exists(
                                select 1 from T_BD_PurchaseSmall t1 
                                left join T_BD_SupplierSmall t2 on t1.FMATERIALGROUP=t2.FMATERIALGROUP
				                left join t_BD_Supplier t3 on t2.FSUPPLIERID=t3.FSUPPLIERID
				                left join t_BD_Supplier t4 on t3.FMASTERID=t4.FMASTERID
                                where t0.FSUPPLIERID=t4.FSUPPLIERID and t4.FUseOrgId={this.Context.CurrentOrganizationInfo.ID} and t1.FPURCHASEID={userId})";
                    e.FilterString = e.FilterString.IsNullOrEmptyOrWhiteSpace() ? filter : e.FilterString + " and " + filter;
                }
            }

        }

        public override void AfterCreateSqlBuilderParameter(SqlBuilderParameterArgs e)
        {
            base.AfterCreateSqlBuilderParameter(e);

            if (this.View.ParentFormView != null)
            {
                if (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SAL_SaleOrder")
                {
                    e.sqlBuilderParameter.IsolationOrgId = 1;
                }
            }

        }
    }
}
