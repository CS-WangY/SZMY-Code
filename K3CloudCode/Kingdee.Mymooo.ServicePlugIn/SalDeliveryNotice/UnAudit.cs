using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServicePlugIn.SalesBill;
using System;
using System.Collections;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单反批核审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class UnAudit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FDeliveryOrgID");//发货组织
        }
        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //foreach (var data in e.DataEntitys)
            //{
            //    if (((DynamicObject)data["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY") return;
            //    var whDn = (ArrayList)(GetDelivery(data));
            //    var requestData = JsonConvertUtils.SerializeObject(whDn);
            //    //云仓储删除送货单
            //    AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockDeleteTempDeliveryArea", Convert.ToString(data["BillNo"]));

            //}
        }

        /// <summary>
        /// 构建发货通知单接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
            {
                var result = new
                {
                    itemId = data["BillNo"] + "-" + item["Seq"],
                    exWarehouseOrderNumber = data["BillNo"]
                };
                arrList.Add(result);
            }
            return arrList;
        }

    }
}
