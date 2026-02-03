using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.PayApplyToPayBill
{
    /// <summary>
    /// 付款申请单下推付款单
    /// </summary>
    [Description("付款申请单下推付款单插件"), HotUpdate]
    public class PayApplyToPayBillConvertPlugIn : AbstractConvertPlugIn
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);

            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                var supper = headEntity.DataEntity["CONTACTUNIT"] as DynamicObject;
                if (supper != null)
                {
                    foreach (var entry in headEntity.DataEntity["PAYBILLENTRY"] as DynamicObjectCollection)
                    {
                        var purpose = entry["PURPOSEID"] as DynamicObject;
                        entry["COMMENT"] = string.Format("{0} {1} 货款", purpose["Name"], supper["Name"]);
                    }
                }
            }
        }
    }
}
