using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单撤销解锁库存插件"), HotUpdate]
    public class UnSubmit : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //解锁修改以预留方式做锁库后此处不用了
            //检查电缸厂/型材厂发外仓是否有库存，如果有库存直接锁定；如果没有库存锁定电缸厂/型材厂除发外仓外的其它可用仓的库存
            //foreach (var item in e.DataEntitys)
            //{
            //    var fid = Convert.ToInt64(item["Id"]);
            //    var billno = item["BillNo"].ToString();
            //    foreach (var entitem in item["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
            //    {
            //        //取物料的mster码
            //        var materialid = entitem["materialid"] as DynamicObject;
            //        var msterID = Convert.ToString(materialid["msterID"]);
            //        string srcbillid = "";
            //        string srcrowid = "";
            //        foreach (var itemlink in entitem["FEntity_Link"] as DynamicObjectCollection)
            //        {
            //            srcbillid = itemlink["SBillId"] as string;
            //            srcrowid = itemlink["SId"] as string;
            //        }
            //        string sSql = $@"DELETE T_PLN_RESERVELINKENTRY
            //                --SELECT * 
            //                FROM T_PLN_RESERVELINKENTRY t1
            //                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
            //                WHERE t1.FSUPPLYFORMID='STK_Inventory' AND t2.FSRCINTERID='{srcbillid}' and t2.FSRCENTRYID='{srcrowid}' AND t1.FLINKTYPE=0";
            //        DBUtils.Execute(this.Context, sSql);
            //        ////更新销售订单预留类型
            //        sSql = $@"UPDATE T_PLN_RESERVELINK SET FRESERVETYPE=2 WHERE FSRCINTERID='{srcbillid}' AND FSRCENTRYID='{srcrowid}'";
            //        DBUtils.Execute(this.Context, sSql);
            //    }
            //}
        }
    }
}
