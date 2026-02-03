using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
    [Description("采购订单保存验证插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            //e.FieldKeys.Add("FBillTypeID");
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
