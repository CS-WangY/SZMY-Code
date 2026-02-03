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
    [Description("生产领料成本报表过滤条件"), HotUpdate]
    public class PrdPickingCostFilterRpt : AbstractDynamicFormPlugIn
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
            this.View.Model.SetValue("F_Filter_MulBaseOrgList", base.Context.CurrentOrganizationInfo.ID);
            this.View.UpdateView("F_Filter_MulBaseOrgList");

        }
    }
}
