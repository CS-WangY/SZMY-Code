using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.StkTransferOut
{
    [Description("单据转换,到分步式调出单插件")]
    [HotUpdate]
    public class StkTransferOutConvertPlugIn : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");

            foreach (var headEntity in headEntitys)
            {
                //供应商信息集合
                var supplierDynamic = headEntity["SupplierId"] as DynamicObject;
                var supplierCode = "";
                if (supplierDynamic != null)
                {
                    supplierCode = supplierDynamic["Number"].ToString();
                }
                //获取明细数据
                var detDynamicObject = headEntity["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
                foreach (var item in detDynamicObject)
                {
                    var materialDynamic = item["MaterialID"] as DynamicObject;
                    var materialCode = "";
                    if (materialDynamic != null)
                    {
                        materialCode = materialDynamic["Number"].ToString();
                    }
                    item["F_PENY_QRCode"] = materialCode + "/" + supplierCode;
                }
            }
        }
    }
}
