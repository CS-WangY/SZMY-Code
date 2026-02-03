using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{

    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var headEntity in dataEntities)
            {
                if ((DynamicObject)headEntity["SupplierId"] == null)
                {
                    if (Convert.ToString(((DynamicObject)headEntity["BillTypeId"])["id"]).EqualsIgnoreCase("0023240234df807511e308990e04cf6a"))
                    {
                        validateContext.AddError(headEntity, new ValidationErrorInfo(
                          string.Empty,
                          headEntity["Id"].ToString(),
                          headEntity.DataEntityIndex,
                          headEntity.RowIndex,
                          headEntity["Id"].ToString(),
                          "VMI采购申请订单选了非VMI供应商，保存失败",
                          $"采购订单保存[{headEntity["BillNo"]}]",
                          ErrorLevel.FatalError));
                    }
                }
                else
                {
                    if (((DynamicObject)headEntity["SupplierId"])["FIsTemporary"].Equals(true))
                    {
                        //获取临时供应商采购订单数
                        int orderNum = GetPoOrderCount(((DynamicObject)headEntity["SupplierId"])["Number"].ToString(), headEntity.BillNo);
                        if (orderNum >= 5)
                        {
                            validateContext.AddError(headEntity, new ValidationErrorInfo(
                                  string.Empty,
                                  headEntity["Id"].ToString(),
                                  headEntity.DataEntityIndex,
                                  headEntity.RowIndex,
                                  headEntity["Id"].ToString(),
                                  string.Format("临时供应商[{0}]超过最大下单次数。", ((DynamicObject)headEntity["SupplierId"])["Name"].ToString()),
                                  $"采购订单保存[{headEntity["BillNo"]}]",
                                  ErrorLevel.FatalError));
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 获取临时供应商的下单次数
        /// </summary>
        /// <param name="vendorCode"></param>
        /// <returns></returns>
        private int GetPoOrderCount(string vendorCode, string orderNo)
        {
            var sqlWhere = "";
            if (!string.IsNullOrEmpty(orderNo))
            {
                sqlWhere = $"  and t1.FBILLNO!='{orderNo}' ";
            }
            var sql = $@"select case when FISTEMPORARY=1 then count(1) else 0 end orderNum from T_PUR_POORDER t1
                                inner join t_BD_Supplier t2 on t1.FSUPPLIERID=t2.FSUPPLIERID
                                where t1.FCANCELSTATUS='A' and t2.FNUMBER='{vendorCode}' {sqlWhere}
                                group by FISTEMPORARY ";
            return DBUtils.ExecuteScalar<int>(this.Context, sql, 0);
        }
    }

}
