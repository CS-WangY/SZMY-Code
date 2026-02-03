using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;

namespace Kingdee.Mymooo.ServicePlugInBill
{
    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            foreach (var data in dataEntities)
            {
                validateContext.AddError(data, new ValidationErrorInfo(
                                  string.Empty,
                                  data["Id"].ToString(),
                                  data.DataEntityIndex,
                                  data.RowIndex,
                                  data["Id"].ToString(),
                                  string.Format("编码:{0}校验不通过,不允许删除!", data["BillNo"]),
                                  "测试",
                                  ErrorLevel.FatalError));
            }
        }
    }
}