using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.Mymooo.Report.Plugin.ProductionManagement
{
    [Description("生产入库产值统计表过滤条件"), HotUpdate]
    public class PrdInStockProdValueFilterRpt : AbstractDynamicFormPlugIn
    {

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("F_Filter_StartDate", System.DateTime.Now.ToString("yyyy-MM-01"));
            this.Model.SetValue("F_Filter_EndDate", System.DateTime.Now.ToShortDateString());
            this.View.UpdateView("F_Filter_StartDate");
            this.View.UpdateView("F_Filter_EndDate");
        }

        public override void BeforeBindData(EventArgs e)
        {
            base.BeforeBindData(e);
            List<long> lstOrg = new List<long>();
            if (this.View.ParentFormView != null)
            {
                lstOrg = this.GetPermissionOrg(this.View.ParentFormView.BillBusinessInfo.GetForm().Id);
            }
            //组织
            List<EnumItem> organization = this.GetOrganization(lstOrg);
            ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("F_Filter_MulBaseOrgList", 0);
            fieldEditor.SetComboItems(organization);
            object value = this.Model.GetValue("F_Filter_MulBaseOrgList");
            if (value.IsNullOrEmpty())
            {
                if (organization.Count((EnumItem p) => Convert.ToInt64(p.Value) == base.Context.CurrentOrganizationInfo.ID) > 0)
                {
                    this.View.Model.SetValue("F_Filter_MulBaseOrgList", base.Context.CurrentOrganizationInfo.ID);
                }
            }
            this.View.UpdateView("F_Filter_MulBaseOrgList");
            this.View.Model.SetValue("F_Filter_IsAllOrgID", string.Join<long>(",", lstOrg));
            this.View.UpdateView("F_Filter_IsAllOrgID");

            //生产车间
            List<EnumItem> workShop = this.GetWorkShop(lstOrg);
            ComboFieldEditor fieldWsEditor = this.View.GetFieldEditor<ComboFieldEditor>("F_Filter_WorkShopID", 0);
            fieldWsEditor.SetComboItems(workShop);
            this.View.UpdateView("F_Filter_WorkShopID");

        }

        /// <summary>
        /// 根据内码获取组织信息
        /// </summary>
        /// <param name="lstOrg"></param>
        /// <returns></returns>
        protected virtual List<EnumItem> GetOrganization(List<long> lstOrg)
        {
            List<EnumItem> list = new List<EnumItem>();
            List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
            list2.Add(new SelectorItemInfo("FORGID"));
            list2.Add(new SelectorItemInfo("FNUMBER"));
            list2.Add(new SelectorItemInfo("FNAME"));

            string text = string.Format("{0} IN ({1})", "FORGID", string.Join<long>(",", lstOrg));
            QueryBuilderParemeter para = new QueryBuilderParemeter
            {
                FormId = "ORG_Organizations",
                SelectItems = list2,
                FilterClauseWihtKey = text
            };
            DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, para, null);
            foreach (DynamicObject current in dynamicObjectCollection)
            {
                list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
                {
                    EnumId = current["FORGID"].ToString(),
                    Value = current["FORGID"].ToString(),
                    Caption = new LocaleValue((current["FName"].ToString().IndexOf("-") >= 0 ? current["FName"].ToString().Substring(0, current["FName"].ToString().IndexOf("-")) : current["FName"].ToString()), base.Context.UserLocale.LCID)
                });
            }
            if (list.Count<EnumItem>() == 0)
            {
                this.View.ShowMessage(ResManager.LoadKDString("没有合适类型的业务组织！", "016007000000876", SubSystemType.CY, new object[0]), MessageBoxOptions.OK, "", MessageBoxType.Notice);
            }
            return list;
        }

        /// <summary>
        /// 根据内码获取生产部门
        /// </summary>
        /// <param name="lstOrg"></param>
        /// <returns></returns>
        protected virtual List<EnumItem> GetWorkShop(List<long> lstOrg)
        {
            if (lstOrg.Count == 0)
            {
                lstOrg.Add(0);
            }
            List<EnumItem> list = new List<EnumItem>();
            list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
            {
                EnumId = "",
                Value = "",
                Caption = new LocaleValue("", base.Context.UserLocale.LCID)
            });
            var sql = $@"/*dialect*/SELECT distinct t0.FMASTERID, t0_L.FNAME fname,  t0_L.FFULLNAME ffullname
                        FROM T_BD_DEPARTMENT t0 
                        LEFT OUTER JOIN T_BD_DEPARTMENT_L t0_L ON (t0.FDEPTID = t0_L.FDEPTID AND t0_L.FLocaleId = 2052)
                        WHERE (t0.FISSTOCK = '1') and  t0.FUSEORGID IN ({string.Join<long>(",", lstOrg)}) order by t0_L.FFULLNAME ";
            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
            foreach (var item in datas)
            {
                list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
                {
                    EnumId = item["FMASTERID"].ToString(),
                    Value = item["FMASTERID"].ToString(),
                    Caption = new LocaleValue(item["ffullname"].ToString(), base.Context.UserLocale.LCID)
                });
            }
            return list;
        }

        /// <summary>
        /// 获取有权限的组织
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        protected virtual List<long> GetPermissionOrg(string formId)
        {
            BusinessObject bizObject = new BusinessObject
            {
                Id = formId,
                PermissionControl = this.View.ParentFormView.BillBusinessInfo.GetForm().SupportPermissionControl,
                SubSystemId = this.View.ParentFormView.Model.SubSytemId
            };
            return PermissionServiceHelper.GetPermissionOrg(base.Context, bizObject, "6e44119a58cb4a8e86f6c385e14a17ad");
        }
    }
}
