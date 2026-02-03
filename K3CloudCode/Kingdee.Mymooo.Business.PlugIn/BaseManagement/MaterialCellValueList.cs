using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.PUR;
using Kingdee.Mymooo.Core.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("物料增加查看BOM版本字段"), HotUpdate]
    public class MaterialCellValueList : AbstractListPlugIn
    {
        string sSql;
        DynamicObjectCollection datas;
        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);

            //判断,加载到新增的即时库存这个字段,读到这里
            if (args.Header.FieldName.EqualsIgnoreCase("FPENYBOMN"))
            {
                string MaterialId = args.DataRow["FMATERIALID"].ToString();

                sSql = $@"/*dialect*/SELECT top 1 FNUMBER FROM T_ENG_BOM where fmaterialid={MaterialId} and FUSEORGID={this.Context.CurrentOrganizationInfo.ID} ORDER BY FID DESC";

                datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                if (datas.Count > 0)
                {
                    args.FormateValue = datas[0]["FNUMBER"].ToString();
                }
            }
        }
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if (e.BarItemKey.EqualsIgnoreCase("PENY_ImportBOM"))
            {
                // 刷新选中行
                if (this.ListView.SelectedRowsInfo.Count == 0)
                {
                    this.View.ShowMessage("没有选择任何数据，请先选择数据！");
                    return;
                }

                string materialID = this.ListView.SelectedRowsInfo[0].PrimaryKeyValue;
                string materialNumber = this.ListView.SelectedRowsInfo[0].Number;
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.Resizable = false;
                param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
                param.FormId = "PENY_ImportExcel";
                param.CustomComplexParams.Add("MaterialNumber", materialNumber);

                this.View.ShowForm(param, new Action<FormResult>((result) =>
                {
                    if (result.ReturnData != null)
                    {
                        ENGBomInfo resdata = result.ReturnData as ENGBomInfo;
                        this.ListView.Refresh();
                        //var sSql = $@"/*dialect*/UPDATE T_BD_MATERIAL SET FPENYBOMN='{}' WHERE FMATERIALID={materialID}";
                        //DBServiceHelper.Execute(this.Context, sSql);
                    }
                }));

                //this.ListView.RefreshSelectRows(this.ListView.SelectedRowsInfo);
            }
            if (e.BarItemKey.EqualsIgnoreCase("PENY_GetLinkBOM"))
            {
                // 刷新选中行
                if (this.ListView.SelectedRowsInfo.Count == 0)
                {
                    this.View.ShowMessage("没有选择任何数据，请先选择数据！");
                    return;
                }
                var material = this.ListView.SelectedRowsInfo[0].DataRow as DynamicObjectDataRow;

                string materialNumber = material.DynamicObject["FNumber"].ToString();
                long productid = Convert.ToInt64(material.DynamicObject["FProductId"]);
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.Resizable = false;
                param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
                param.FormId = "PENY_GetBom";
                param.CustomComplexParams.Add("MaterialNumber", materialNumber);
                param.CustomComplexParams.Add("MaterialName", material.DynamicObject["FName"].ToString());
                param.CustomComplexParams.Add("ProductId", productid);
                this.View.ShowForm(param);
            }
        }
    }
}
