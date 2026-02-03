using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugInBill
{
    /// <summary>
    /// 
    /// </summary>
    [Description("测试单据保存插件")]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            
            SaveValidator isPushValidator = new SaveValidator();
            isPushValidator.AlwaysValidate = true;
            isPushValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPushValidator);
        }

        /// <summary>
        /// 事务外 操作前
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
        }

        /// <summary>
        /// 事务中 操作开始
        /// </summary>
        /// <param name="e"></param>
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);

            //执行出错
            //throw new Exception("");
        }

        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //执行出错
            //throw new Exception("");
        }

        /// <summary>
        /// 事务外  操作后
        /// </summary>
        /// <param name="e"></param>
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
        }
    }
}
