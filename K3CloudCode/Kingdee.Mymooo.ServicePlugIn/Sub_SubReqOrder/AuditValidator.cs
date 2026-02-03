using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Mymooo.ServicePlugIn.Sub_SubReqOrder
{
    public class AuditValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            foreach (var data in dataEntities)
            {
                var dynamicObject = data.DataEntity as DynamicObject;
                foreach (var item in data["TreeEntity"] as DynamicObjectCollection)
                {
                    var entid = Convert.ToInt64(item["ID"]);
                    string sSql = $"SELECT FENTRYID FROM T_SUB_PPBOMENTRY WHERE FSUBREQENTRYID={entid}";
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                    if (datas.Count <= 0)
                    {
                        validateContext.AddError(data, new ValidationErrorInfo(
                                  string.Empty,
                                  data["Id"].ToString(),
                                  data.DataEntityIndex,
                                  data.RowIndex,
                                  data["Id"].ToString(),
                                  string.Format("单据编号[{0}]的用料清单为空", data["BillNo"]),
                                  $"批核[{data["BillNo"]}]",
                                  ErrorLevel.FatalError));
                    }
                }
            }
        }
    }
}
