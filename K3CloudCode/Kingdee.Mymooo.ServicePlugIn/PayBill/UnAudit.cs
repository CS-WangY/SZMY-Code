using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PayBill
{
    [Description("付款单反审核插件"), HotUpdate]
    public class UnAudit : AbstractOperationServicePlugIn
    {
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            if (this.Option.TryGetVariableValue<bool>("RemoveValidators", out bool validator) && validator)
            {
                e.Validators.Clear();
            }
        }
    }
}
