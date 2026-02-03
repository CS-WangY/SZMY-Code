using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core
{
    public class DemoTestService : IDemoTestService
    {
        public string DemoTestAction(Context ctx)
        {
            var test = DBUtils.ExecuteDynamicObject(ctx, "select 1 from T_BD_Material");
            return "测试成功";
        }
    }
}
