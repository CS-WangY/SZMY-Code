using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM.RPM;
using Kingdee.Mymooo.Core.BomManagement;
using Newtonsoft.Json.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.TransferOutManagement
{
    [Description("分步式调拨单-列表超链接插件"), HotUpdate]
    public class StkTransferList : AbstractListPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);
            var rowData = this.ListView.CurrentPageRowsInfo.FirstOrDefault(row => row.RowKey == e.Row);
            if (rowData == null)
            {
                return;
            }
            var billno = Convert.ToString(rowData.DataRow[e.FieldKey]);
            string _formid = "";
            switch (e.FieldKey)
            {
                case string x when x.EqualsIgnoreCase("FPENYDeliveryNotice"):
                    _formid = "SAL_DELIVERYNOTICE";
                    break;
                case string x when x.EqualsIgnoreCase("FPENYSalOrderNo"):
                    _formid = "SAL_SaleOrder";
                    break;
                case string x when x.EqualsIgnoreCase("FPENYSalReturnNo"):
                    _formid = "SAL_RETURNSTOCK";
                    break;
                default:
                    return;
            }
            
            if (billno != "")
            {
                var requisitionMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.Context, _formid);
                var objs = BusinessDataServiceHelper.Load(this.Context, requisitionMetadata.BusinessInfo, 
                    new List<SelectorItemInfo>(new[] { new SelectorItemInfo("FID") }), 
                    OQLFilter.CreateHeadEntityFilter($"FBillNo='{billno}'"));
                if (objs == null || objs.Length == 0)
                {
                    return;
                }

                var pkId = objs[0]["Id"].ToString();
                var showParameter = new BillShowParameter
                {
                    FormId = _formid,//业务对象标识
                    PKey = pkId,
                    Status = OperationStatus.VIEW,//查看模式打开(EDIT编辑模式)
                };
                this.View.ShowForm(showParameter);
            }
        }

    }
}
