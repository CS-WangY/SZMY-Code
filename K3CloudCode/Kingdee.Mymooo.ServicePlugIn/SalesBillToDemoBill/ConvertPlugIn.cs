using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBillToDemoBill
{
    public class ConvertPlugIn  : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);

            ExtendedDataEntity[] arrays = e.Result.FindByEntityKey("FBillHead");

            foreach (var array in arrays)
            {

            }
        }
    }
}
