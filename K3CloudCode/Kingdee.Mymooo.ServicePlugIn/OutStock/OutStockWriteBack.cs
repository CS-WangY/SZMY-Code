using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.BusinessFlow.PlugIn;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.OutStock
{
    [Description("销售出库反写插件-调用云平台"), HotUpdate]
    /// <summary>
    /// 销售出库反写插件
    /// </summary>
    public class OutStockWriteBack : AbstractBusinessFlowServicePlugIn
    {
        /// <summary>
        /// 当前是否为本插件关注的反写规则?
        /// </summary>
        /// <remarks>
        /// 从BeforeWriteBack事件开始，事件基于反写规则，循环触发；
        /// 为避免重复反写，需要判断当前反写规则
        /// </remarks>
        //private bool _thisIsMyRule = false;

        public override void AfterCloseRow(AfterCloseRowEventArgs e)
        {
            //var billstatus = e.BillCloseFieldStatus;

            if (e.SourceBusinessInfo.GetForm().Id.EqualsIgnoreCase("SAL_SaleOrder")
                && e.BillCloseFieldStatus.EqualsIgnoreCase("B")
                && e.OperationName.EqualsIgnoreCase("Audit")
                )
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
                        ExpressNumber = Convert.ToString(e.SourceDataObject["BillNo"]),
                        PartCostNumbers,
                        PartCostStatus = 8
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
