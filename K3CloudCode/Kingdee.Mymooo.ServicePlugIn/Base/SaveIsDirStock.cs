using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.Base
{
    [Description("仓库判断是否直发保存更新插件"), HotUpdate]
    public class SaveIsDirStock : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FIsDirStock");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                var isdir = item["FIsDirStock"].ToString();
                if (isdir == "True")
                {
                    string sSql = $@"update T_BD_STOCK set FISDIRSTOCK=0 where FUSEORGID={this.Context.CurrentOrganizationInfo.ID} and FSTOCKID<>{fid}";
                    DBUtils.Execute(this.Context, sSql);
                }
            }
        }
    }
}
