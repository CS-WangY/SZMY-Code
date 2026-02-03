using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
 using Kingdee.BOS.App.Data;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    /// <summary>
    /// 明细关闭验证
    /// </summary>
    public class DetClosedValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            foreach (var data in dataEntities)
            {
                var billid = data["Id"];
                //存在正常的发货通知单，没出库的。不能关闭销售订单。
                var sql = $@"select top 1 t1.FBILLNO from  T_SAL_DELIVERYNOTICE t1 
                                inner join T_SAL_DELIVERYNOTICEENTRY t2 on t1.FID=t2.FID
                                inner join T_SAL_DELIVERYNOTICEENTRY_LK t3 on t2.FENTRYID=t3.FENTRYID
                                where  t1.FCLOSESTATUS='A' and t1.FCANCELSTATUS='A'
                                and  t3.FSID='{billid}'  and t2.FSumOutQty=0 ";
                var retStr = DBUtils.ExecuteScalar<string>(this.Context, sql, string.Empty);
                if (!string.IsNullOrWhiteSpace(retStr))
                {
                    validateContext.AddError(data, new ValidationErrorInfo(
                                    string.Empty,
                                    data["Id"].ToString(),
                                    data.DataEntityIndex,
                                    data.RowIndex,
                                    data["Id"].ToString(),
                                    string.Format("单据编号[{0}]已存在正常未出库的发货通知单[{1}]，不能关闭销售订单", data.BillNo + "-" + data["Seq"], retStr),
                                    $"行关闭[{data.BillNo}]",
                                    ErrorLevel.FatalError));
                }

            }
        }
    }
}
