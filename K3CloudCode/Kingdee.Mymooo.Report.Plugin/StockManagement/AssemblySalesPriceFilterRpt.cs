using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.StockManagement
{
    [Description("组装销售价报表过滤条件"), HotUpdate]
    public class AssemblySalesPriceFilterRpt : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("F_Filter_StartDate", Convert.ToDateTime(System.DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01")).ToShortDateString());
            this.Model.SetValue("F_Filter_EndDate", Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-01")).AddDays(-1).ToShortDateString());
            this.View.UpdateView("F_Filter_StartDate");
            this.View.UpdateView("F_Filter_EndDate");

        }
    }

}
