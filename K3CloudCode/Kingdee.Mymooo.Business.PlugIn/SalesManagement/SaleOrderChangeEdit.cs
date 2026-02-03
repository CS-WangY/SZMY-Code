using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单新变更单单据插件"), HotUpdate]
    public class SaleOrderChangeEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FAllDisCount") || e.Field.Key.EqualsIgnoreCase("FChangeType"))
            {
                var allDisAmount = this.View.Model.GetValue<decimal>("FAllDisCount", 0, 0);
                var entity = this.View.BusinessInfo.GetEntity("FSaleOrderEntry");
                var entryDatas = this.View.Model.GetEntityDataObject(entity);
                var editEntryDatas = entryDatas.Where(p => !p.GetValue<string>("ChangeType", "").EqualsIgnoreCase("D") && !p.GetValue<string>("ChangeType", "").EqualsIgnoreCase("T")).ToList();
                var allocationEntryDatas = editEntryDatas.Where(p => !p.GetValue<string>("ChangeType", "").EqualsIgnoreCase("C")).ToList();
                var amount = allocationEntryDatas.Sum(p => p.GetValue<decimal>("AllAmountExceptDisCount", 0));
                var disAmount = editEntryDatas.Where(p => p.GetValue<string>("ChangeType", "").EqualsIgnoreCase("C")).Sum(p => p.GetValue<decimal>("Discount", 0));
                allDisAmount -= disAmount;
                var rate = amount == 0 ? 0 : Math.Round(allDisAmount / amount * 100, 6, MidpointRounding.AwayFromZero);

                foreach (var allocationEntryData in allocationEntryDatas)
                {
                    int rowIndex = entryDatas.IndexOf(allocationEntryData);
                    this.View.Model.SetValue("FDiscountRate", rate, rowIndex);
                    this.View.InvokeFieldUpdateService("FDiscountRate", rowIndex);
                }

                //差异调整到金额最大行
                var rowDisAmount = allocationEntryDatas.Sum(p => p.GetValue<decimal>("Discount", 0));
                if (allDisAmount != rowDisAmount)
                {
                    var max = allocationEntryDatas.OrderByDescending(p => p.GetValue<decimal>("Discount", 0)).FirstOrDefault();
                    if (max != null)
                    {
                        this.View.Model.SetValue("FDiscount", max.GetValue<decimal>("Discount", 0) + allDisAmount - rowDisAmount, entryDatas.IndexOf(max));
                    }
                }

                //调整加税合计
                foreach (var allocationEntryData in allocationEntryDatas)
                {
                    int rowIndex = entryDatas.IndexOf(allocationEntryData);
                    this.View.Model.SetValue("FAllAmount", this.View.Model.GetValue<decimal>("FAllAmountExceptDisCount", rowIndex, 0) - this.View.Model.GetValue<decimal>("FDiscount", rowIndex, 0), rowIndex);
                }
            }
        }

        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            base.BeforeUpdateValue(e);
            if (e.Key.EqualsIgnoreCase("FAllDisCount"))
            {
                var disAmount = Convert.ToDecimal(e.Value);
                var entity = this.View.BusinessInfo.GetEntity("FSaleOrderEntry");
                var entryDatas = this.View.Model.GetEntityDataObject(entity);
                var amount = entryDatas.Where(p => !p.GetValue<string>("ChangeType", "").EqualsIgnoreCase("D")).Sum(p => p.GetValue<decimal>("AllAmountExceptDisCount", 0));

                if (disAmount > amount)
                {
                    this.View.ShowMessage("整单折扣金额不能大于整单金额!");
                    e.Cancel = true;
                }
            }
        }
    }
}
