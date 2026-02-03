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
using Kingdee.BOS.Util;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;

namespace Kingdee.Mymooo.ServicePlugIn.PrdPickMtrl
{
    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var headEntity in dataEntities)
            {
                var dynamicObject = headEntity.DataEntity as DynamicObject;
                var entrys = headEntity["Entity"] as DynamicObjectCollection;
                long orgId = Convert.ToInt64(dynamicObject["PrdOrgId_Id"].ToString());
                List<string> synList = new List<string>();
                List<string> noSynList = new List<string>();
                if (orgId == 7401780)
                {
                    foreach (var entry in entrys)
                    {
                        //同步
                        if (bool.Parse(((DynamicObject)entry["StockID"])["FSyncToWarehouse"].ToString()))
                        {
                            synList.Add(((DynamicObject)entry["StockID"])["Name"].ToString());
                        }
                        else
                        {
                            noSynList.Add(((DynamicObject)entry["StockID"])["Name"].ToString());
                        }
                    }
                    if (synList.Count() > 0 && noSynList.Count() > 0)
                    {
                        validateContext.AddError(headEntity, new ValidationErrorInfo(
                          string.Empty,
                          headEntity["Id"].ToString(),
                          headEntity.DataEntityIndex,
                          headEntity.RowIndex,
                          headEntity["Id"].ToString(),
                          $"仓库传云仓储和不传云仓储，需要分开，传云仓储的仓库[{string.Join("、", synList.Distinct())}]，不传云仓储的仓库[{string.Join("、", noSynList.Distinct())}]",
                          $"生产领料订单保存[{headEntity["BillNo"]}]",
                          ErrorLevel.FatalError));
                    }
                }
            }
        }
    }
}
