using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.SCM.ServiceHelper;
using System.ComponentModel;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("可销控制客户销售员单据插件"), HotUpdate]
    public class SCSalerCustSingleEdit : AbstractDynamicFormPlugIn
    {
        private DynamicObject[] synCustInfoData;

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            string text;
            if ((text = e.BarItemKey.ToUpperInvariant()) != null && text == "TBSYNCUSTSALER" && View.Model.DataObject != null)
            {
                synCustInfoData = new DynamicObject[1] { View.Model.DataObject };
                if (SaleServiceHelper.SynCustomerInfoWithSalesControl(base.Context, synCustInfoData) > 0)
                {
                    View.ShowMessage(ResManager.LoadKDString("同步客户资料销售员成功！", "005130030001369", SubSystemType.SCM));
                }
            }
        }

        public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
        {
            switch (e.BaseDataFieldKey.ToUpperInvariant())
            {
                case "FSALERID":
                case "FSALERGROUPID":
                    {
                        if (GetFieldFilter(e.BaseDataFieldKey, e.Row, out var filter))
                        {
                            if (string.IsNullOrEmpty(e.Filter))
                            {
                                e.Filter += filter;
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

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            switch (e.FieldKey.ToUpperInvariant())
            {
                case "FSALERID":
                case "FSALERGROUPID":
                    {
                        if (GetFieldFilter(e.FieldKey, e.Row, out var filter))
                        {
                            if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
                            {
                                e.ListFilterParameter.Filter = filter;
                                break;
                            }
                            IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
                            listFilterParameter.Filter = listFilterParameter.Filter + " AND " + filter;
                        }
                        break;
                    }
            }
        }

        private bool GetFieldFilter(string fieldKey, int row, out string filter)
        {
            filter = "";
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                return false;
            }
            switch (fieldKey.ToUpperInvariant())
            {
                case "FSALERID":
                    {
                        DynamicObject dynamicObject2 = View.Model.GetValue("FSalerDeptId") as DynamicObject;
                        DynamicObject dynamicObject3 = View.Model.GetValue("FSalerGroupId") as DynamicObject;
                        DynamicObject dynamicObject4 = View.Model.GetValue("FSALEORGID") as DynamicObject;
                        if (dynamicObject4 != null && Convert.ToInt32(dynamicObject4["Id"]) > 0)
                        {
                            filter += $" FIsUse='1' and FBizORGId={Convert.ToInt32(dynamicObject4["Id"])} ";
                        }
                        else
                        {
                            filter += $" FIsUse='1' and FBizORGId={View.Context.CurrentOrganizationInfo.ID} ";
                        }
                        if (dynamicObject2 != null && Convert.ToInt64(dynamicObject2["Id"]) > 0)
                        {
                            filter += string.Format(" And FDeptId={0} ", Convert.ToInt64(dynamicObject2["Id"]));
                        }
                        if (dynamicObject3 != null && Convert.ToInt64(dynamicObject3["Id"]) > 0)
                        {
                            filter += string.Format(" And FOperatorGroupId={0} ", Convert.ToInt64(dynamicObject3["Id"]));
                        }
                        break;
                    }
                case "FSALERGROUPID":
                    {
                        DynamicObject dynamicObject = View.Model.GetValue("FSALER") as DynamicObject;
                        DynamicObject dynamicObject4 = View.Model.GetValue("FSALEORGID") as DynamicObject;
                        if (dynamicObject4 != null && Convert.ToInt32(dynamicObject4["Id"]) > 0)
                        {
                            filter += $" FIsUse='1' and FBizORGId={Convert.ToInt32(dynamicObject4["Id"])} ";
                        }
                        else
                        {
                            filter += $" FIsUse='1' and FBizORGId={View.Context.CurrentOrganizationInfo.ID} ";
                        }
                        if (dynamicObject != null && Convert.ToInt64(dynamicObject["Id"]) > 0)
                        {
                            filter += string.Format(" And Exists (Select 1 From V_BD_SALESMANENTRY SE Where SE.FOperatorGroupID=FENTRYID AND SE.FId={0}) ", Convert.ToInt64(dynamicObject["Id"]));
                        }
                        break;
                    }
            }
            return !string.IsNullOrWhiteSpace(filter);
        }
    }
}
