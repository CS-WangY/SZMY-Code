using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Mymooo.Business.PlugIn.MrpModelManagement
{
    [Description("运算界面互斥判断"), HotUpdate]
    public class MRPCALCWIZARDBuilderPlugIn : AbstractDynamicFormPlugIn
    {
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
        }
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            string sSql = $"SELECT * FROM dbo.T_MRP_MRPTABLEMANAGE WHERE FISUSED=1";
            var mrpused = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).Count();
            if (mrpused > 0)
            {
                this.View.GetControl("FNext").Enabled = false;
                this.View.GetControl("FBtnDireCalc").Enabled = false;
                throw new Exception("已有计划运算中,不允许手工继续运算！");
            }
        }
    }
}
