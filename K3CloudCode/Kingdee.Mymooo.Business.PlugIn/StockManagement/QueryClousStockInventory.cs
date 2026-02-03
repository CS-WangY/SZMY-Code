using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.StockManagement;
using System.Reflection.Emit;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    [Description("查询云仓储对应物料库存插件"), HotUpdate]
    public class QueryClousStockInventory : AbstractBillPlugIn
    {
        public override void EntryCellFocued(EntryCellFocuedEventArgs e)
        {
            base.EntryCellFocued(e);
        }
        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            if (e.Key.EqualsIgnoreCase("FKDStock"))
            {
                string itemNo = Convert.ToString(this.View.Model.GetValue("FITEMNO", e.Row));
                string locCode = Convert.ToString(this.View.Model.GetValue("FLOCCODE", e.Row));
                var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "api/goods/situation", "{\"page\":-1,\"pageSize\":-1,\"filter\":null}", "MYMO", "POST");
                var returnInfo = JsonConvertUtils.DeserializeObject<MessageHelp>(cwResult);
                if (!returnInfo.IsSuccess)
                {
                    throw new Exception($"获取云仓储库存失败！");
                }
                List<WarehousesInventoryQueryEntity.Goods> goods = JsonConvertUtils.DeserializeObject<WarehousesInventoryQueryEntity>(returnInfo.Message).goods;


                var result = goods.Where(t =>
                string.Equals(t.modelNumber, itemNo, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(t.warehouse.coding, locCode, StringComparison.CurrentCultureIgnoreCase))
                .Select(t =>
                new WarehousesInventoryDetPartialEntity
                {
                    ItemNo = t.modelNumber,
                    ItemDesc = t.name,
                    Qty = t.quantity,
                    PositionCode = t.position?.coding,
                    PositionDesc = t.position?.address,
                    Uom = t.unit?.name?.ToUpper()
                }).ToList();

                foreach (var item in result)
                {
                    int rowcount = this.Model.GetEntryRowCount("FManagerEntity");
                    this.Model.CreateNewEntryRow("FManagerEntity");
                    this.Model.SetValue("FCSMODELNAMBER", item.ItemNo.ToString(), rowcount); // 物料号
                    this.Model.SetValue("FCSNAME", item.ItemDesc.ToString(), rowcount); //物料名称
                    this.Model.SetValue("FCSQTY", item.Qty, rowcount); //库存数量
                    this.Model.SetValue("FCSUOM", item.Uom.ToString(), rowcount); // 库存单位
                    this.Model.SetValue("FCSCODING", item.PositionCode.ToString(), rowcount); // 云仓储货架号
                    this.Model.SetValue("FCSCODINGNAME", item.PositionDesc.ToString(), rowcount);// 云仓储货架描述
                }
                this.View.UpdateView("FManagerEntity");
            }
        }
    }
}
