using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.ServicePlugIn.SaleXBill;

namespace Kingdee.Mymooo.ServicePlugIn.PLN_ForecastBill
{
    [Description("预测订单保存-校验插件"), HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            ForecastSaveValidator forecastSaveValidator = new ForecastSaveValidator();
            ((AbstractValidator)forecastSaveValidator).AlwaysValidate = (true);
            ((AbstractValidator)forecastSaveValidator).EntityKey = "FBillHead";
            e.Validators.Add((AbstractValidator)(object)forecastSaveValidator);
        }
    }
}
