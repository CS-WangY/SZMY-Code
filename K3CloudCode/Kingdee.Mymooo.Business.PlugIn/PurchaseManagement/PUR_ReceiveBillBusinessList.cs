using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    [Description("收料通知单打印更新状态列表插件"), HotUpdate]
    public class PUR_ReceiveBillBusinessList : AbstractListPlugIn
    {
        public override void OnAfterPrint(AfterPrintEventArgs e)
        {
            base.OnAfterPrint(e);
            if (e.NoteIDPairs.Count > 0)
            {
                string[] ids = this.ListView.SelectedRowsInfo.GetEntryPrimaryKeyValues();
                string sSql = "";
                foreach (string eid in ids)
                {
                    sSql = $"UPDATE T_PUR_RECEIVEENTRY SET FPENYPRINTSTART=1 WHERE FENTRYID={eid}";
                    DBServiceHelper.Execute(this.Context, sSql);
                }
            }

        }
    }
}
