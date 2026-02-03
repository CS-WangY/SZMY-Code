using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Web;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using Context = Kingdee.BOS.Context;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement.ScheduleService
{
    /// <summary>
    /// 销售预留齐套信息更新计划
    /// </summary>
    public class HistoryReserveLinkKittingRateScheduleService : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            //计算历史订单关闭且没有预留日期的记录
            var sSql = $@"/*dialect*/SELECT t2.FENTRYID,t3.FSALEORGID,t2.FSUPPLYTARGETORGID,t3.FOBJECTTYPEID,t4.FDeliveryDate
,t2.FPENYCLOSEDATETIME,t3.FBILLNO,t2.FSEQ,t5.FAPPROVEDATE
FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN dbo.T_SAL_ORDER t3 ON t2.FID=t3.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY_D t4 ON t2.FENTRYID=t4.FENTRYID
LEFT JOIN (SELECT t1.FSOENTRYID,MAX(t3.FAPPROVEDATE) FAPPROVEDATE FROM dbo.T_SAL_OUTSTOCKENTRY_R t1
INNER JOIN dbo.T_SAL_OUTSTOCKENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCK t3 ON t2.FID=t3.FID GROUP BY t1.FSOENTRYID) t5 ON t1.FENTRYID=t5.FSOENTRYID
WHERE t1.FREMAINOUTQTY=0 AND t3.FBUSINESSTYPE='NORMAL' AND t2.FPENYKITTINGRATEDATETIME IS NULL
ORDER BY t3.FDATE,t3.FBILLNO,t2.FSEQ";
            var reader = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            if (!(reader is null))
            {
                List<SqlObject> sqlObjects = new List<SqlObject>();
                foreach (var item in reader)
                {

                    var sql = $@"INSERT INTO T_ReserveLinkKittingRate
                               (FID,FUSERID,FSALORGID,FSUPPLYORGID,FOBJECTFORMID,FDEMANDDATE,FSUPPLYDATE,
                                FCREATEDATETIME,FKITTINGDATE,FDESCRIPTION,FBILLNO,FSALBILLNO,FSALBILLSEQ)
                                VALUES
                                (@FID,@FUSERID,@FSALORGID,@FSUPPLYORGID,@FOBJECTFORMID,@FDEMANDDATE,@FSUPPLYDATE,
                                @FCREATEDATETIME,@FKITTINGDATE,@FDESCRIPTION,@FBILLNO,@FSALBILLNO,@FSALBILLSEQ)";
                    List<SqlParam> sqlParams = new List<SqlParam>()
                        {
                            new SqlParam("@FID", KDDbType.Int64, Convert.ToInt64(item["FENTRYID"])),
                            //new SqlParam("@FFormId", KDDbType.Int64, messageIds.Data[index]),
                            new SqlParam("@FUSERID", KDDbType.Int64, ctx.UserId),
                            new SqlParam("@FSALORGID", KDDbType.Int64, Convert.ToInt64(item["FSALEORGID"])),
                            new SqlParam("@FSUPPLYORGID", KDDbType.Int64, Convert.ToInt64(item["FSUPPLYTARGETORGID"])),
                            new SqlParam("@FOBJECTFORMID", KDDbType.String, Convert.ToString(item["FOBJECTTYPEID"])),
                            new SqlParam("@FDEMANDDATE", KDDbType.DateTime, Convert.ToDateTime(item["FDeliveryDate"])),
                            new SqlParam("@FSUPPLYDATE", KDDbType.DateTime, Convert.ToDateTime(item["FAPPROVEDATE"])),
                            new SqlParam("@FCREATEDATETIME",KDDbType.DateTime, DateTime.Now),
                            new SqlParam("@FKITTINGDATE", KDDbType.DateTime, Convert.ToDateTime(item["FAPPROVEDATE"])),
                            new SqlParam("@FDESCRIPTION", KDDbType.String, ""),
                            new SqlParam("@FBILLNO", KDDbType.String,Convert.ToString(item["FBILLNO"])+"-"+Convert.ToString(item["FSEQ"])),
                            new SqlParam("@FSALBILLNO", KDDbType.String, Convert.ToString(item["FBILLNO"])),
                            new SqlParam("@FSALBILLSEQ", KDDbType.Int32, Convert.ToInt32(item["FSEQ"])),
                        };
                    sSql = $@"SELECT * FROM T_ReserveLinkKittingRate WHERE FID={Convert.ToInt64(item["FENTRYID"])}";
                    var datas = DBUtils.ExecuteDynamicObject(ctx, sSql);
                    if (datas.Count <= 0)
                    {
                        sqlObjects.Add(new SqlObject(sql, sqlParams));
                        sSql = $@"/*dialect*/UPDATE dbo.T_SAL_ORDERENTRY SET FPENYKittingRateDatetime='{Convert.ToDateTime(item["FAPPROVEDATE"])}' WHERE FENTRYID={Convert.ToInt64(item["FENTRYID"])}";
                        sqlObjects.Add(new SqlObject(sSql, sqlParams));
                    }
                }
                DBUtils.ExecuteBatch(ctx, sqlObjects);
            }
        }
    }
}
