using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Util;
using static System.Net.WebRequestMethods;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
    [Description("分标预览3d文件动态表单构建插件"), HotUpdate]
    public class PlanOrder3DViewBillCreatePlugIn : AbstractDynamicWebFormBuilderPlugIn
    {
        public override void CreateControl(CreateControlEventArgs e)
        {
            base.CreateControl(e);
            if (e.ControlAppearance.Key == "FPanelWebBrowse")
            {
                e.Control["xtype"] = "kdwebbrowser";
            }
        }
    }

    [Description("分标预览3d文件动态表单插件"), HotUpdate]
    public class PlanOrder3DViewBillPlugIn : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var url = "https://www.deepseek.com/";
            //拿到面板控件，设置需要展示的嵌套网页地址，参数通过url传参。
            Control webBrowse = this.View.GetControl("FPanelWebBrowse");
            webBrowse.SetCustomPropertyValue("Source", url);
            webBrowse.SetCustomPropertyValue("IsSetBrowseVisible", true);
            webBrowse.SetCustomPropertyValue("Allowfullscreen", true);
            //兼容GUI。
            if (this.View.Context.ClientType != ClientType.Silverlight
                && this.View.Context.ClientType != ClientType.Html)
            {
                webBrowse.InvokeControlMethod("StartTime", 1);
            }
            this.View.AddAction("notShowMainFormHolder", true);
        }
    }
}
