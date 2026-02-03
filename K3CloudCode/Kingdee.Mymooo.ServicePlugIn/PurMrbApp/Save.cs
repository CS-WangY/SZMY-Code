using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PurMrbApp
{
	[Description("退料申请保存验证插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class Save : AbstractOperationServicePlugIn
	{
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FSRCBILLTYPEID");
			e.FieldKeys.Add("FSRCBILLNO");
			e.FieldKeys.Add("FSRCSEQ");

		}
		public override void OnAddValidators(AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);
			SaveValidator isPoValidator = new SaveValidator();
			isPoValidator.AlwaysValidate = true;
			isPoValidator.EntityKey = "FBillHead";
			e.Validators.Add(isPoValidator);
		}
	}
}
