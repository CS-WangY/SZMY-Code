using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.PLN.App.MrpModel;
using Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc;
using static Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc.AbstractNetCalcPolicy;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel
{
    [Description("MRP_DP_MTL_BR"), HotUpdate]
    public class MaterialScopeBomHDFI : AbstractMrpDataPolicy
    {
        ///
        /// 策略执行前事件[MRP_DP_MTL_BR]
        ///
        protected override bool BeforeExecuteDataPolicy()
        {
            if (Convert.ToString(this.MrpGlobalDataContext.SchemaData["Number"]) == "华东五部1")
            {
                //将193	2052	非标-华东小板类零件物料修改为外购类型
                var sSql = @"/*dialect*/UPDATE T_MRP_MATERIALDATA1 SET FERPCLSID=1
FROM T_MRP_MATERIALDATA1 t1
LEFT JOIN dbo.T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIALGROUP t3 ON t2.FMATERIALGROUP=t3.FID
WHERE t3.FIsSendMES=1";
                DBUtils.Execute(this.Context, sSql);
                //获取参与运算的物料信息
                //foreach (var dsmaterial in this.MrpGlobalDataContext.DSMaterialItems)
                //{
                //    dsmaterial.Value["ErpClsId"] = 1;
                //}
            }
            return base.BeforeExecuteDataPolicy();
        }
    }
}
