using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.PayableToPayApply
{
    /// <summary>
    /// 付款申请单下推付款单
    /// </summary>
    [Description("应付单下推付款申请单"), HotUpdate]
    public class PayableToPayApplyConvertPlugIn : AbstractConvertPlugIn
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            FormMetadata meta = MetaDataServiceHelper.Load(this.Context, "CN_PAYAPPLY") as FormMetadata;
            var settFiled = meta.BusinessInfo.GetField("FSETTLETYPEID") as BaseDataField;
            var settleType = BusinessDataServiceHelper.LoadSingle(this.Context, 231285, settFiled.RefFormDynamicObjectType);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                foreach (var entry in headEntity.DataEntity["FPAYAPPLYENTRY"] as DynamicObjectCollection)
                {
                    var settlety = entry["FSETTLETYPEID"] as DynamicObject;
                    if (settlety == null)
                    {
                        entry["FSETTLETYPEID"] = settleType;
                        entry["FSETTLETYPEID_Id"] = 231285;
                    }
                }
            }
        }
    }
}
