using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
 using Kingdee.BOS.App.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    /// <summary>
    /// 销售单反审核
    /// </summary>
    public class UnAuditValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            foreach (var data in dataEntities)
            {
                //销售单编号
                var salesOrderNo = data["BillNo"].ToString();
                //验证采购单
                //采购单编号
                var poBillNo = "";
                List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, salesOrderNo) };
                var sql = @"select top 1 FBILLNO from (	 
						select t3.FBILLNO
                        from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
						inner join t_PUR_POOrder t3 on  t1.FID=t3.FID
                        where isnull(t2.FDEMANDBILLNO,'')<>'' and t3.FCANCELSTATUS='A' and t2.FDEMANDBILLNO=@FBILLNO
                        union
                        select t6.FBILLNO
                         from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                        left join T_PUR_REQENTRY_LK t3 on t3.FENTRYID=t2.FSID
                        left join T_PLN_PLANORDER_LK t4 on t3.FSID=t4.FID
                        left JOIN T_PLN_PLANORDER_B t5 ON t4.FSBILLID=t5.FID
						inner join t_PUR_POOrder t6 on  t1.FID=t6.FID
                        where isnull(t5.FSALEORDERNO,'')<>'' and t6.FCANCELSTATUS='A' and t5.FSALEORDERNO=@FBILLNO
						) datas";
                var datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                foreach (var item in datas)
                {
                    poBillNo = item["FBILLNO"].ToString();
                }
                if (!string.IsNullOrWhiteSpace(poBillNo))
                {
                    validateContext.AddError(data, new ValidationErrorInfo(
                                      string.Empty,
                                      data["Id"].ToString(),
                                      data.DataEntityIndex,
                                      data.RowIndex,
                                      data["Id"].ToString(),
                                      string.Format("销售单“{0}”已存在未作废的采购单“{1}”,不允许反审核！", data["BillNo"], poBillNo),
                                      $"反批核[{data["BillNo"]}]",
                                      ErrorLevel.FatalError));
                }
                //验证组织间需求单
                //组织间需求单编号
                var reqBillNo = "";
                sql = @" SELECT TOP 1 FBILLNO FROM T_PLN_REQUIREMENTORDER where FSALEORDERNO=@FBILLNO and FISCLOSED='A' ";
                datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                foreach (var item in datas)
                {
                    reqBillNo = item["FBILLNO"].ToString();
                }
                if (!string.IsNullOrWhiteSpace(reqBillNo))
                {
                    validateContext.AddError(data, new ValidationErrorInfo(
                                      string.Empty,
                                      data["Id"].ToString(),
                                      data.DataEntityIndex,
                                      data.RowIndex,
                                      data["Id"].ToString(),
                                      string.Format("销售单“{0}”已存在未关闭组织间需求单“{1}”,不允许反审核！", data["BillNo"], reqBillNo),
                                      $"反批核[{data["BillNo"]}]",
                                      ErrorLevel.FatalError));
                }
            }
        }
    }
}
