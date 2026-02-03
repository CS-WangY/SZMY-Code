using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.PLN_REQUIREMENTORDER
{
    [Description("组织间需求单关闭-刷新关闭时间关闭人"), HotUpdate]
    public class Closed : AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
            }
        }
    }
}
