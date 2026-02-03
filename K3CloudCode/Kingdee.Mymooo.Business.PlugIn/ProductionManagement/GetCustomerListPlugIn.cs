using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    [Description("生产用料清单获取客户单据列表插件")]
    public class GetCustomerListPlugIn : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            //生产用料清单获取客户
            if (e.BarItemKey == "PENY_GetCustomer")
            {
                var list = this.ListView.SelectedRowsInfo;
                if (list.Count == 0)
                {
                    this.View.ShowMessage("没有选择生产用料清单");
                }
                foreach (var item in list)
                {
                    List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int32, Convert.ToInt32(item.PrimaryKeyValue)) };
                    var sql = $@"select top 1 so.FCUSTID,pp.FSALEORDERID from T_PRD_PPBOM pp inner join T_SAL_ORDER so on pp.FSALEORDERID=so.FID where pp.FID=@FID";
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());

                    //获取销售订单客户ID
                    var FCUSTID = 0;
                    //获取销售订单ID
                    var FSALEORDERID = 0;
                    foreach (var d in datas)
                    {
                        FCUSTID = Convert.ToInt32(d["FCUSTID"]);
                        FSALEORDERID = Convert.ToInt32(d["FSALEORDERID"]);
                    }

                    //更新生产用料清单客户ID
                    pars = new List<SqlParam>() {
                            new SqlParam("@FCUSTOMERID", KDDbType.Int32, FCUSTID),
                            new SqlParam("@FSALEORDERID", KDDbType.Int32, FSALEORDERID),
                        };
                    var updateSql = $@"update T_PRD_PPBOM set FCUSTOMERID=@FCUSTOMERID where FSALEORDERID=@FSALEORDERID";
                    DBServiceHelper.Execute(this.Context, updateSql, pars);
                }

                this.View.ShowMessage("获取客户完成");
                this.View.Refresh();
            }

        }
    }
}
