using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.StkAssembledApp
{
    [Description("组装拆卸单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FStockOrgId");
            e.FieldKeys.Add("FAffairType");//拆卸：Dassembly 组装：Assembly
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FStockID");
            e.FieldKeys.Add("FMaterialIDSETY");
            e.FieldKeys.Add("FQtySETY");
            e.FieldKeys.Add("FStockIDSETY");

        }
        //验证
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            AuditValidator isPoValidator = new AuditValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }

    }
}
