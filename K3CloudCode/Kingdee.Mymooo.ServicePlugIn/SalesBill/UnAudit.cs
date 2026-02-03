using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售单反审核验证采购单是否已采购插件"), HotUpdate]
    public class UnAudit : AbstractOperationServicePlugIn
	{
		public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            UnAuditValidator isPoValidator = new UnAuditValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);

			if (this.Option.TryGetVariableValue<bool>("RemoveValidators", out bool validator) && validator)
			{
				e.Validators.Clear();
			}
		}
    }
}
