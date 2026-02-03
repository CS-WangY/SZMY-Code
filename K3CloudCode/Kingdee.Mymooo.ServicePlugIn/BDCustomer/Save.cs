using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.BDCustomer
{
    /// <summary>
    /// 客户保存插件
    /// </summary>
    [Description("客户保存插件")]
    public class Save : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            var aduitValidator = new SaveValidator();
            aduitValidator.AlwaysValidate = true;
            aduitValidator.EntityKey = "FBillHead";
            e.Validators.Add(aduitValidator);
        }
    }
}
