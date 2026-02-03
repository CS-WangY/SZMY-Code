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

namespace Kingdee.Mymooo.Business.PlugIn.ReceivablesManagement
{
    [Description("应收单插件"), HotUpdate]
    public class ReceivableEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FALLAMOUNTFOR_D")|| e.Field.Key.EqualsIgnoreCase("F_PENY_TaxPrice"))
            {
                //价税差额=（原含税单价-含税单价）×数量
                decimal diffAllAmount =
                    (Convert.ToDecimal(this.View.Model.GetValue("F_PENY_TaxPrice", e.Row)) - Convert.ToDecimal(this.View.Model.GetValue("FTaxPrice", e.Row))) *
                    Convert.ToDecimal(this.View.Model.GetValue("FPriceQty", e.Row));
                this.View.Model.SetValue("F_PENY_DIFFALLAMOUNT", diffAllAmount, e.Row);
                //更新头
                Entity entity = this.View.BusinessInfo.GetEntity("FEntityDetail");
                var isApproval = false;
                foreach (var item in this.View.Model.GetEntityDataObject(entity))
                {
                    //表头增加字段”是否需要审批“枚举，单行”价税差额“>20，显示：是
                    if (Convert.ToDecimal(item["F_PENY_DIFFALLAMOUNT"]) > 20)
                    {
                        isApproval = true;
                        break;
                    }
                }
                this.View.Model.SetValue("F_PENY_ISAPPROVAL", isApproval);
            }
        }
    }
}
