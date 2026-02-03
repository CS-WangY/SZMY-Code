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

namespace Kingdee.Mymooo.ServicePlugIn.InStock
{
    [Description("生产入库反写插件-调用云平台"), HotUpdate]
    /// <summary>
    /// 生产入库反写插件
    /// </summary>
    public class InStockWriteBack : AbstractBusinessFlowServicePlugIn
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
        public override void AfterCloseRow(AfterCloseRowEventArgs e)
        {
            //var billstatus = e.BillCloseFieldStatus;

            if (_thisIsMyRule)
            {
                //判断是否非标销售订单
                var billtype = e.SourceDataObject["BillTypeId"] as DynamicObject;
                if (Convert.ToString(billtype["Number"]).EqualsIgnoreCase("FBXSDD"))
                {
                    //string disurl = Kingdee.Mymooo.Core.Utils.WebApiServiceUtils.DispatchToCloudUrl;
                    //string url = disurl + "api/cnc/partcosts/updatepartcoststatus";

                    List<string> PartCostNumbers = new List<string>();
                    foreach (var item in e.SourceDataObject["SaleOrderEntry"] as DynamicObjectCollection)
                    {
                        var MaterialId = item["MaterialId"] as DynamicObject;
                        PartCostNumbers.Add(Convert.ToString(MaterialId["Number"]));
                    }
                    var pairs = new
                    {
                        OrderNumber = Convert.ToString(e.SourceDataObject["BillNo"]),
                        //ExpressNumber = Convert.ToString(e.SourceDataObject["BillNo"]),
                        PartCostNumbers,
                        PartCostStatus = 7
                    };

                    //string spairs = JsonConvertUtils.SerializeObject(pairs);
                    //var request = JsonConvertUtils.DeserializeObject<NonStandardRequest>(Kingdee.Mymooo.Core.Utils.WebApiServiceUtils.HttpPost(url, pairs));
                    //if (!request.success)
                    //{
                    //    //记录数据，用于查询。
                    //    var logSql = $@"/*dialect*/ insert into RabbitMQScheduledMessage (FAction,FKeyword,FMessage,FCreateDate,FExecuteDate,FIsExecute) 
                    //                    values  ('CNCAPI','{Convert.ToString(e.SourceDataObject["BillNo"])}','{request.msg}',SYSDATETIME(),SYSDATETIME(),'1')";
                    //    DBUtils.Execute(Context, logSql);
                    //}
                }
            }
        }
    }
}
