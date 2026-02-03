using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.SAL_RETURNSTOCK
{
    [Description("销售退货通知单保存插件"), HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBUSINESSDIVISIONIDS");
            e.FieldKeys.Add("FBUSINESSDIVISIONID");
        }
        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var entitem = item["SAL_RETURNNOTICEENTRY"] as DynamicObjectCollection;
                if (entitem.Count > 0)
                {
                    //更新表头的事业部字段，取明细第一个事业部
                    var BusinessDivisionId = ((DynamicObject)entitem[0])["FBUSINESSDIVISIONID_Id"];
                    string sSql = $@"update T_SAL_RETURNNOTICE set  FBUSINESSDIVISIONIDS='{BusinessDivisionId}' where FId={item["id"]}";
                    DBUtils.Execute(this.Context, sSql);
                }
            }
        }
    }
}
