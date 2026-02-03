using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("套打插件-明细重复字段显示空白"), HotUpdate]
    public class PrintPlugin : AbstractListPlugIn
    {
        public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
        {
            base.OnPrepareNotePrintData(e);
            //这里获取打印的数据
            List<DynamicObject> AfterDealDate = new List<DynamicObject>();
            //解释：此方法会被执行两次，参数e的DataSourceId标志进来的是单据体或者单据头
            if (e.DataSourceId.Equals("FPAYAPPLYENTRY"))//如果是单据体数据进来，进行处理
            {
                foreach (var ob in e.DataObjects)
                {
                    string num2 = Convert.ToString(ob["FSRCBILLNO"]);
                    if (!AfterDealDate.Select(x => x["FSRCBILLNO"].ToString()).Contains(num2))
                    {
                        AfterDealDate.Add(ob);
                    }
                }
                e.DataObjects = AfterDealDate.ToArray();
            }
        }
    }
    [Description("套打插件表单-明细重复字段显示空白"), HotUpdate]
    public class PrintBillPlugin : AbstractBillPlugIn
    {
        public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
        {
            base.OnPrepareNotePrintData(e);
            //这里获取打印的数据
            List<DynamicObject> AfterDealDate = new List<DynamicObject>();
            //解释：此方法会被执行两次，参数e的DataSourceId标志进来的是单据体或者单据头
            if (e.DataSourceId.Equals("FPAYAPPLYENTRY"))//如果是单据体数据进来，进行处理
            {
                foreach (var ob in e.DataObjects)
                {
                    string num2 = Convert.ToString(ob["FSRCBILLNO"]);
                    if (!AfterDealDate.Select(x => x["FSRCBILLNO"].ToString()).Contains(num2))
                    {
                        AfterDealDate.Add(ob);
                    }
                }
                e.DataObjects = AfterDealDate.ToArray();
            }
        }
    }
}
