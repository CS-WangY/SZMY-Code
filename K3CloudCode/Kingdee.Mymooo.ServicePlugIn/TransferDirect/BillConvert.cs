using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.TransferDirect
{
    [Description("直接调拨单单据转换携带货主"), HotUpdate]
    public class BillConvert : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            foreach (var headEntity in headEntitys)
            {
                //var ss = e.SourceBusinessInfo.GetField("ddd");
            }
        }
    }
}
