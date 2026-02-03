using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.ServicePlugIn.AR_Receivable
{
    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var headEntity in dataEntities)
            {
                foreach (var entitem in headEntity["AP_PAYABLEENTRY"] as DynamicObjectCollection)
                {
                    if (Convert.ToDecimal(entitem["F_PENY_DIFFALLAMOUNT"]) != 0 && string.IsNullOrWhiteSpace(Convert.ToString(entitem["Comment"])))
                    {
                        validateContext.AddError(headEntity, new ValidationErrorInfo(
                                string.Empty,
                                headEntity["Id"].ToString(),
                                headEntity.DataEntityIndex,
                                headEntity.RowIndex,
                                headEntity["Id"].ToString(),
                                string.Format("第{0}行，价税差额不等于0，需要填写备注！", Convert.ToString(entitem["seq"])),
                                $"保存[{headEntity["BillNo"]}]",
                                ErrorLevel.FatalError));
                    }
                }
            }
        }
    }
}
