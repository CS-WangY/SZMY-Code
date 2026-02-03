using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.STK.StkUnAudit
{
    [Description("仓库单据反审核判断云仓储是否可以反审核"), HotUpdate]
	public class StkBillUnAudit : AbstractOperationServicePlugIn
	{
		public override void OnAddValidators(AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);

			UnAuditValidator isPoValidator = new UnAuditValidator();
			isPoValidator.AlwaysValidate = true;
			isPoValidator.EntityKey = "FBillHead";
			e.Validators.Add(isPoValidator);
		}
	}
}
