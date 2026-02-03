using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    [Description("计划订单批改")]

    [Kingdee.BOS.Util.HotUpdate]


    public class PlanOrderBulkEdit : AbstractListPlugIn
    {
        //点击按钮触发
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            if (e.BarItemKey.Equals("PENY_tbBatchEdit"))
            {
                //获取选择记录
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;

                //读取ID,放到数组
                string[] ID = selectRows.GetPrimaryKeyValues();
                string FID = "";

                //没有选择行提示
                if (ID.Length == 0)
                {
                    //如果选择的是0,即没有选择行记录,弹窗报错,返回
                    this.View.ShowMessage("请选择单据!", MessageBoxType.Notice);
                    return;
                }
                else
                {
                    //如果点击成功,循环ID
                    for (int i = 0; i < ID.Length; i++)
                    {
                        FID = FID + ID[i].ToString() + ",";
                    }
                    DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                    formParameter.FormId = "PENY_PlanBlukEdit";
                    //FID通过字符串传递过去
                    formParameter.CustomParams.Add("FID", FID.Substring(0, FID.Length - 1));
                    this.View.ShowForm(formParameter);
                }
            }
        }
    }
}