using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单跟踪查询报表过滤条件"), HotUpdate]
    public class SalesOrderTrackQueryFilterRpt : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("FStartDate", System.DateTime.Now.AddMonths(-1).ToShortDateString());
            this.Model.SetValue("FEndDate", System.DateTime.Now.ToShortDateString());
            this.Model.SetValue("FSoStatus", "A");
            this.View.UpdateView("FStartDate");
            this.View.UpdateView("FEndDate");
            this.View.UpdateView("FSoStatus");
        }
    }
}
