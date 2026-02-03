using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.PLN.Business.PlugIn.DynamicForm;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.PLN;
using Kingdee.K3.SCM.Core;
using Kingdee.Mymooo.Core.Utils;

namespace Kingdee.Mymooo.Business.PlugIn.ReservedManagement
{
    [Description("预留综合查询扩展"), HotUpdate]
    public class ReserveLinkQueryEx : AbstractDynamicFormPlugIn
    {
        public override void OnLoad(EventArgs e)
        {
            //showOption = MFGBillUtil.GetParentFormSession<object>(View, "FormInputParam") as ReserveQueryOption;

        }
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var formid = this.View.ParentFormView?.ParentFormView?.GetFormId();
            if (formid.EqualsIgnoreCase("PLN_MtrlSupplyDemandExpRpt"))
            {
                var materialid = View.ParentFormView.OpenParameter.GetCustomParameter("FMaterialID");
                //DynamicObject newObj = new DynamicObject(filterParam.CustomFilter.DynamicObjectType);
                //newObj.SetDynamicObjectItemValue("MaterialFrom", billView.Model.DataObject);
                //newObj.SetDynamicObjectItemValue("MaterialTo", billView.Model.DataObject);
                //filterParam.CustomFilter = newObj;
                //ShowFromOtherForm();
                View.Model.SetItemValueByID("FMaterialFrom", materialid, 0);
                View.Model.SetItemValueByID("FMaterialTo", materialid, 0);
                View.UpdateView("FMaterialFrom");
                View.UpdateView("FMaterialTo");
            }
        }
    }

    [Description("预留综合查询扩展2"), HotUpdate]
    public class ReserveLinkQueryEditEx : ReserveLinkQuery
    {
        public override void OnInitialize(InitializeEventArgs e)
        {
            //var formid = this.View.ParentFormView?.GetFormId();
            //if (formid.EqualsIgnoreCase("PLN_MtrlSupplyDemandExpRpt"))
            //{
            //    ReserveQueryOption inputParam = new ReserveQueryOption();
            //    List<ReserveQueryBillInfo> billInfos = new List<ReserveQueryBillInfo>();
            //    inputParam.BillInfos = billInfos;
            //    List<long> longs = new List<long>();
            //    longs.Add(Convert.ToInt64(View.OpenParameter.GetCustomParameter("FMaterialID")));
            //    inputParam.MaterialIds = longs;
            //    View.ParentFormView.Session["FormInputParam"] = inputParam;
            //}
            //base.OnInitialize(e);
        }
    }
}
