using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    /// <summary>
    /// 审批验证
    /// </summary>
    public class AuditValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            //先验证存不存在一个订单有同步和不同步云仓储的情况
            foreach (var data in dataEntities)
            {
                var dynamicObject = data.DataEntity as DynamicObject;
                if (((DynamicObject)dynamicObject["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY") return;
                var list = data["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;
                List<StockInfo> stockList = new List<StockInfo>();
                foreach (var item in list)
                {

                    if (item["StockID"] == null)
                    {
                        validateContext.AddError(data, new ValidationErrorInfo(
                                                        string.Empty,
                                                        data["Id"].ToString(),
                                                        data.DataEntityIndex,
                                                        data.RowIndex,
                                                        data["Id"].ToString(),
                                                        string.Format("单据编号[{0}]明细出货仓库不能为空", data["BillNo"]),
                                                        $"批核[{data["BillNo"]}]",
                                                        ErrorLevel.FatalError));
                    }
                    else
                    {
                        stockList.Add(new StockInfo
                        {
                            OrgId = Int64.Parse(((DynamicObject)item["StockID"])["UseOrgId_Id"].ToString()),
                            //是否同步云仓储
                            SyncToWarehouse = bool.Parse(((DynamicObject)item["StockID"])["FSyncToWarehouse"].ToString())
                        });
                    }
                }
                var newStockList = stockList.GroupBy(p => new { OrgId = p.OrgId, SyncToWarehouse = p.SyncToWarehouse }).Select(x => new
                {
                    OrgId = x.Key.OrgId,
                    SyncToWarehouse = x.Key.SyncToWarehouse
                }).GroupBy(p => p.OrgId).Select(t => new
                {
                    OrgId = t.Key,
                    Count = t.Count()
                }).ToList();

                foreach (var item in newStockList)
                {
                    if (item.Count > 1)
                    {
                        validateContext.AddError(data, new ValidationErrorInfo(
                                          string.Empty,
                                          data["Id"].ToString(),
                                          data.DataEntityIndex,
                                          data.RowIndex,
                                          data["Id"].ToString(),
                                          string.Format("单据编号[{0}]的明细出货仓库存在同步和不同步云仓储", data["BillNo"]),
                                          $"批核[{data["BillNo"]}]",
                                          ErrorLevel.FatalError));
                    }
                }
            }
        }

        public class StockInfo
        {
            /// <summary>
            /// OrgId
            /// </summary>
            public long OrgId { get; set; }

            public bool SyncToWarehouse { get; set; }
        }
    }
}
