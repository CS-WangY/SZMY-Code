using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
    [Description("组织间需求单插件")]
    [HotUpdate]
    public class RequirementOrderEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FMaterialId") || e.Field.Key.EqualsIgnoreCase("FSupplyOrgId"))
            {
                var demandOrgId = this.View.Model.GetValue<long>("FDemandOrgId", 0, 0);
                var supplyOrgId = this.View.Model.GetValue<long>("FSupplyOrgId", 0, 0);
                var material = this.View.Model.GetValue("FMaterialId") as DynamicObject;
                if (supplyOrgId > 0 && material != null)
                {
                    var sql = @"select e.FTAXPRICE 
from T_IOS_PRICELIST o
	inner join T_IOS_PRICELISTENTRY e on o.FID = e.FID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
where o.FCREATEORGID=@FCREATEORGID and o.F_PENY_SupplyOrgId = @FSupplyOrgId  and m.FMASTERID = @FMASTERID ";

                    var price = DBServiceHelper.ExecuteScalar<decimal>(this.View.Context, sql, 0,
                        new SqlParam("@FCREATEORGID", KDDbType.Int64, demandOrgId),
                        new SqlParam("@FSupplyOrgId", KDDbType.Int64, supplyOrgId),
                        new SqlParam("@FMASTERID", KDDbType.Int64, material["MsterId"]));
                    this.View.Model.SetValue("F_PENY_Price", price);
                }
                else
                {
                    this.View.Model.SetValue("F_PENY_Price", 0);
                }
            }
        }
    }
}
