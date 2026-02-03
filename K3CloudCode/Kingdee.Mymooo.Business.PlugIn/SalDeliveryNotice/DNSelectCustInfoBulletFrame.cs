using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.Mymooo.Core.PurchaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
namespace Kingdee.Mymooo.Business.PlugIn.SalDeliveryNotice
{
    [Description("发货通知单选择客户收件信息插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class DNSelectCustInfoBulletFrame : AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 初始化绑定
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var receiverID = this.View.OpenParameter.GetCustomParameter("ReceiverID");
            string sql = $@"select FNUMBER,FRECEIVER,FMOBILE,FADDRESS from T_BD_CUSTLOCATION  where FCUSTID={receiverID} and FIsUsed=1 ";
            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
            foreach (var item in datas)
            {
                int rowcount = this.Model.GetEntryRowCount("FEntity");
                if (rowcount > 0)
                {
                    this.Model.CreateNewEntryRow("FEntity");
                }
                this.Model.SetValue("FADDRESS", Convert.ToString(item["FADDRESS"]), rowcount);
                this.Model.SetValue("FRECEIVER", Convert.ToString(item["FRECEIVER"]), rowcount);
                this.Model.SetValue("FMOBILE", Convert.ToString(item["FMOBILE"]), rowcount);
                this.Model.SetValue("FNUMBER", Convert.ToString(item["FNUMBER"]), rowcount);
            }
            this.View.UpdateView("FEntity");
        }

        /// <summary>
        /// 双击事件
        /// </summary>
        /// <param name="e"></param>
        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntity");
            DynamicObject selectedEntityObj = (this.View.Model.DataObject["FEntity"] as DynamicObjectCollection)[rowIndex];
            this.View.ReturnToParentWindow(selectedEntityObj);
            this.View.Close();
        }
    }
}
