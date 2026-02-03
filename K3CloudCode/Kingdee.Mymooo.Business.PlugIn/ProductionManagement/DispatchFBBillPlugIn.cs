using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    [Description("销售订单派产云平台单据插件")]
    public class DispatchFBBillPlugIn : AbstractBillPlugIn
    {
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
        }

        private void CalDiscountAmount(BOSActionExecuteContext ctx)
        {
            throw new NotImplementedException();
        }

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            if (e.BarItemKey == "PENY_DispatchFB")
            {

            }

            //this.View.ShowMessage("销售订单派产云平台菜单点击");
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("F_PENY_DISCOUNTRATE") || e.Field.Key.EqualsIgnoreCase("F_PENY_AMOUNT"))
            {
                var amount = Convert.ToDecimal(this.View.Model.GetValue("F_PENY_AMOUNT", e.Row));
                var rate = Convert.ToDecimal(this.View.Model.GetValue("F_PENY_DISCOUNTRATE", e.Row));

                this.View.Model.SetValue("F_PENY_Discount", amount * rate / 100, e.Row);
            }
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            if (e.FieldKey.EqualsIgnoreCase("F_PENY_Base"))
            {
                e.ListFilterParameter.Filter = "FNumber = 'PRE001'";
            }
        }
    }
}
