using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    /// <summary>
    /// 发货通知单保存验证可用库存
    /// </summary>
    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var data in dataEntities)
            {
                //stockValidator.ExecuteStockValidator(data, validateContext, ctx);
                stockValidator.ExecuteStockValidatorN(data, validateContext, ctx);
                stockValidator.SalDeliveryNoticeValidator(data, validateContext, ctx);
            }
        }
    }
}
