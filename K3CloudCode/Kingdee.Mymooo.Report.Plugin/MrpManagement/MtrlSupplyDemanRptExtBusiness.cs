using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Report.Plugin.MrpManagement
{
    [Description("MRP意外信息报表-表单插件"), HotUpdate]
    public class MtrlSupplyDemanRptExtBusiness : AbstractSysReportPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);
            if (e.FieldKey.EqualsIgnoreCase("FMaterialNumber"))
            {
                var materialId = GetValue(e.Row - 1, "FMaterialID");
                if (materialId.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    ShowForm("PLN_RESERVELINKQUERY", materialId);
                }
            }
        }
        private void ShowForm(string fromId, string pkValue)
        {
            BillShowParameter showParameter = new BillShowParameter();
            showParameter.FormId = fromId;
            showParameter.OpenStyle.ShowType = ShowType.Floating;
            showParameter.Status = OperationStatus.EDIT;
            //showParameter.PKey = pkValue;
            showParameter.CustomParams.Add("FMaterialID", pkValue);
            View.ShowForm(showParameter);
        }
        private string GetValue(int row, string fieldkey)
        {
            DataTable dt = ((ISysReportModel)this.View.Model).DataSource;
            if (row >= 0 && row <= dt.Rows.Count)
            {
                return Convert.ToString(dt.Rows[row][fieldkey]);
            }
            return string.Empty;
        }
    }
}
