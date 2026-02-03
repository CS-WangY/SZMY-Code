using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.PRD_MO
{
	[Description("生产订单反审核"), HotUpdate]
	public class UnAudit : AbstractOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
		}

		public override void OnAddValidators(AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);
			UnAuditValidator unAuditValidator = new UnAuditValidator();
			unAuditValidator.AlwaysValidate = true;
			unAuditValidator.EntityKey = "FTreeEntity";
			e.Validators.Add(unAuditValidator);

		}
	}
}
