using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("退货通知单插件"), HotUpdate]
    public class SalReturnNoticeEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FPENYRmType"))
            {
                Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
                int i = 0;
                foreach (var item in this.View.Model.GetEntityDataObject(entity))
                {
                    this.View.Model.SetValue("FRmType", e.NewValue, i++);
                }
            }
        }
    }
}
