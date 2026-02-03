using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.MFG.PLN.App.Core;

namespace Kingdee.Mymooo.Business.PlugIn.MrpModelManagement
{
    public class MymoooMrpComputePlugInEx : MrpComputeService
    {
        public override IOperationResult RunMrp(Context ctx, DynamicObject mrpDataObject, OperateOption option)
        {
            var ss = base.RunMrp(ctx, mrpDataObject, option);
            return ss;
        }
    }
}
