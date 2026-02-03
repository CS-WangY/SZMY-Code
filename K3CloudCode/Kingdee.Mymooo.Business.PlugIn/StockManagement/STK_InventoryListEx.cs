using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Stock.Business.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    [Description("即时库存明细扩展大小类"), HotUpdate]
    public class STK_InventoryListEx : AbstractListPlugIn
    {
        public override void BeforeGetDataForTempTableAccess(BeforeGetDataForTempTableAccessArgs e)
        {
            base.BeforeGetDataForTempTableAccess(e);

            List<string> strings = new List<string>();
            if (DBServiceHelper.IsExistTableField(this.Context, e.TableName, "fsmallid_id"))
            {
                strings.Add("fsmallid_id=t2.FMATERIALGROUP");
            }
            if (DBServiceHelper.IsExistTableField(this.Context, e.TableName, "fparentsmallid_id"))
            {
                strings.Add("fparentsmallid_id=ISNULL(t3.FPARENTID,0)");
            }
            if (DBServiceHelper.IsExistTableField(this.Context, e.TableName, "fbusinessdivisionid_id"))
            {
                strings.Add("fbusinessdivisionid_id=ISNULL(t4.FBUSINESSDIVISION,0)");
            }
            string setwhere = "";
            if (strings.Count > 0)
            {
                setwhere = "," + string.Join(",", strings);
            }
            string sSql = $@"/*dialect*/UPDATE {e.TableName} 
SET fparentsmallid=0 {setwhere}
--SELECT t1.fmaterialid_id,t2.FMATERIALGROUP,t3.FPARENTID,t4.FBUSINESSDIVISION
FROM {e.TableName} t1
LEFT JOIN dbo.T_BD_MATERIAL t2 ON t1.fmaterialid_id=t2.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIALGROUP t3 ON t2.FMATERIALGROUP=t3.FID
LEFT JOIN T_BD_MasterialGroupTaxCode t4 ON t2.FMATERIALGROUP=t4.FMATERIALGROUP";
            DBServiceHelper.Execute(this.Context, sSql);
        }
    }
}
