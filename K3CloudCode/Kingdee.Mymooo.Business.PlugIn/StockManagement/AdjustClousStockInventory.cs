using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    [Description("调整云仓储库存插件"), HotUpdate]
    public class AdjustClousStockInventory : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            //if (e.BarItemKey == "PENY_AdjustClousStock")
            //{
            //    var dataEntities = this.ListView.SelectedRowsInfo;
            //    List<PutToTempStockAreaRequest> putToTempStockAreaRequests = new List<PutToTempStockAreaRequest>();
            //    List<PutToTempDeliveryAreaRequest> putToTempDeliveryAreaRequests = new List<PutToTempDeliveryAreaRequest>();
            //    foreach (var dataEntity in dataEntities)
            //    {
            //        DynamicObjectDataRow data = dataEntity.DataRow as DynamicObjectDataRow;
            //        //string modelNumber = Convert.ToString(((DynamicObject)data["FCSMODELNAMBER"])["Number"]);
            //        string modelNumber = data["FCSMODELNAMBER"].ToString();
            //        string name = data["FCSNAME"].ToString();
            //        decimal kdqty = Convert.ToDecimal(data["FERPQTY"]);
            //        decimal cloudstockqty = Convert.ToDecimal(data["FCSQTY"]);
            //        string uom = data["FCSQTY"].ToString();
            //        decimal adjustQty = kdqty - cloudstockqty;
            //        if (adjustQty == 0)
            //        {
            //            continue;
            //        }
            //        else if (adjustQty > 0)// 云仓储做入库
            //        {
            //            PutToTempStockAreaRequest putToTempStockAreaRequest = new PutToTempStockAreaRequest
            //            {
            //                ItemId = data["BillNo"] + "-" + data["Seq"],
            //                EntryWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
            //                ModelNumber = modelNumber,
            //                Name = name,
            //                Specification = Convert.ToString(data["FUOM"]),
            //                Quantity = adjustQty,
            //                EntryOnUtc = DateTime.Now,
            //                Unit = new NewUnitModel
            //                {
            //                    Name = uom,
            //                },
            //                Type = new ExternalTypeModel
            //                {
            //                    Value = "AOUT",
            //                    Description = "调整出仓"
            //                },
            //                Remark = "仓库盘点"
            //            };
            //            putToTempStockAreaRequests.Add(putToTempStockAreaRequest);

            //        }
            //        else// 云仓储做出库
            //        {
            //            PutToTempDeliveryAreaRequest putToTempDeliveryAreaRequest = new PutToTempDeliveryAreaRequest
            //            {
            //                ItemId = data["BillNo"] + "-" + data["Seq"],
            //                ExWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
            //                ModelNumber = modelNumber,
            //                Name = name,
            //                Specification = Convert.ToString(data["Model"]),
            //                Quantity = Math.Abs(adjustQty),
            //                ExWarehouseOnUtc = DateTime.Now.ToUniversalTime(),
            //                Unit = new NewUnitModel
            //                {
            //                    Name = uom,
            //                },
            //                Type = new ExternalTypeModel
            //                {
            //                    Value = "AIN",
            //                    Description = "调整进仓"
            //                },
            //                Remark = "仓库盘点"
            //            };
            //            putToTempDeliveryAreaRequests.Add(putToTempDeliveryAreaRequest);
            //        }
            //    }
            //    // 写入rabbitmq消息队列
            //    if (putToTempStockAreaRequests.Count > 0)
            //    {
            //        var updateTempStockAreaMessage = JsonConvertUtils.SerializeObject(putToTempStockAreaRequests);
            //        AddRabbitMessageUtils.AddRabbitMessage(this.Context, updateTempStockAreaMessage, "CloudStockUpdateTempStockArea", putToTempStockAreaRequests.FirstOrDefault()?.EntryWarehouseOrderNumber);
            //    }
            //    if (putToTempDeliveryAreaRequests.Count > 0)
            //    {
            //        var updateTempDeliveryAreaMessage = JsonConvertUtils.SerializeObject(putToTempDeliveryAreaRequests);
            //        AddRabbitMessageUtils.AddRabbitMessage(this.Context, updateTempDeliveryAreaMessage, "CloudStockUpdateTempDeliveryArea", putToTempDeliveryAreaRequests.FirstOrDefault()?.ExWarehouseOrderNumber);
            //    }
            //}
        }
    }
}
