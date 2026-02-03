using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Warn.PlugIn;
using Kingdee.BOS.Core.Warn.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.Mymooo.Core.BaseManagement;

namespace Kingdee.Mymooo.ServicePlugIn.WarnService
{
    [Description("预警-预留信息-消息解析插件"), HotUpdate]
    public class PLN_RESERVELINK_WarnService : AbstractWarnMessagePlugIn
    {
        /// <summary>
        /// 处理消息
        /// 可以自定义
        /// </summary> 
        public override void ProcessWarnMessage(ProcessWarnMessageEventArgs e)
        {
            //string text = EarlyWarningServiceHelper.ProcessWarnMessage(((AbstractWarnMessagePlugIn)this).Context, e.MsgDataKeyValueList, "1");
            if (e.MsgDataKeyValueList != null && e.MsgDataKeyValueList.Count() > 0)
            {
                e.IsProcessByPlugin = true;

                var opResult = e.Result as OperateResult;
                //插件中显示了提示消息，公共的提示消息就不显示了
                e.Result.IsShowMessage = false;
                //标识已被插件处理,阻断默认的消息处理
                e.IsProcessByPlugin = true;
                var operateResultLst = new OperateResultCollection();
                if (e.MsgDataKeyValueList != null)
                {
                    List<string> billNos = new List<string>();
                    foreach (var msgKeyValue in e.MsgDataKeyValueList)
                    {
                        operateResultLst.Add(new OperateResult()
                        {
                            PKValue = msgKeyValue.MessageId,
                        });
                        billNos.Add("'" + msgKeyValue.First.Value + "'");
                    }
                    ListShowParameter listShowParameter = new ListShowParameter();
                    listShowParameter.FormId = "PLN_REQUIREMENTORDER";
                    //listShowParameter.Status = OperationStatus.EDIT;
                    listShowParameter.OpenStyle.ShowType = ShowType.Modal;
                    // 增加过滤条件
                    listShowParameter.ListFilterParameter.Filter = $" FBillNo in({string.Join(",", billNos)})";
                    //listShowParameter.IsShowQuickFilter = true;
                    //listShowParameter.IsIsolationOrg = true;
                    //listShowParameter.IsShowFilter = true;
                    //listShowParameter.IsShowUsed = true;
                    this.ParentView.ShowForm(listShowParameter, new Action<FormResult>((result) =>
                    {
                        e.Result.OperateResult = operateResultLst;
                        //界面提示已处理
                        this.ParentView.ShowMessage("消息已处理");
                    }));
                }
            }

            base.ProcessWarnMessage(e);
        }
    }
}
