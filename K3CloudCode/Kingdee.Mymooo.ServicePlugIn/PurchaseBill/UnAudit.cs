using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单反审核验证插件")]
	[HotUpdate]
	public  class UnAudit : AbstractOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);

			e.FieldKeys.Add("FSupplierId");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("FElectricSyncStatus"); 
		}

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
