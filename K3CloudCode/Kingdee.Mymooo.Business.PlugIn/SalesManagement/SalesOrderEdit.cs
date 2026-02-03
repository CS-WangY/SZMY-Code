using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单单据插件"), HotUpdate]
    public class SalesOrderEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            //蚂蚁物料
            if (Convert.ToString(((DynamicObject)this.View.Model.DataObject["BillTypeId"])?["Name"]) == "退货订单")
            {
                if (e.Field.Key.EqualsIgnoreCase("FQty"))
                {

                }
            }
        }

        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);
            InvokeField("FSaleOrderEntry", "FQty");//上游单据下推时，触发字段值更新事件
        }

        private void InvokeField(string FEntryID, string FieldKey)
        {
            Entity entity = this.Model.BusinessInfo.GetEntity(FEntryID);
            DynamicObjectCollection dy = this.Model.GetEntityDataObject(entity);
            for (int i = 0; i < dy.Count; i++)
            {
                this.View.InvokeFieldUpdateService(FieldKey, i);
            }
        }

    }
}
