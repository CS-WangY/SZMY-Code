using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;


namespace Kingdee.Mymooo.Business.PlugIn.CN_BillReceivableManagement
{
    [Description("应收票据拆分单表单扩展插件"), HotUpdate]
    public class SplitRecBillEditEx : AbstractBillPlugIn
    {
        public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
        {
            //((AbstractDynamicFormPlugIn)this).BeforeDeleteRow(e);
            string text;
            if ((text = e.EntityKey.ToUpperInvariant()) == null || !(text == "FSPLITRECBILLENTITY"))
            {
                return;
            }
            Entity entity = ((IDynamicFormView)((AbstractBillPlugIn)this).View).BusinessInfo.GetEntity("FSPLITRECBILLENTITY");
            DynamicObjectCollection entityDataObject = ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).GetEntityDataObject(entity);
            if (entityDataObject != null && ((Collection<DynamicObject>)(object)entityDataObject).Count >= e.Row && ((Collection<DynamicObject>)(object)entityDataObject).Count > 0)
            {
                DynamicObject val = ((Collection<DynamicObject>)(object)entityDataObject)[e.Row];
                if (val != null && Convert.ToDecimal(val["FAmount"]) == 0m)
                {
                    return;
                }
            }
            if (entityDataObject == null || ((IEnumerable<DynamicObject>)entityDataObject).Where((DynamicObject o) => Convert.ToDecimal(o["FAmount"]) > 0m).Count() < 3)
            {
                ((IDynamicFormView)((AbstractBillPlugIn)this).View).ShowErrMessage(ResManager.LoadKDString("应收票据拆分明细至少保留两行金额大于0，不允许删除!", "0033135030049361", (SubSystemType)4, new object[0]), "", (MessageBoxType)0);
                e.Cancel = true;
            }
        }

        public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
        {
            //((AbstractDynamicFormPlugIn)this).AfterDeleteRow(e);
            string text;
            if ((text = e.EntityKey.ToUpperInvariant()) != null && text == "FSPLITRECBILLENTITY")
            {
                SetSplitRecBillEntity();
            }
        }

        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            //((AbstractDynamicFormPlugIn)this).BeforeUpdateValue(e);
            int entryRowCount = ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).GetEntryRowCount("FSPLITRECBILLENTITY");
            string text;
            if ((text = e.Key.ToUpperInvariant()) == null || !(text == "FAMOUNT") || e.Value == null)
            {
                return;
            }
            decimal value = Convert.ToDecimal(((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FAmount", e.Row));
            decimal num = Convert.ToDecimal(e.Value);
            decimal num2 = Convert.ToDecimal(((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FStandardAmt", 0));
            if (num2 == 0m)
            {
                num2 = 0.01m;
            }
            if (num < num2)
            {
                ((IDynamicFormView)((AbstractBillPlugIn)this).View).ShowErrMessage(string.Format(ResManager.LoadKDString(" 金额不能少于拆分最少单元{0}，不允许修改!", "0033135000028132", (SubSystemType)4, new object[0]), Math.Round(num2, 2)), "", (MessageBoxType)0);
                e.Cancel = true;
            }
            decimal num3 = num - value;
            if (e.Row < entryRowCount - 1)
            {
                decimal value2 = Convert.ToDecimal(((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FAmount", entryRowCount - 1));
                if (num3 > 0m && num3 - value2 > 0m)
                {
                    ((IDynamicFormView)((AbstractBillPlugIn)this).View).ShowErrMessage(ResManager.LoadKDString(" 拆分的金额大于应收票据票面金额，不允许修改!", "0033135030049355", (SubSystemType)4, new object[0]), "", (MessageBoxType)0);
                    e.Cancel = true;
                }
            }
            else if (num3 > 0m)
            {
                ((IDynamicFormView)((AbstractBillPlugIn)this).View).ShowErrMessage(ResManager.LoadKDString(" 拆分的金额大于应收票据票面金额，不允许修改!", "0033135030049355", (SubSystemType)4, new object[0]), "", (MessageBoxType)0);
                e.Cancel = true;
            }
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            //((AbstractDynamicFormPlugIn)this).DataChanged(e);
            string text;
            if ((text = ((AbstractElement)e.Field).Key.ToUpperInvariant()) != null && text == "FAMOUNT")
            {
                int entryRowCount = ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).GetEntryRowCount("FSPLITRECBILLENTITY");
                if (e.Row < entryRowCount - 1)
                {
                    SetSplitRecBillEntity();
                    return;
                }
                ((AbstractDynamicFormPlugIn)this).Model.BatchCreateNewEntryRow("FSPLITRECBILLENTITY", 1);
                SetSplitRecBillEntity();
            }
        }

        protected void SetSplitRecBillEntity()
        {
            Entity entity = ((IDynamicFormView)((AbstractBillPlugIn)this).View).BusinessInfo.GetEntity("FSPLITRECBILLENTITY");
            DynamicObjectCollection entityDataObject = ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).GetEntityDataObject(entity);
            if (entityDataObject == null || ((Collection<DynamicObject>)(object)entityDataObject).Count <= 0)
            {
                return;
            }
            string value = "";
            if (this.View.Model.GetValue<string>("FBILLNUMBER", 0).Contains("-1-"))
            {
                value = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue<string>("FBILLNUMBER", 0);
            }
            else
            {
                value = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue<string>("FRCBILLNUMBER", 0);
            }
            

            decimal num = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FStandardAmt", 0, 0m);
            long value2 = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FSubbillStartNo", 0, 0L);
            long value3 = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FSubbillEndNo", 0, 0L);
            if (num == 0m)
            {
                num = 0.01m;
            }
            decimal value4 = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FPARAMOUNTFOR", 0, 0m);
            int row = 0;
            ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).BeginIniti();
            for (int i = 0; i < ((Collection<DynamicObject>)(object)entityDataObject).Count; i++)
            {
                if (i == ((Collection<DynamicObject>)(object)entityDataObject).Count - 1 && value4 > 0m)
                {
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FAMOUNT", (object)value4, i);
                }
                long num2 = value2;
                decimal value5 = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FAMOUNT", i, 0m);
                if (i < ((Collection<DynamicObject>)(object)entityDataObject).Count - 1 && value5 <= 0m)
                {
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillStartNo", (object)0, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillEndNo", (object)0, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FERCBILLNUMBER", (object)null, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillRange", (object)null, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillnumber", (object)0, i);
                    continue;
                }
                if (i > 0)
                {
                    num2 = ((IDynamicFormModel)(object)((AbstractBillPlugIn)this).View.Model).GetValue("FESubbillEndNo", row, 0L) + 1;
                }
                if (value3 < num2)
                {
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FAMOUNT", (object)0, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillStartNo", (object)0, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillEndNo", (object)0, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FERCBILLNUMBER", (object)null, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillRange", (object)null, i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillnumber", (object)0, i);
                    continue;
                }
                ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("fseq", (object)(i + 1), i);
                ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillStartNo", (object)num2, i);
                if (value5 > 0m && num2 > 0 && num > 0m)
                {
                    long num3 = num2 + (long)(value5 / num) - 1;
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillEndNo", (object)num3, i);
                    //((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FERCBILLNUMBER", (object)(value + "-" + Convert.ToString(num2) + "-" + Convert.ToString(num3)), i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FERCBILLNUMBER", (object)(value + "-" + Convert.ToString((i + 1))), i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillRange", (object)(Convert.ToString(num2) + "-" + Convert.ToString(num3)), i);
                    ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).SetValue("FESubbillnumber", (object)(num3 - num2 + 1), i);
                    row = i;
                }
                value4 -= value5;
            }
            ((IDynamicFormModel)((AbstractBillPlugIn)this).View.Model).EndIniti();
            ((IDynamicFormView)((AbstractBillPlugIn)this).View).UpdateView("FSPLITRECBILLENTITY");
        }
    }
}