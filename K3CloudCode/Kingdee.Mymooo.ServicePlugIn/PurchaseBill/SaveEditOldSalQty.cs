using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单保存更新采购订单原销售数量")]
	[Kingdee.BOS.Util.HotUpdate]
	public class SaveEditOldSalQty : AbstractOperationServicePlugIn
	{
		/// <summary>
		/// 事务外  操作后
		/// </summary>
		/// <param name="e"></param>
		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			foreach (var item in e.DataEntitys)
			{
				var fid = Convert.ToInt64(item["Id"]);
				var sql = $@"/*dialect*/update t1 set FOldSalQty=t2.FQTY from T_PUR_POORDERENTRY t1,
                        (
                        select FENTRYID,SUM(FQTY) FQTY from (
                        select t1.FENTRYID,t2.FDEMANDBILLNO,t2.FDEMANDBILLENTRYSEQ
                        from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                        where isnull(t2.FDEMANDBILLNO,'')<>'' and t1.FID ={fid}
                        union
                        select t1.FENTRYID,t5.FSALEORDERNO,t5.FSALEORDERENTRYSEQ
                         from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                        left join T_PUR_REQENTRY_LK t3 on t3.FENTRYID=t2.FSID
                        left join T_PLN_PLANORDER_LK t4 on t3.FSID=t4.FID
                        left JOIN T_PLN_PLANORDER_B t5 ON t4.FSBILLID=t5.FID
                        where isnull(t5.FSALEORDERNO,'')<>''  and t1.FID ={fid}
                        ) tt1
                        inner join (
                        select case when t1.FDEMANDTYPE='0' then  t1.FBILLNO else t1.FSALEORDERNO end FBILLNO,
                        case when t1.FDEMANDTYPE='0' then  0 else t1.FSALEORDERENTRYSEQ end FSEQ,
                        case when t1.FDEMANDTYPE='0' then  t1.FFirmQty else t1.FDEMANDQTY end FQTY
                        from T_PLN_REQUIREMENTORDER  t1 where FDEMANDTYPE in ('0','8')  and FDOCUMENTSTATUS='C'
                        union 
                        select  t1.FBILLNO,t2.FSEQ,FQTY from T_SAL_ORDER t1
                        inner join T_SAL_ORDERENTRY t2 on t1.FID =t2.FID
                        inner join T_SAL_ORDERENTRY_D t3 on t2.FENTRYID=t3.FENTRYID
                        where  t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS='A'
                        ) tt2 on tt1.FDEMANDBILLNO=tt2.FBILLNO and tt1.FDEMANDBILLENTRYSEQ=tt2.FSEQ
                        group by FENTRYID
                        ) t2 where t1.FENTRYID=t2.FENTRYID";
				DBUtils.Execute(this.Context, sql);

				//保存更新销售单价含税(销售单价等于0，取采购订单日期最近的一笔销售订单的含税单价(按平台审核日期))
				sql = $@"/*dialect*/update t1 set t1.FSOUNITPRICE=isnull((select top 1 tt4.FTAXPRICE from T_SAL_ORDER tt1
                            inner join T_SAL_ORDERENTRY tt2 on tt1.FID=tt2.FID
                            inner join 	T_BD_MATERIAL tt3  on tt2.FMATERIALID=tt3.FMATERIALID
                            inner join T_SAL_ORDERENTRY_F tt4 on tt2.FENTRYID=tt4.FENTRYID
                            where tt1.FDOCUMENTSTATUS='C' and tt3.FNUMBER=t3.FNUMBER 
                            and tt2.FSupplyTargetOrgId=t2.FPURCHASEORGID and t2.FDATE>tt1.FAUDITTIME order by tt1.FAUDITTIME desc
                            ),0) from T_PUR_POORDERENTRY t1,T_PUR_POORDER t2,T_BD_MATERIAL t3 
                            where  t1.FID={fid} and t1.FSOUNITPRICE=0 and t1.FID=t2.FID and t1.FMATERIALID=t3.FMATERIALID ";
				DBUtils.Execute(this.Context, sql);

				//更新 毛利率%  = ( 销售单价(含税)  -  含税单价  ) / 销售单价(含税)  * 100
				 sql = $@"/*dialect*/update t1 set t1.FVatProfitRate=case when t1.FSoUnitPrice=0  then 0 
                                         else convert(decimal(18,2),(t1.FSoUnitPrice-t2.FTaxPrice)/t1.FSoUnitPrice*100) end 
                                         from t_PUR_POOrderEntry t1,T_PUR_POORDERENTRY_F t2 where t1.FID={fid} and t1.FENTRYID=t2.FENTRYID ";
				DBUtils.Execute(this.Context, sql);
			}
		}
	}
}
