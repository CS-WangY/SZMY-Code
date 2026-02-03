using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Sal.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单执行汇总表扩展字段插件"), HotUpdate]
    public class SalesCollectRpt: SalCollectRpt
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();

            this.IsCreateTempTableByPlugin = true;

        }
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            var sql = $@" ALTER TABLE {tableName} ADD FParentSmallName nvarchar(50);
                          ALTER TABLE {tableName} ADD FSmallName nvarchar(50);
                          ALTER TABLE {tableName} ADD FSupplyTargetOrgName nvarchar(50); ";
            DBUtils.Execute(this.Context, sql);

            //更新大类，小类，供货组织
            sql = $@"/*dialect*/update dt1 set dt1.FParentSmallName=dt2.FParentSmallName,dt1.FSmallName=dt2.FSmallName,dt1.FSupplyTargetOrgName=dt2.FSupplyTargetOrgName from {tableName} dt1,
                    (select t1.fid,FORDERID,gll.FNAME FParentSmallName,gl.FNAME FSmallName,t3.FNAME FSupplyTargetOrgName from {tableName} t1
                    inner join T_SAL_ORDERENTRY t2 on t1.fid=t2.FID and t1.FORDERID=t2.FENTRYID
                    inner join T_ORG_ORGANIZATIONS_L t3 on t3.FORGID=t2.FSupplyTargetOrgId and t3.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP_L gl on t2.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP_L gll on t2.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052) dt2 where dt1.FORDERID=dt2.FORDERID and dt1.FID=dt2.FID ";
            DBUtils.Execute(this.Context, sql);
        }
    }
}
