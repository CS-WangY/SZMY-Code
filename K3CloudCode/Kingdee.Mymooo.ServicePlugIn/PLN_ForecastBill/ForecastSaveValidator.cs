using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn.SaleOrder;
using Kingdee.K3.SCM.Contracts;

namespace Kingdee.Mymooo.ServicePlugIn.PLN_ForecastBill
{
    public class ForecastSaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (ObjectUtils.IsNullOrEmpty((object)dataEntities) || dataEntities.Length == 0)
            {
                return;
            }
            for (int i = 0; i < dataEntities.Length; i++)
            {
                ExtendedDataEntity item = dataEntities[i];
                long num = Convert.ToInt64(((ExtendedDataEntity)(item))["Id"]);
                //long num2 = Convert.ToInt64(((ExtendedDataEntity)(item))["SaleOrgId_Id"]);
                //string text = Convert.ToString(((ExtendedDataEntity)(item))["VersionNo"]);
                //Convert.ToString(((ExtendedDataEntity)(item))["BillTypeId"]);
                DynamicObjectCollection val = (DynamicObjectCollection)((ExtendedDataEntity)(item))["PLN_FORECASTENTRY"];
                int count = ((Collection<DynamicObject>)(object)val).Count;
                for (int j = 0; j < count; j++)
                {
                    var entryrows = ((Collection<DynamicObject>)(object)val)[j];
                    var StartDate = Convert.ToDateTime(entryrows["StartDate"]);
                    var EndDate = Convert.ToDateTime(entryrows["EndDate"]);

                    if (DateTime.Compare(StartDate.Date, EndDate.Date) == 0)
                    {
                        string errmsg = string.Format(
                        ResManager.LoadKDString("第{0}行分录，开始日期不能与结束日期是同一天，请检查.", "005129030005955",
                        (SubSystemType)5, new object[0])
                        , j + 1);
                        validateContext.AddError((object)null, new ValidationErrorInfo("FEndDate", num.ToString(), item.DataEntityIndex, item.RowIndex, "ForecastOrder", errmsg, "", (ErrorLevel)2));
                    }
                }
            }
        }
    }
}
