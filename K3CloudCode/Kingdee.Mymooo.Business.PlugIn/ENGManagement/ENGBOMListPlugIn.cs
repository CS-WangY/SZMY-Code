using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.ENGManagement
{
    [Description("导入EXCEL文件，多料号同时导入"), HotUpdate]
    public class ENGBOMListPlugIn : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if (e.BarItemKey.EqualsIgnoreCase("PENY_ImportExcel"))
            {
                DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                formParameter.FormId = "PENY_ImportExcelBom";
                //formParameter.OpenStyle.ShowType = ShowType.Modal;
                //FID通过字符串传递过去
                //formParameter.CustomParams.Add("FID", FID.Substring(0, FID.Length - 1));
                this.View.ShowForm(formParameter);
            }
        }
    }
}
