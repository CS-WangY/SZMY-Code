using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    [Description("采购申请插件"), HotUpdate]
    public class RequisitionOrderEdit : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            //默认携带该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单），需要根据组织过滤
            if (e.Field.Key.EqualsIgnoreCase("FMaterialId"))
            {
                long materialId = Convert.ToInt64(e.NewValue);
                var supplyOrgId = Convert.ToInt64(((DynamicObject)this.View.Model.GetValue("FPurchaseOrgId", e.Row))["id"]);
                //华东五部的不携带
                if (supplyOrgId == 7401803)
                {
                    this.View.Model.SetValue("FSuggestSupplierId", 0, e.Row);
                }
                else
                {
                    var supplierId = GetSupplierId(materialId);
                    this.View.Model.SetValue("FSuggestSupplierId", supplierId, e.Row);
                }

            }
        }

        /// <summary>
        /// 该物料最近一笔采购订单的供应商（一年之内，已审核的采购订单）
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        private long GetSupplierId(long materialId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MaterialId", KDDbType.Int64, materialId) };
            var sql = $@"/*dialect*/select top 1 FSupplierId from T_PUR_POORDER t1
                        inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
                        where t1.FDOCUMENTSTATUS='C' and t1.FDATE>=dateadd(year,-1,getdate()) and FMATERIALID=@MaterialId
                        order by t1.FDATE desc ";
            return DBServiceHelper.ExecuteScalar<long>(this.Context, sql, 0, paramList: pars.ToArray());
        }
    }
}
