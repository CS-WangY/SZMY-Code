using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.Utils;
using System.Collections;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core;

namespace Kingdee.Mymooo.ServicePlugIn.TransferDirect
{
    [Description("直接调拨单反审核插件")]
    [HotUpdate]
    public class UnAudit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FStockOutOrgId");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FSrcStockId");//调出仓库
            e.FieldKeys.Add("FDestStockId");//调入仓库
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FOwnerOutId");//调出货主
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var data in e.DataEntitys)
            {
                //云仓储做出库
                var whDn = (ArrayList)(GetOutDelivery(data));
                //云仓储做入库
                var whInDn = (ArrayList)(GetInDelivery(data));
                //存在出库单，会一起删除入库单。不存在出库单，删除入库单。
                if (whDn.Count > 0)
                {
                    var requestData = JsonConvertUtils.SerializeObject(whDn);
                    var cwResult = WarehouseApiRequest.RequestData(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/cancelleddelivery", requestData, "MYMO", "DELETE");
                    var returnInfo = JsonConvertUtils.DeserializeObject<ResponseCloudWarehouseMessage>(cwResult);
                    if (!returnInfo.IsSuccess)
                    {
                        throw new Exception(returnInfo.Message);
                    }
                }
                else if (whInDn.Count > 0)
                {
                    var requestData = JsonConvertUtils.SerializeObject(whInDn);
                    var cwResult = WarehouseApiRequest.RequestData(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/unshelved", requestData, "MYMO", "DELETE");
                    var returnInfo = JsonConvertUtils.DeserializeObject<ResponseCloudWarehouseMessage>(cwResult);
                    if (!returnInfo.IsSuccess)
                    {
                        throw new Exception(returnInfo.Message);
                    }
                }
            }

        }

        /// <summary>
        /// 构建云仓储做出库接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetOutDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["TransferDirectEntry"] as DynamicObjectCollection)
            {
                if (item["SrcStockId"] != null)
                {
                    //是否同步云仓储
                    if (bool.Parse(((DynamicObject)item["SrcStockId"])["FSyncToWarehouse"].ToString()))
                    {
                        var result = new
                        {
                            itemId = data["BillNo"] + "-" + item["Seq"],
                            exWarehouseOrderNumber = data["BillNo"]
                        };
                        arrList.Add(result);
                    }
                }
            }
            return arrList;
        }

        /// <summary>
        /// 构建云仓储做入库接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetInDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["TransferDirectEntry"] as DynamicObjectCollection)
            {
                if (item["DestStockId"] != null)
                {
                    //是否同步云仓储
                    if (bool.Parse(((DynamicObject)item["DestStockId"])["FSyncToWarehouse"].ToString()))
                    {
                        var result = new
                        {
                            itemId = data["BillNo"] + "-" + item["Seq"],
                            entryWarehouseOrderNumber = data["BillNo"],
                            quantity = Convert.ToDecimal(item["Qty"])
                        };
                        arrList.Add(result);
                    }
                }
            }
            return arrList;
        }
    }
}
