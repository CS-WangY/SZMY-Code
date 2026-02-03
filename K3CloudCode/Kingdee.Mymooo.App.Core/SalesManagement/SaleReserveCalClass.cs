using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;

namespace Kingdee.Mymooo.App.Core.SalesManagement
{
    /// <summary>
    /// 计算销售订单预留关系类
    /// </summary>
    public class SaleReserveCalClass
    {
        /// <summary>
        /// 取销售订单预留下的组织间需求单信息
        /// </summary>
        public void GetSaleRequirementorder(Context ctx, long billid, long rowid)
        {
            var sSql = $@"SELECT t1.FSUPPLYINTERID,t1.FSUPPLYORGID,t1.FBASEQTY,t1.FSTOCKID FROM T_PLN_RESERVELINKENTRY t1
                LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                WHERE t2.FDEMANDINTERID={billid} AND t2.FDEMANDENTRYID={rowid} AND t1.FSUPPLYFORMID='PLN_REQUIREMENTORDER'";
            var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
        }
    }
}
