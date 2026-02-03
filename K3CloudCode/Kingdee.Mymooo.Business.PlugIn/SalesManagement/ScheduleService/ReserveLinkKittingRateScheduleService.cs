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
    public class ReserveLinkKittingRateScheduleService : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            //计算当天预留信息内的数据
            string sSql = $@"/*dialect*/WITH
rl AS (
SELECT SUM(t1.FBASEQTY) AS FBASEQTY,orgl.FNAME AS orgname,org.FNUMBER
,t2.FSRCENTRYID,t1.FMATERIALID,bdm.FMASTERID,MAX(t1.FDATE) AS FDATE
,t1.FSUPPLYORGID
FROM T_PLN_RESERVELINKENTRY t1
INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
LEFT JOIN T_ORG_ORGANIZATIONS org ON t1.FSUPPLYORGID=org.FORGID
LEFT JOIN T_ORG_ORGANIZATIONS_L orgl ON org.FORGID=orgl.FORGID
LEFT JOIN T_STK_INVENTORY stk ON t1.FSUPPLYINTERID=stk.FID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
WHERE t1.FSUPPLYFORMID='STK_Inventory' AND t1.FBASEQTY>0
AND t2.FDEMANDFORMID IN ('SAL_SaleOrder','PLN_REQUIREMENTORDER')
GROUP BY orgl.FNAME,org.FNUMBER,t2.FSRCENTRYID,t1.FMATERIALID,bdm.FMASTERID,t1.FSUPPLYORGID
)
,sal AS (
SELECT t2.FAPPROVEDATE,t2.FOBJECTTYPEID,t2.FBILLNO,t1.*,tr.FREMAINOUTQTY FCANOUTQTY,bdm.FMASTERID AS SalMASTERID,t2.FSALEORGID FROM dbo.T_SAL_ORDERENTRY t1
INNER JOIN dbo.T_SAL_ORDERENTRY_R tr ON t1.FENTRYID=tr.FENTRYID
INNER JOIN dbo.T_SAL_ORDER t2 ON t2.FID = t1.FID
LEFT JOIN dbo.T_BD_MATERIAL bdm ON t1.FMATERIALID=bdm.FMATERIALID
WHERE t2.FDOCUMENTSTATUS='C'
)
SELECT t1.FOBJECTTYPEID,t1.FBILLNO,t1.FSEQ,t1.FENTRYID,t1.FMATERIALID,t1.SalMASTERID,t1.FCANOUTQTY,t1.FSALEORGID,t2.*,t1.FAPPROVEDATE FROM sal t1 INNER JOIN rl t2 ON t1.FENTRYID=t2.FSRCENTRYID
AND t1.SalMASTERID=t2.FMASTERID
ORDER BY t1.FBILLNO,t1.FSEQ";
            var reader = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            if (!(reader is null))
            {
                List<SqlObject> sqlObjects = new List<SqlObject>();
                foreach (var item in reader)
                {
                    var canqty = Convert.ToDecimal(item["FCANOUTQTY"]);
                    var baseqty = Convert.ToDecimal(item["FBASEQTY"]);
                    if (baseqty >= canqty)
                    {
                        var update = Convert.ToDateTime(item["FDATE"]);
                        var salDate = Convert.ToDateTime(item["FAPPROVEDATE"]);
                        if (update == DateTime.Parse("1993-08-08"))
                        {
                            update = salDate;
                        }
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
                            new SqlParam("@FSUPPLYORGID", KDDbType.Int64, Convert.ToInt64(item["FSUPPLYORGID"])),
                            new SqlParam("@FOBJECTFORMID", KDDbType.String, Convert.ToString(item["FOBJECTTYPEID"])),
                            new SqlParam("@FDEMANDDATE", KDDbType.DateTime, salDate),
                            new SqlParam("@FSUPPLYDATE", KDDbType.DateTime, update),
                            new SqlParam("@FCREATEDATETIME",KDDbType.DateTime, DateTime.Now),
                            new SqlParam("@FKITTINGDATE", KDDbType.DateTime, update),
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
                            sSql = $@"/*dialect*/UPDATE dbo.T_SAL_ORDERENTRY SET FPENYKittingRateDatetime='{update}' WHERE FENTRYID={Convert.ToInt64(item["FENTRYID"])}";
                            sqlObjects.Add(new SqlObject(sSql, sqlParams));
                        }

                    }
                }
                DBUtils.ExecuteBatch(ctx, sqlObjects);
            }
            //计算当天关闭的已经出货的齐货时间
            sSql = $@"/*dialect*/SELECT t2.FENTRYID,t3.FSALEORGID,t2.FSUPPLYTARGETORGID as FSUPPLYORGID,t3.FOBJECTTYPEID,t4.FDeliveryDate as FDEMANDDATE
,t2.FPENYCLOSEDATETIME,t5.FAPPROVEDATE
,t3.FBILLNO,t2.FSEQ FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN dbo.T_SAL_ORDER t3 ON t2.FID=t3.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY_D t4 ON t2.FENTRYID=t4.FENTRYID
LEFT JOIN (SELECT t1.FSOENTRYID,MAX(t3.FAPPROVEDATE) FAPPROVEDATE FROM dbo.T_SAL_OUTSTOCKENTRY_R t1
INNER JOIN dbo.T_SAL_OUTSTOCKENTRY t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCK t3 ON t2.FID=t3.FID GROUP BY t1.FSOENTRYID) t5 ON t1.FENTRYID=t5.FSOENTRYID
WHERE t1.FRemainOutQty<=0
AND t2.FPENYCLOSEDATETIME BETWEEN '{DateTime.Now.Date}' AND '{DateTime.Now.Date.AddDays(1)}'";
            reader = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
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
                            new SqlParam("@FSUPPLYORGID", KDDbType.Int64, Convert.ToInt64(item["FSUPPLYORGID"])),
                            new SqlParam("@FOBJECTFORMID", KDDbType.String, Convert.ToString(item["FOBJECTTYPEID"])),
                            new SqlParam("@FDEMANDDATE", KDDbType.DateTime, Convert.ToDateTime(item["FDEMANDDATE"])),
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
