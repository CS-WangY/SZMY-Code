using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    /// <summary>
    /// 发货通知单提交验证可用库存
    /// </summary>
    public class SubmitValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var data in dataEntities)
            {
                var dynamicObject = data.DataEntity as DynamicObject;
                if ((Convert.ToBoolean(dynamicObject["FIsShipInspect"])) && string.IsNullOrWhiteSpace(Convert.ToString(dynamicObject["FPENYNOTE"])))
                {
                    validateContext.AddError(data, new ValidationErrorInfo(
                                    string.Empty,
                                    data["Id"].ToString(),
                                    data.DataEntityIndex,
                                    data.RowIndex,
                                    data["Id"].ToString(),
                                    string.Format("单据编号[{0}]出货检验勾选，客户备注不能为空。", data["BillNo"]),
                                    $"提交[{data["BillNo"]}]",
                                    ErrorLevel.FatalError));
                }
                //库存验证
                stockValidator.ExecuteStockValidatorN(data, validateContext, ctx);
            }
        }
    }
    public class SubmitMultipleSelValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (dataEntities.Count() > 1)
            {
                throw new Exception("不允许选择多张发货通知单同时提交,请重新选择!");
            }
        }
    }
}
