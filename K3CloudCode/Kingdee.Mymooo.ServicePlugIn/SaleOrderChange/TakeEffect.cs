using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.SaleOrderChange
{
	[Description("销售订单新变更单生效操作"), HotUpdate]
    public class TakeEffect : AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<SqlObject> sqlObjects = new List<SqlObject>();

            //发货通知单
            var deliveNotSql = @"/*dialect*/
with salesorder as
(
	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
	from T_BF_INSTANCEENTRY s
		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
	union all
	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
)
update de
set FSMALLID = e.FSmallId,FPARENTSMALLID = e.FParentSmallId,FCUSTMATID = e.FMAPID,FProjectNo = e.FProjectNo,FCustMaterialNo=e.FCustMaterialNo,FStockFeatures=e.FStockFeatures
,FLocFactory=e.FLocFactory,FBUSINESSDIVISIONID=e.FBUSINESSDIVISIONID,FCUSTPURCHASENO=o.FCUSTPURCHASENO,FCustItemNo=e.FCustItemNo,FCustItemName=e.FCustItemName
from salesorder s
	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	inner join T_SAL_ORDER o on e.FID = o.FID
	inner join T_SAL_DELIVERYNOTICEENTRY de on s.FTID = de.FENTRYID
where s.FTTABLENAME = @FTTABLENAME";
            //销售出库单
            var outStockSql = @"/*dialect*/
with salesorder as
(
	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
	from T_BF_INSTANCEENTRY s
		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
	union all
	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
)
update de
set FSMALLID = e.FSmallId,FPARENTSMALLID = e.FParentSmallId,FCUSTMATID = e.FMAPID,FPENY_PROJECTNO = e.FProjectNo,FCustMaterialNo=e.FCustMaterialNo,FStockFeatures=e.FStockFeatures
,FLocFactory=e.FLocFactory,FBUSINESSDIVISIONID=e.FBUSINESSDIVISIONID,FCUSTPURCHASENO=o.FCUSTPURCHASENO,FCustItemNo=e.FCustItemNo,FCustItemName=e.FCustItemName
from salesorder s
	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	inner join T_SAL_ORDER o on e.FID = o.FID
	inner join T_SAL_OUTSTOCKENTRY de on s.FTID = de.FENTRYID
where s.FTTABLENAME = @FTTABLENAME";

            //退货通知单
            var returnNoteSql = @"/*dialect*/
with salesorder as
(
	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
	from T_BF_INSTANCEENTRY s
		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
	union all
	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
)
update de
set FSMALLID = e.FSmallId,FPARENTSMALLID = e.FParentSmallId,FMAPID = e.FMAPID,FProjectNo = e.FProjectNo,FCustMaterialNo=e.FCustMaterialNo,FStockFeatures=e.FStockFeatures
,FLocFactory=e.FLocFactory,FBUSINESSDIVISIONID=e.FBUSINESSDIVISIONID,FCUSTPURCHASENO=o.FCUSTPURCHASENO,FCustItemNo=e.FCustItemNo,FCustItemName=e.FCustItemName
from salesorder s
	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	inner join T_SAL_ORDER o on e.FID = o.FID
	inner join T_SAL_RETURNNOTICEENTRY de on s.FTID = de.FENTRYID
where s.FTTABLENAME = @FTTABLENAME";
            //销售退货单
            var returnStockSql = @"/*dialect*/
with salesorder as
(
	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
	from T_BF_INSTANCEENTRY s
		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
	union all
	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
)
update de
set FSMALLID = e.FSmallId,FPARENTSMALLID = e.FParentSmallId,FMAPID = e.FMAPID,FPENY_PROJECTNO = e.FProjectNo,FCustMaterialNo=e.FCustMaterialNo,FStockFeatures=e.FStockFeatures
,FLocFactory=e.FLocFactory,FBUSINESSDIVISIONID=e.FBUSINESSDIVISIONID,FCUSTPURCHASENO=o.FCUSTPURCHASENO,FCustItemNo=e.FCustItemNo,FCustItemName=e.FCustItemName
from salesorder s
	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	inner join T_SAL_ORDER o on e.FID = o.FID
	inner join T_SAL_RETURNSTOCKENTRY de on s.FTID = de.FENTRYID
where s.FTTABLENAME = @FTTABLENAME";
            //应收单
            var receivableSql = @"/*dialect*/
with salesorder as
(
	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
	from T_BF_INSTANCEENTRY s
		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
	union all
	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
	from salesorder s
		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
)
update de
set FCUSTMATID = e.FMAPID,FProjectNo = e.FProjectNo,FCUSTPURCHASENO=o.FCUSTPURCHASENO,FCustMaterialNo=e.FCustMaterialNo,FCustItemNo=e.FCustItemNo,FCustItemName=e.FCustItemName
from salesorder s
	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
	inner join T_SAL_ORDER o on e.FID = o.FID
	inner join t_AR_receivableEntry de on s.FTID = de.FENTRYID
where s.FTTABLENAME = @FTTABLENAME";

            foreach (var data in e.DataEntitys)
            {
                //变更发货通知单
                sqlObjects.Add(new SqlObject(deliveNotSql, new List<SqlParam>()
                {
                    new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, data["PKIDX"]),
                    new SqlParam("@FTTABLENAME", KDDbType.String, "T_SAL_DELIVERYNOTICEENTRY")
                }));
                //变更销售出库单
                sqlObjects.Add(new SqlObject(outStockSql, new List<SqlParam>()
                {
                    new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, data["PKIDX"]),
                    new SqlParam("@FTTABLENAME", KDDbType.String, "T_SAL_OUTSTOCKENTRY")
                }));
                //变更退货通知单
                sqlObjects.Add(new SqlObject(returnNoteSql, new List<SqlParam>()
                {
                    new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, data["PKIDX"]),
                    new SqlParam("@FTTABLENAME", KDDbType.String, "T_SAL_RETURNNOTICEENTRY")
                }));
                //变更销售退货单
                sqlObjects.Add(new SqlObject(returnStockSql, new List<SqlParam>()
                {
                    new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, data["PKIDX"]),
                    new SqlParam("@FTTABLENAME", KDDbType.String, "T_SAL_RETURNSTOCKENTRY")
                }));
                //变更应收单
                sqlObjects.Add(new SqlObject(receivableSql, new List<SqlParam>()
                {
                    new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, data["PKIDX"]),
                    new SqlParam("@FTTABLENAME", KDDbType.String, "t_AR_receivableEntry")
                }));
            }

            DBUtils.ExecuteBatch(this.Context, sqlObjects);
        }
    }
}
