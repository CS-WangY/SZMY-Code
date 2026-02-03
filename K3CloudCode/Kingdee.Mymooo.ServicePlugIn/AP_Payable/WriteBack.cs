using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.BOS.Core.BusinessFlow.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
 using Kingdee.BOS.App.Data;

namespace Kingdee.Mymooo.ServicePlugIn.AP_Payable
{
    [Description("应收单反写插件-销售未关联金额"), HotUpdate]
    public class WriteBack : AbstractBusinessFlowServicePlugIn
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
            if (srcformId.EqualsIgnoreCase("SAL_OUTSTOCK")
                //&&
                //(
                //operNumber.EqualsIgnoreCase("AUDIT") 
                //|| operNumber.EqualsIgnoreCase("UNAUDIT")
                //|| operNumber.EqualsIgnoreCase("SAVE")
                //|| operNumber.EqualsIgnoreCase("DELETE")
                //|| operNumber.EqualsIgnoreCase("CANCEL")
                //|| operNumber.EqualsIgnoreCase("UNCANCEL")
                //)
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
                e.AddFieldKey("FPENYNotJoinAmount");
                e.AddFieldKey("FAllAmount");
                e.AddFieldKey("FARJOINAMOUNT");
            }
        }
        public override void AfterCommitAmount(AfterCommitAmountEventArgs e)
        {
            base.AfterCommitAmount(e);
            string srcFormId = e.SourceBusinessInfo.GetForm().Id;
            if (this.WriteBackDeliPlanCondition(srcFormId, this.OperationNumber))
            {
                e.SourceActiveRow["FPENYNotJoinAmount"] = Convert.ToDecimal(e.SourceActiveRow["AllAmount"])
                    - Convert.ToDecimal(e.SourceActiveRow["ARJOINAMOUNT"]);
            }
        }

        public override void AfterCloseRow(AfterCloseRowEventArgs e)
        {
            //var billstatus = e.BillCloseFieldStatus;
            if (_thisIsMyRule)
            {
                //e.SourceDataObject["aa"] = 1;
            }
        }

        public override void BeforeSaveWriteBackData(BeforeSaveWriteBackDataEventArgs e)
        {
            base.BeforeSaveWriteBackData(e);

        }
    }
}
