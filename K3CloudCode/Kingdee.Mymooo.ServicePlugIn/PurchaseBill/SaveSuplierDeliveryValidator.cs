using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
    public class SaveSuplierDeliveryValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var headEntity in dataEntities)
            {
                var remarks = Convert.ToString(headEntity["FRemarks"]);
                var trackingNumber = Convert.ToString(headEntity["FTrackingNumber"]);
                var dynamicTrackingID = headEntity["FTrackingID"];
                if (string.IsNullOrWhiteSpace(remarks) && string.IsNullOrWhiteSpace(trackingNumber))
                {
                    validateContext.AddError(headEntity, new ValidationErrorInfo(
                       string.Empty,
                       headEntity["Id"].ToString(),
                       headEntity.DataEntityIndex,
                       headEntity.RowIndex,
                       headEntity["Id"].ToString(),
                       "回复说明或者快递单号不能为空。",
                       $"",
                       ErrorLevel.FatalError));
                }
                else if (!string.IsNullOrWhiteSpace(trackingNumber) && dynamicTrackingID == null)
                {
                    validateContext.AddError(headEntity, new ValidationErrorInfo(
                       string.Empty,
                       headEntity["Id"].ToString(),
                       headEntity.DataEntityIndex,
                       headEntity.RowIndex,
                       headEntity["Id"].ToString(),
                       "存在快递单号，请选择对应的快递公司。",
                       $"",
                       ErrorLevel.FatalError));
                }
                else if (dynamicTrackingID != null)
                {
                    var trackingCode = Convert.ToString(((DynamicObject)dynamicTrackingID)["FNumber"].ToString());
                    if (string.IsNullOrWhiteSpace(trackingNumber) && !trackingCode.EqualsIgnoreCase("huolala") && !trackingCode.EqualsIgnoreCase("qita"))
                    {
                        validateContext.AddError(headEntity, new ValidationErrorInfo(
                          string.Empty,
                          headEntity["Id"].ToString(),
                          headEntity.DataEntityIndex,
                          headEntity.RowIndex,
                          headEntity["Id"].ToString(),
                          "快递公司不为空，请填写准确的快递单号。",
                          $"",
                          ErrorLevel.FatalError));
                    }



                }
            }
        }
    }
}
