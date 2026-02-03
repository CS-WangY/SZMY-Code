using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.K3.FIN.Core.Object.Acctg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PurReqBill
{
    [Description("采购申请订单下推采购订单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ConvertPurReqToPoOrder : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");

            foreach (var headEntity in headEntitys)
            {
                //获取明细数据
                var detDynamicObject = headEntity["POOrderEntry"] as DynamicObjectCollection;
                foreach (var item in detDynamicObject)
                {
                    var materialDynamic = item["MaterialID"] as DynamicObject;
                    long materialId = 0;
                    if (materialDynamic != null)
                    {
                        materialId = Convert.ToInt64(materialDynamic["id"]);
                        //历史最高单价
                        item["FHighestTaxPrice"] = GetHighestTaxPrice(this.Context, materialId);
                        //历史最低单价
                        item["FLowestTaxPrice"] = GetLowestTaxPrice(this.Context, materialId);
                        //上次下单单价
                        item["FLastTaxPrice"] = GetLastTaxPrice(this.Context, materialId);
                    }
                    if (!string.IsNullOrWhiteSpace(Convert.ToString(item["FPENYDELIVERYDATE"])) && !Convert.ToString(item["FPENYDELIVERYDATE"]).Contains("0001-01-01"))
                    {
                        item["FPENYDELIVERYDATE"] = DateTime.Parse(Convert.ToString(item["FPENYDELIVERYDATE"])).ToString("yyyy-MM-dd"); ;
                    }
                    else
                    {
                        item["FPENYDELIVERYDATE"] = null;
                    }
                }
            }
        }
        //历史最高单价
        private decimal GetHighestTaxPrice(Context ctx, long materialID)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MaterialID", KDDbType.Int64, materialID) };
            var sql = $@"select top 1 t3.FTAXPRICE from T_PUR_POORDER t1
                        inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
                        inner join T_PUR_POORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        where t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS='A' and t2.FMATERIALID=@MaterialID and t1.FAPPROVEDATE>=dateadd(year,-1,getdate()) 
                        order by t3.FTAXPRICE desc ";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }
        //历史最低单价
        private decimal GetLowestTaxPrice(Context ctx, long materialID)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MaterialID", KDDbType.Int64, materialID) };
            var sql = $@"select top 1 t3.FTAXPRICE from T_PUR_POORDER t1
                        inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
                        inner join T_PUR_POORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        where t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS='A' and t2.FMATERIALID=@MaterialID and t1.FAPPROVEDATE>=dateadd(year,-1,getdate()) and t3.FTAXPRICE>0
                        order by t3.FTAXPRICE asc ";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }

        //上次下单单价
        private decimal GetLastTaxPrice(Context ctx, long materialID)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@MaterialID", KDDbType.Int64, materialID) };
            var sql = $@"select top 1 t3.FTAXPRICE from T_PUR_POORDER t1
                        inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
                        inner join T_PUR_POORDERENTRY_F t3 on t2.FENTRYID=t3.FENTRYID
                        where t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS='A' and t2.FMATERIALID=@MaterialID and t1.FAPPROVEDATE>=dateadd(year,-1,getdate()) and t3.FTAXPRICE>0
                        order by t1.FAPPROVEDATE desc ";
            return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
