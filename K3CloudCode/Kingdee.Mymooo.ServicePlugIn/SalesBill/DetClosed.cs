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
    [Description("销售单明细关闭验证"), HotUpdate]
    public class DetClosed : AbstractOperationServicePlugIn
    {
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            if (this.FormOperation.OperationId == 38)
            {
                DetClosedValidator isPoValidator = new DetClosedValidator();
                isPoValidator.AlwaysValidate = true;
                isPoValidator.EntityKey = "FSaleOrderEntry";
                e.Validators.Add(isPoValidator);
            }
        }
    }
}
