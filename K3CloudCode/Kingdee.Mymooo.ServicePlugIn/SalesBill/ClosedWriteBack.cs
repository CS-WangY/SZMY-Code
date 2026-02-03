using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.BusinessFlow.PlugIn;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Mymooo.ServicePlugIn.InStock
{
    [Description("销售出库反写销售订单关闭插件"), HotUpdate]
    /// <summary>
    /// 销售出库反写插件
    /// </summary>
    public class ClosedWriteBack : AbstractBusinessFlowServicePlugIn
    {
        /// <summary>
        /// 当前是否为本插件关注的反写规则?
        /// </summary>
        /// <remarks>
        /// 从BeforeWriteBack事件开始，事件基于反写规则，循环触发；
        /// 为避免重复反写，需要判断当前反写规则
        /// </remarks>
        private bool _thisIsMyRule = false;
        /// <summary>
        /// 循环中，逐一执行反写规则之前，触发本事件；
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// 判断当前规则，是否为本插件需关注的规则
        /// </remarks>
        public override void BeforeWriteBack(BeforeWriteBackEventArgs e)
        {
            string srcFormId = e.SourceBusinessInfo.GetForm().Id;
            if (this.WriteBackDeliPlanCondition(srcFormId, this.OperationNumber))
            {
                _thisIsMyRule = true;
            }
        }
        private bool WriteBackDeliPlanCondition(string srcformId, string operNumber)
        {
            //判断源单是到采购订单
            //因为现在是根据库存更新时点反写累计数量的，保存、审核根据库存更新时点配置只会执行一次//(无需取库存更新时点参数)
            //反写分配交货计划明细行的过程是每次根据累计数量重新分配的，类似于恒等式
            if (srcformId.EqualsIgnoreCase("SAL_SaleOrder") &&
                (
                operNumber.EqualsIgnoreCase("AUDIT") //|| operNumber.EqualsIgnoreCase("UNAUDIT") ||
                                                     //operNumber.EqualsIgnoreCase("SAVE") || operNumber.EqualsIgnoreCase("DELETE") ||
                                                     //operNumber.EqualsIgnoreCase("CANCEL") || operNumber.EqualsIgnoreCase("UNCANCEL")
                )
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void AfterCustomReadFields(AfterCustomReadFieldsEventArgs e)
        {
            if (_thisIsMyRule == true)
            {
                e.AddFieldKey("FBillTypeID");
            }
        }
        public override void AfterSaveWriteBackData(AfterSaveWriteBackDataEventArgs e)
        {
            base.AfterSaveWriteBackData(e);
        }
        public override void AfterCommitAmount(AfterCommitAmountEventArgs e)
        {
            base.AfterCommitAmount(e);
            
        }
        public override void AfterCloseRow(AfterCloseRowEventArgs e)
        {
            

            if (_thisIsMyRule)
            {

            }
        }
        
        public override void FinishWriteBack(FinishWriteBackEventArgs e)
        {
            base.FinishWriteBack(e);
            //string srcFormId = e.SourceBusinessInfo.GetForm().Id;
            //if (srcFormId == "SAL_SaleOrder")
            //{
            //    var billstatus = e.EntryCloseFieldStatus;
            //    var oldbillstatus = e.EntryOldCloseFieldStatus;
            //    if (oldbillstatus == "A" && billstatus == "B")
            //    {
            //        var request = new List<SyncOrderDeliveryRequest>();
            //        string sSql = $"SELECT FORDERDETAILID FROM dbo.T_SAL_ORDERENTRY WHERE FENTRYID={e.SourceActiveRow["Id"]}";
            //        var orderid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0);
            //        var newrows = new SyncOrderDeliveryRequest();
            //        newrows.BillNo = Convert.ToString(e.SourceDataObject["BillNo"]);
            //        newrows.ActualQuantity = Convert.ToDecimal(e.SourceActiveRow["BaseDeliQty"]);
            //        newrows.DeliveryDate = System.DateTime.Now;
            //        newrows.DetailId = orderid;
            //        request.Add(newrows);

            //        KafkaProducerService kafkaProducer = new KafkaProducerService();
            //        List<RabbitMQMessage> messages = new List<RabbitMQMessage>
            //        {
            //            new RabbitMQMessage()
            //            {
            //                Exchange = "mall-salesOrder",
            //                Routingkey = "SyncOrderDelivery",
            //                Keyword =Convert.ToString(e.SourceDataObject["BillNo"]),
            //                Message = JsonConvertUtils.SerializeObject(request)
            //            }
            //        };
            //        kafkaProducer.AddMessage(this.Context, messages.ToArray());
            //    }
            //}
        }
    }
}
