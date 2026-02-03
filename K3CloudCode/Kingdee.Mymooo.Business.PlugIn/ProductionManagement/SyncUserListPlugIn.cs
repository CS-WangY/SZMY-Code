using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    [Description("员工同步Mes单据列表插件")]
    public class SyncUserListPlugIn : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            //员工同步Mes
            if (e.BarItemKey == "PENY_SyncUser")
            {
                var list = this.ListView.SelectedRowsInfo;
                if (list.Count == 0)
                {
                    this.View.ShowMessage("没有选择同步Mes的员工");
                }
                var listSync = new List<UserSyncToMesEntity>();
                foreach (var item in list)
                {
                    DynamicObjectDataRow dynamicObject = item.DataRow as DynamicObjectDataRow;

                    var entity = new UserSyncToMesEntity
                    {
                        username = Convert.ToString(item.Name),
                        realname = Convert.ToString(item.Name),
                        workNo = Convert.ToString(item.Number),
                        sex = Convert.ToInt32("1"),
                        email = Convert.ToString(dynamicObject["FEmail"].ToString()),
                    };
                    listSync.Add(entity);
                }

                var package = new
                {
                    action = "mymoooErpToMesForUser",
                    id = Guid.NewGuid(),
                    data = listSync
                };

                var timestamp = WebApiSignature.CreateTimestamp();
                var nonce = "b80b9c1148434c8fb975185238a7965c";
                var signature = WebApiSignature.Sign(timestamp, nonce, "");
                var msg = CommonApiRequest.WorkbenchSignatureInvokeWebService(timestamp, signature, WebUIConfigHelper.WorkbenchForMqMesUrl, JsonConvertUtils.SerializeObject(package), "post");

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    var result = JsonConvertUtils.DeserializeObject<MessageHelpMes>(msg);
                    if (result.IsSuccess)
                    {
                        //销售订单明细表增加一个派产的状态（未派产，派产中，已派产）
                        //更新表字段状态值
                        //this.View.ShowMessage("同步人员信息到mes成功");
                        this.View.ShowMessage("员工同步给Mes完成");
                        this.View.Refresh();
                    }
                    else
                    {

                        var err = new Exception($"/mymoooErpToMesForUser 同步人员错误：“{result.errorMessage}”");
                        //LogHelper.Exception(err);
                        this.View.ShowMessage($"/mymoooErpToMesForUser 同步人员错误：“{result.errorMessage}”");
                    }
                }
                
            }

            //this.View.ShowMessage("员工同步Mes菜单点击");
        }
    }
}
