using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售出库成本表过滤条件表单插件"), HotUpdate]
    public class SAL_OUTSTOCK_IncomeFilter : AbstractCommonFilterPlugIn
    {
        private List<long> lstSalOrg = new List<long>();

        private List<long> filterOrgId = new List<long>();

        public override void AfterBindData(EventArgs e)
        {
            this.View.StyleManager.SetEnabled("FSaleOrgList", (string)null, this.Context.IsMultiOrg);
            //((AbstractDynamicFormPlugIn)this).get_View().get_StyleManager().SetEnabled("FAffiliation", (string)null, ((AbstractDynamicFormPlugIn)this).get_Context().get_IsMultiOrg());
            //((AbstractDynamicFormPlugIn)this).get_View().GetControl("FISINCLUDEIOS").set_Visible(((AbstractDynamicFormPlugIn)this).get_Context().get_IsMultiOrg());
            SetCostTypeValue();
            SetDefaultDate();
        }

        public override void OnLoad(EventArgs e)
        {
            SetCostTypeValue();
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            string text = string.Empty;
            string text2 = Convert.ToString(this.View.Model.GetValue("FSaleOrgList"));
            if (string.IsNullOrWhiteSpace(text2))
            {
                text2 = "0";
            }
            switch (e.FieldKey.ToUpperInvariant())
            {
                case "FSALEDEPARTFROM":
                case "FSALEDEPARTTO":
                case "FMATERIALIDFROM":
                case "FMATERIALIDTO":
                case "FCUSTOMERFROM":
                case "FCUSTOMERTO":
                case "FSALERIDFROM":
                case "FSALERIDTO":
                    {
                        DynamicFormShowParameter dynamicFormShowParameter = e.DynamicFormShowParameter;
                        ListShowParameter val = (ListShowParameter)(object)((dynamicFormShowParameter is ListShowParameter) ? dynamicFormShowParameter : null);
                        val.MutilListUseOrgId = text2;
                        break;
                    }
                case "FSALEGROUPIDFROM":
                case "FSALEGROUPIDTO":
                    text = " FOperatorGroupType='XSZ' AND FBizOrgId in  (" + text2 + ")";
                    break;
                case "FAFFILIATION":
                    {
                        string[] array = text2.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> list = new List<string>();
                        list.Add("0");
                        if (array != null && array.Length != 0)
                        {
                            string[] array2 = array;
                            foreach (string text3 in array2)
                            {
                                if (CheckAffPermission(Convert.ToInt64(text3)))
                                {
                                    list.Add(text3);
                                }
                            }
                        }
                        text = string.Format(" FTYPE = 101 AND ( FROOTORGID IN ({0}) OR FAFFILIATIONID IN ( SELECT DISTINCT FAFFILIATIONID FROM T_ORG_AFFILIATIONENTRY WHERE FORGID IN ({0}) )) ", string.Join(",", list.ToArray()));
                        break;
                    }
            }
            if (!string.IsNullOrEmpty(text))
            {
                if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
                {
                    e.ListFilterParameter.Filter = text;
                    return;
                }
                IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
                listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
            }
        }

        public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
        {
            switch (e.BaseDataFieldKey.ToUpperInvariant())
            {
                case "FSALEDEPARTFROM":
                case "FSALEDEPARTTO":
                case "FMATERIALIDFROM":
                case "FMATERIALIDTO":
                case "FCUSTOMERFROM":
                case "FCUSTOMERTO":
                case "FSALEGROUPIDFROM":
                case "FSALEGROUPIDTO":
                case "FSALERIDFROM":
                case "FSALERIDTO":
                case "FAFFILIATION":
                    {
                        if (GetFieldFilter(e.BaseDataFieldKey, out var filter))
                        {
                            if (string.IsNullOrEmpty(e.Filter))
                            {
                                e.Filter = e.Filter + filter;
                            }
                            else
                            {
                                e.Filter = e.Filter + " AND " + filter;
                            }
                        }
                        break;
                    }
            }
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            SetCostTypeValue();
            if (e.Key == "FBTNOK")
            {
                CheckRight(e);
            }
        }

        public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
        {
            string empty = string.Empty;
            string text;
            if ((text = e.Key.ToUpperInvariant()) != null && text == "FSALORGNAME")
            {
                empty = string.Format("FORGID in ({0})", string.Join(",", filterOrgId));
                if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
                {
                    e.ListFilterParameter.Filter = empty;
                    return;
                }
                IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
                listFilterParameter.Filter = listFilterParameter.Filter + " AND " + empty;
            }
        }

        private bool CheckAffPermission(long orgId)
        {
            Context context = this.Context;
            BusinessObject val = new BusinessObject();
            val.Id = "Sal_ProfitAnalyse";
            PermissionAuthResult val2 = PermissionServiceHelper.FuncPermissionAuth(context, val, "1aef314349a1454c9519f793490bd9e1");
            if (!val2.Passed)
            {
                return false;
            }
            IEnumerable<Organization> enumerable = from p in val2.OrgActions
                                                   where p.Id == orgId
                                                   select p;
            if (ListUtils.IsEmpty(enumerable))
            {
                return false;
            }
            return true;
        }

        private bool GetFieldFilter(string fieldKey, out string filter)
        {
            filter = "";
            string text = Convert.ToString(this.View.Model.GetValue("FSaleOrgList"));
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                return false;
            }
            switch (fieldKey.ToUpperInvariant())
            {
                case "FSALEDEPARTFROM":
                case "FSALEDEPARTTO":
                case "FMATERIALIDFROM":
                case "FMATERIALIDTO":
                case "FCUSTOMERFROM":
                case "FCUSTOMERTO":
                    filter = " FUseOrgId  in (" + text + ")";
                    break;
                case "FSALERIDFROM":
                case "FSALERIDTO":
                    filter = " FBizOrgId  in (" + text + ")";
                    break;
                case "FSALEGROUPIDFROM":
                case "FSALEGROUPIDTO":
                    filter = " FOperatorGroupType='XSZ' AND FBizOrgId in  (" + text + ")";
                    break;
                case "FAFFILIATION":
                    //filter = string.Format(" FTYPE = 101 AND ( FROOTORGID = {0} OR FAFFILIATIONID IN ( SELECT DISTINCT FAFFILIATIONID FROM T_ORG_AFFILIATIONENTRY WHERE FORGID = {0} )) ", text);
                    break;
            }
            return !string.IsNullOrWhiteSpace(filter);
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            switch (((AbstractElement)e.Field).Key.ToUpperInvariant())
            {
                case "FSALEORGLIST":
                    {
                        string text = Convert.ToString(this.View.Model.GetValue("FSaleOrgList"));
                        object value2 = this.View.Model.GetValue("FAffiliation");
                        DynamicObject val2 = (DynamicObject)((value2 is DynamicObject) ? value2 : null);
                        long num = ((val2 == null) ? 0 : Convert.ToInt64(val2["Id"]));
                        if (text.Length > 0 && text.IndexOf(',') < 0)
                        {
                            long num2 = Convert.ToInt64(text);
                            List<long> list = new List<long>();
                            if (num > 0)
                            {
                                list = OrganizationServiceHelper.GetOrgsInAffiliation(this.Context, num2, num);
                            }
                            if (!list.Contains(num2))
                            {
                                //this.View.Model.SetValue("FAffiliation", (object)null);
                            }
                            if (this.Context.IsMultiOrg)
                            {
                                //((AbstractDynamicFormPlugIn)this).View.StyleManager.SetEnabled("FAffiliation", "", true);
                            }
                        }
                        break;
                    }
                case "FDATEFROM":
                case "FDATETO":
                    {
                        //Field field = ((AbstractDynamicFormPlugIn)this).View.Model.BusinessInfo
                        //    .GetField("FAffiliation");
                        //object value = ((AbstractDynamicFormPlugIn)this).View.Model.GetValue("FAffiliation");
                        //DynamicObject val = (DynamicObject)((value is DynamicObject) ? value : null);
                        //DateTime t = Convert.ToDateTime(((BaseDataField)field).GetRefPropertyValue(val, "FStartDate"));
                        //DateTime t2 = Convert.ToDateTime(((BaseDataField)field).GetRefPropertyValue(val, "FEndDate"));
                        //DateTime t3 = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue) ? DateTime.MinValue : Convert.ToDateTime(e.NewValue));
                        //if (DateTime.Compare(DateTime.MinValue, t) != 0 && DateTime.Compare(DateTime.MinValue, t2) != 0 && val != null && (DateTime.Compare(t3, t) < 0 || DateTime.Compare(t3, t2) > 0))
                        //{
                        //    ((AbstractDynamicFormPlugIn)this).View.ShowMessage(ResManager.LoadKDString("设置的日期范围超出了隶属方案的有效期范围!", "004104030002455", (SubSystemType)5, new object[0]), (MessageBoxType)0);
                        //}
                        break;
                    }
            }
        }

        public override void TreeNodeClick(TreeNodeArgs e)
        {
            InitSalOrgId();
            SetCostTypeValue();
        }

        private void SetCostTypeValue()
        {
            object value = this.View.Model.GetValue("FCostType");
            if (value == null || string.IsNullOrWhiteSpace(Convert.ToString(value)))
            {
                this.View.Model.SetValue("FCostType", (object)"FACTCOST");
            }
        }

        private void SetDefaultDate()
        {
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            FilterParameter filterParameter = ((ICommonFilterModelService)this.Model).GetFilterParameter();
            DateTime now = DateTime.Now;
            string text = now.Year.ToString();
            string text2 = now.Month.ToString();
            string value = text + "-" + text2 + "-1 00:00:00";
            DateTime dateTime = Convert.ToDateTime(value);
            DateTime dateTime2 = dateTime.AddMonths(1).AddDays(-1.0);
            if (filterParameter.SchemeEntity.IsDefault)
            {
                this.View.Model.SetValue("FDateFrom", (object)dateTime);
                this.View.Model.SetValue("FDateTo", (object)dateTime2);
            }
        }

        private void InitSalOrgId()
        {
            if (this.View.ParentFormView != null)
            {
                lstSalOrg = GetPermissionOrg(this.View.ParentFormView.BillBusinessInfo
                    .GetForm().Id);
            }
            List<EnumItem> organization = GetOrganization(this.View.Context);
            ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FSaleOrgList", 0);
            fieldEditor.SetComboItems(organization);
            object value = this.Model.GetValue("FSaleOrgList");
            if (ObjectUtils.IsNullOrEmpty(value) && this.Context.CurrentOrganizationInfo.FunctionIds
                .Contains(101L))
            {
                this.View.Model.SetValue("FSaleOrgList", (object)this.Context.CurrentOrganizationInfo.ID);
            }
        }

        private List<long> GetPermissionOrg(string formId)
        {
            BusinessObject val = new BusinessObject();
            val.Id = formId;
            val.PermissionControl = (this.View.ParentFormView.BillBusinessInfo
                .GetForm()
                .SupportPermissionControl);
            val.SubSystemId = (this.View.ParentFormView.Model
                .SubSytemId);
            BusinessObject val2 = val;
            return PermissionServiceHelper.GetPermissionOrg(this.Context, val2, "6e44119a58cb4a8e86f6c385e14a17ad");
        }

        protected List<EnumItem> GetOrganization(Context ctx)
        {
            List<EnumItem> list = new List<EnumItem>();
            List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
            list2.Add(new SelectorItemInfo("FORGID"));
            list2.Add(new SelectorItemInfo("FNUMBER"));
            list2.Add(new SelectorItemInfo("FNAME"));
            string inFilter = GetInFilter("FORGID", lstSalOrg);
            inFilter += $" AND FORGFUNCTIONS LIKE '%{101L.ToString()}%' ";
            QueryBuilderParemeter val = new QueryBuilderParemeter();
            val.FormId = ("ORG_Organizations");
            val.SelectItems = (list2);
            val.FilterClauseWihtKey = (inFilter);
            val.OrderByClauseWihtKey = ("FNUMBER");
            QueryBuilderParemeter val2 = val;
            DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(this.Context, val2, null);
            foreach (DynamicObject item in (Collection<DynamicObject>)(object)dynamicObjectCollection)
            {
                EnumItem val3 = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
                val3.EnumId = (item["FORGID"].ToString());
                val3.Value = (item["FORGID"].ToString());
                val3.Caption = (new LocaleValue(Convert.ToString(item["FName"]), this.Context.UserLocale.LCID));
                filterOrgId.Add(Convert.ToInt64(val3.EnumId));
                list.Add(val3);
            }
            return list;
        }

        private string GetInFilter(string key, List<long> valList)
        {
            if (valList == null || ListUtils.IsEmpty<long>(valList))
            {
                return $"{key} = -1 ";
            }
            return string.Format("{0} in ({1})", key, string.Join(",", valList));
        }

        private void CheckRight(ButtonClickEventArgs e)
        {
            DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(this.Context);
            StringBuilder stringBuilder = new StringBuilder();
            DateTime dateTime = systemDateTime;
            dateTime = Convert.ToDateTime(this.View.Model.GetValue("FDateFrom"));
            DateTime dateTime2 = systemDateTime;
            dateTime2 = Convert.ToDateTime(this.View.Model.GetValue("FDateTo"));
            if (DateTime.Compare(dateTime, dateTime2) > 0)
            {
                stringBuilder.AppendLine(ResManager.LoadKDString("单据日期:开始日期不能大于目标日期!（日期字段为空默认是最小日期）", "004104030035372", (SubSystemType)5, new object[0]));
            }
            if (stringBuilder.Length > 0)
            {
                e.Cancel = true;
                this.View.ShowErrMessage(stringBuilder.ToString(), ResManager.LoadKDString("过滤条件格式错误！", "004104030002452", (SubSystemType)5, new object[0]), (MessageBoxType)1);
            }
        }
    }

}
