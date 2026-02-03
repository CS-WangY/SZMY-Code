using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单关闭提交备注插件"), HotUpdate]
    public class SalesClosedApprovalBusiness : AbstractDynamicFormPlugIn
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            switch (e.Key)
            {
                case "FPENYBUTTONOK":
                    this.View.ReturnToParentWindow(this.View.Model.GetValue("FPENYRemarks"));
                    this.View.Close();
                    break;
                case "FPENYBUTTONCANCEL":
                    this.View.Close();
                    break;
            }
        }
    }
}
