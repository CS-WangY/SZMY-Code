using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.StockManagement
{
	/// <summary>
	/// 库存查询服务
	/// </summary>
	public class StockQuantityService : IStockQuantityService
	{
		public ResponseMessage<dynamic> PurPoInventory(Context ctx, PurchaseProductQuantityRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				var pars = new List<SqlParam>();
				List<PurchaseProductQuantity> list = new List<PurchaseProductQuantity>();
				string param = string.Empty;
				//var AddSql = " where org.FNUMBER='SZMYGC' ";
				var AddSql = " where 1=1 ";
				var CompanyId = request.CompanyId;
				var PurchaseDate = request.PurchaseDate;
				if (!CompanyId.IsNullOrEmpty())
				{
					AddSql = AddSql + $" and org.FNUMBER='{CompanyId}'";
				}
				foreach (var item in request.ProductModel)
				{
					var sql = $@"SELECT tb.FPURCHASEORGID,org.FNUMBER,orgl.FNAME ProductModel,t3.FNUMBER,t4.FNAME,SUM(t1.FQTY) TotalPurchaseQuantity
,SUM(t2.FSTOCKINQTY) AS LastInventoryQuantity,RK.FDATE as LastInventoryDateTime FROM T_PUR_POORDERENTRY t1
LEFT JOIN T_PUR_POORDERENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
LEFT JOIN T_PUR_POORDER tb ON t1.FID=tb.FID
LEFT JOIN T_BD_MATERIAL t3 ON t1.FMATERIALID=t3.FMATERIALID
LEFT JOIN T_BD_MATERIAL_L t4 ON t3.FMATERIALID=t4.FMATERIALID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS org ON tb.FPURCHASEORGID=org.FORGID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L orgl ON org.FORGID=orgl.FORGID
LEFT JOIN (
SELECT MAX(t2.FDATE) AS FDATE,t1.FMATERIALID,t2.FPURCHASEORGID FROM T_STK_INSTOCKENTRY t1
INNER JOIN T_STK_INSTOCK t2 ON t1.FID=t2.FID
WHERE t2.FDOCUMENTSTATUS='C'
GROUP BY t1.FMATERIALID,t2.FPURCHASEORGID
) RK ON t1.FMATERIALID=RK.FMATERIALID AND tb.FPURCHASEORGID=RK.FPURCHASEORGID
{AddSql} and t3.FNUMBER = '{item}'
GROUP BY tb.FPURCHASEORGID,t1.FMATERIALID,t3.FNUMBER,t4.FNAME,RK.FDATE,org.FNUMBER,orgl.FNAME";
					var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
					foreach (var aditem in datas)
					{
						list.Add(JsonConvertUtils.DeserializeObject<PurchaseProductQuantity>(JsonConvertUtils.SerializeObject(aditem)));
					}
				}
				response.Data = list;
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return response;
		}
		public List<StockPlatEntity> StockPlatformAction(Context ctx, List<KeyValue> itemNos)
		{

			var pars = new List<SqlParam>();
			string param = string.Empty;
			//var AddSql = " where org.FNUMBER='SZMYGC' ";
			List<StockPlatEntity> list = new List<StockPlatEntity>();
			foreach (var item in itemNos)
			{
				var AddSql = " where 1=1 ";
				AddSql += $" and m.FNUMBER = '{item.Key}' ";
				AddSql += $" and org.FNUMBER = '{item.Value}' ";


				var sql = $@"select
	                        *
                        from
	                        (
		                        select
			                        m.FMASTERID as FMATERIALID,
									org.FORGID as FORGID,
									org.FNUMBER as ORGNUM,
									orgl.FNAME as ORGNAME,
                                    sto.FSTOCKID as STOID,
                                    sto.FNUMBER as STONUM,
									stol.FNAME as STONAME,
			                        m.FNUMBER as MATERIALNUM,
			                        ml.FNAME as MATERIALNAME, --物料
									u.FNUMBER as UNITNUM,
									ul.FNAME as UNITNAME,
									sto.FISOUTSOURCESTOCK,
                                    sto.FOUTSOURCESTOCKLOC
			                        ,SUM(t1.FBASEQTY) FBASEQTY --库存量
			                        ,SUM(t1.FAVBQTY) FAVBQTY --可用库存
			                        ,SUM(t1.FLOCKQTY) FLOCKQTY --预留量
                                    ,ISNULL(SUM(t1.FAVBQTY),0)+ISNULL(SUM(t2.FBASELOCKQTY),0) FAVBQTYL --可用库存+预留量
			                        ,SUM(ISNULL(NC.FREMAINOUTQTY,0)) UNQTYSHIPDSUM--待出库量
			                        ,SUM(ISNULL(CR.FREMAINSTOCKINQTY,0)) ONORDERQTY--采购在途
			                        ,SUM(ISNULL(PJ.FINSTOCKJOINQTY,0)) QTYINSP--品检数量
		                        from
			                        V_STK_INVENTORY_CUS t1
			                        inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID = org.FORGID
			                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID
			                        inner join T_BD_STOCK sto on t1.FSTOCKID = sto.FSTOCKID
			                        inner join T_BD_STOCK_L stol on sto.FSTOCKID = stol.FSTOCKID
			                        inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID = stos.FSTOCKSTATUSID
			                        inner join T_BD_MATERIAL m on t1.FMATERIALID = m.FMASTERID
			                        and t1.FSTOCKORGID = m.FUSEORGID
			                        inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID
			                        inner join T_BD_UNIT u on t1.FBASEUNITID = u.FUNITID
			                        inner join T_BD_UNIT_L ul on t1.FBASEUNITID = ul.FUNITID
			                        LEFT JOIN (
				                        select
					                        SUM(FREMAINOUTQTY) FREMAINOUTQTY,
					                        t3.FSALEORGID,
					                        t2.FMATERIALID
				                        from
					                        T_SAL_ORDERENTRY_R t1
					                        inner join T_SAL_ORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_SAL_ORDER t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C' AND t3.FCLOSESTATUS<>'B'
				                        group by
					                        FSALEORGID,
					                        t2.FMATERIALID
			                        ) NC ON NC.FSALEORGID = org.FORGID
			                        and NC.FMATERIALID = m.FMATERIALID
			                        LEFT JOIN (
				                        select
					                        SUM(FREMAINSTOCKINQTY) FREMAINSTOCKINQTY,
					                        t3.FPURCHASEORGID,
					                        t2.FMATERIALID
				                        from
					                        T_PUR_POORDERENTRY_R t1
					                        inner join T_PUR_POORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_PUR_POORDER t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C'
				                        group by
					                        t3.FPURCHASEORGID,
					                        t2.FMATERIALID
			                        ) CR ON CR.FPURCHASEORGID = org.FORGID
			                        and CR.FMATERIALID = m.FMATERIALID
			                        LEFT JOIN (
				                        select
					                        SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKJOINQTY) FINSTOCKJOINQTY,
					                        t3.FSTOCKORGID,
					                        t2.FMATERIALID
				                        from
					                        T_PUR_RECEIVEENTRY_S t1
					                        inner join T_PUR_ReceiveEntry t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_PUR_Receive t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C'
					                        and t2.FCHECKINCOMING = 1
				                        group by
					                        t3.FSTOCKORGID,
					                        t2.FMATERIALID
			                        ) PJ ON PJ.FSTOCKORGID = org.FORGID
			                        and PJ.FMATERIALID = m.FMATERIALID
                                LEFT JOIN 
						            (
						            SELECT  TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID,
						            SUM(ISNULL(TKE.FBASEQTY, 0)) AS FBASELOCKQTY
						            FROM    T_PLN_RESERVELINKENTRY TKE
						            left join T_PLN_RESERVELINK TK ON TKE.FID=TK.FID
						            WHERE   TK.FRESERVETYPE=1
						            AND TKE.FBASEQTY > 0
						            GROUP BY TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID
						            ) t2 on m.FMATERIALID=t2.FMATERIALID and t1.FSTOCKID=t2.FSTOCKID and t1.FSTOCKORGID=t2.FSUPPLYORGID
                                    {AddSql}
		                        group by
			                        m.FMASTERID,
									org.FORGID,
									org.FNUMBER,
									orgl.FNAME,
			                        m.FNUMBER,
			                        ml.FNAME,
									u.FNUMBER,
									ul.FNAME,
									sto.FISOUTSOURCESTOCK,
                                    sto.FOUTSOURCESTOCKLOC,
                                    sto.FSTOCKID,
                                    sto.FNUMBER,
                                    stol.FNAME
	                        ) datas";
				var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
				foreach (var aditem in datas)
				{
					list.Add(JsonConvertUtils.DeserializeObject<StockPlatEntity>(JsonConvertUtils.SerializeObject(aditem)));
				}
			}
			return list;
		}

		/// <summary>
		/// 获取即时库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemNos">物料编号</param>
		/// <param name="itemIds">物料金蝶ID</param>
		/// <param name="orgId">0是获取深圳蚂蚁，-1是全部，>0是根据对应的组织</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public List<StockQuantityEntity> StockQuantityAction(Context ctx, List<string> itemNos, List<long> itemIds, long orgId = 0)
		{
			List<StockQuantityEntity> list = new List<StockQuantityEntity>();
			if (itemNos == null)
			{
				itemNos = new List<string>();
			}
			if (itemIds == null)
			{
				itemIds = new List<long>();
			}
			if (itemNos.Count == 0 && itemIds.Count == 0)
			{
				throw new Exception("物料编号集合或者物料ID集合必须存在有数据");
			}
			var pars = new List<SqlParam>();
			string param = string.Empty;
			var AddSql = " where stos.FAvailable='1' ";
			if (orgId == 0)
			{
				AddSql += " and org.FNUMBER='SZMYGC' ";
			}
			else if (orgId > 0)
			{
				pars.Add(new SqlParam("@FORGID", KDDbType.Int64, orgId));
				AddSql += " and org.FORGID=@FORGID ";
			}
			if (itemNos.Count > 0)
			{
				int i = 1;
				foreach (var item in itemNos)
				{
					if (i == 1)
						param = "@FNUMBER" + i;
					else
						param += ",@FNUMBER" + i;
					pars.Add(new SqlParam("@FNUMBER" + i++, KDDbType.String, item));
				}
				AddSql += $" and m.FNUMBER in ({param}) ";
			}
			if (itemIds.Count > 0)
			{
				int i = 1;
				foreach (var item in itemIds)
				{
					if (i == 1)
						param = "@MaterialID" + i;
					else
						param += ",@MaterialID" + i;
					pars.Add(new SqlParam("@MaterialID" + i++, KDDbType.Int64, item));
				}
				AddSql += $" and t1.FMATERIALID in ({param}) ";

			}
			var sql = $@"select * from (
                    select m.FMATERIALID,org.FORGID,org.FNUMBER as OrgNum,orgl.FNAME as OrgName--组织
                      ,sto.FSTOCKID StoID,sto.FNUMBER as StoNum,stol.FNAME as StoName--仓库
                      ,m.FNUMBER as MaterialNum,ml.FNAME as MaterialName--物料
                      ,Sum(t1.FBASEQTY) FBASEQTY--库存量
                      ,SUM(t1.FAVBQTY) FAVBQTY --可用库存
					  ,ISNULL(SUM(t1.FAVBQTY),0) FAVBQTYL --可用库存
                      ,SUM(t1.FLOCKQTY) FLOCKQTY--预留量
                      ,u.FNUMBER as UnitNum,ul.FNAME as UnitName--单位
					  ,sto.FISOUTSOURCESTOCK --是否外发仓库
                      from V_STK_INVENTORY_CUS t1
                      inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID=org.FORGID
                      inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID
                      inner join T_BD_STOCK sto on t1.FSTOCKID=sto.FSTOCKID
                      inner join T_BD_STOCK_L stol on sto.FSTOCKID=stol.FSTOCKID
					  inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID=stos.FSTOCKSTATUSID
                      inner join T_BD_MATERIAL m on t1.FMATERIALID=m.FMATERIALID
                      inner join T_BD_MATERIAL_L ml on m.FMATERIALID=ml.FMATERIALID
                      inner join T_BD_UNIT u on t1.FBASEUNITID=u.FUNITID
                      inner join T_BD_UNIT_L ul on t1.FBASEUNITID=ul.FUNITID
                       {AddSql}
					   group by m.FMATERIALID,org.FORGID,org.FNUMBER,orgl.FNAME
                      ,sto.FSTOCKID,sto.FNUMBER,stol.FNAME
                      ,m.FNUMBER,ml.FNAME
                      ,u.FNUMBER,ul.FNAME
					  ,sto.FISOUTSOURCESTOCK
						) t1
						order by FISOUTSOURCESTOCK desc";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			foreach (var data in datas)
			{
				list.Add(new StockQuantityEntity
				{
					IsOutSourceStock = Convert.ToInt32(data["FISOUTSOURCESTOCK"]) == 1 ? true : false,
					OrgId = Convert.ToInt64(data["FORGID"].ToString()),
					OrgNumber = data["OrgNum"].ToString(),
					StockID = Convert.ToInt64(data["StoID"]),
					StockNumber = data["StoNum"].ToString(),
					ItemID = Convert.ToInt64(data["FMATERIALID"]),
					ItemNo = data["MaterialNum"].ToString(),
					Quantity = Convert.ToDecimal(data["FBASEQTY"]),
					QtyInsp = 0,
					UnQtyShipdSum = 0,//Convert.ToInt32(data["FBASEAVBQTY"]);
					OnOrderQty = 0,//Convert.ToInt32(data["FBASEAVBQTY"]);
					UsableQty = Convert.ToDecimal(data["FAVBQTYL"]),
				});
			}
			return list;
		}

		/// <summary>
		/// 获取即时库存(蚂蚁平台专用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemNos">物料编号</param>
		/// <param name="itemIds">物料金蝶ID</param>
		/// <param name="orgId">0是获取深圳蚂蚁，-1是全部，>0是根据对应的组织</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public List<StockPlatEntity> StockQuantityActionV2(Context ctx, List<string> itemNos)
		{
			//List<StockQuantityEntity> list = new List<StockQuantityEntity>();
			if (itemNos == null)
			{
				itemNos = new List<string>();
			}

			var pars = new List<SqlParam>();
			string param = string.Empty;
			//var AddSql = " where org.FNUMBER='SZMYGC' ";
			var AddSql = " where 1=1 ";

			if (itemNos.Count > 0)
			{
				int i = 1;
				foreach (var item in itemNos)
				{
					if (i == 1)
						param = "@FNUMBER" + i;
					else
						param += ",@FNUMBER" + i;
					pars.Add(new SqlParam("@FNUMBER" + i++, KDDbType.String, item));
				}
				AddSql += $" and m.FNUMBER in ({param}) ";
			}

			var sql = $@"select
	                        *
                        from
	                        (
		                        select
			                        m.FMASTERID as FMATERIALID,
									org.FORGID as FORGID,
									org.FNUMBER as ORGNUM,
									orgl.FNAME as ORGNAME,
									sto.FSTOCKID as STOID,
									sto.FNUMBER as STONUM,
									stol.FNAME as STONAME,
			                        m.FNUMBER as MATERIALNUM,
			                        ml.FNAME as MATERIALNAME, --物料
									u.FNUMBER as UNITNUM,
									ul.FNAME as UNITNAME,
									sto.FISOUTSOURCESTOCK,
                                    sto.FOUTSOURCESTOCKLOC
			                        ,SUM(t1.FBASEQTY) FBASEQTY --库存量
			                        ,SUM(t1.FAVBQTY) FAVBQTY --可用库存
			                        ,SUM(t1.FLOCKQTY) FLOCKQTY --预留量
                                    ,ISNULL(SUM(t1.FAVBQTY),0)+ISNULL(SUM(t2.FBASELOCKQTY),0) FAVBQTYL --可用库存+预留量
			                        ,SUM(ISNULL(NC.FREMAINOUTQTY,0)) UNQTYSHIPDSUM--待出库量
			                        ,SUM(ISNULL(CR.FREMAINSTOCKINQTY,0)) ONORDERQTY--采购在途
			                        ,SUM(ISNULL(PJ.FINSTOCKJOINQTY,0)) QTYINSP--品检数量
		                        from
			                        V_STK_INVENTORY_CUS t1
			                        inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID = org.FORGID
			                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID
			                        inner join T_BD_STOCK sto on t1.FSTOCKID = sto.FSTOCKID
			                        inner join T_BD_STOCK_L stol on sto.FSTOCKID = stol.FSTOCKID
			                        inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID = stos.FSTOCKSTATUSID
			                        inner join T_BD_MATERIAL m on t1.FMATERIALID = m.FMASTERID
			                        and t1.FSTOCKORGID = m.FUSEORGID
			                        inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID
			                        inner join T_BD_UNIT u on t1.FBASEUNITID = u.FUNITID
			                        inner join T_BD_UNIT_L ul on t1.FBASEUNITID = ul.FUNITID
			                        LEFT JOIN (
				                        select
					                        SUM(FREMAINOUTQTY) FREMAINOUTQTY,
					                        t3.FSALEORGID,
					                        t2.FMATERIALID
				                        from
					                        T_SAL_ORDERENTRY_R t1
					                        inner join T_SAL_ORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_SAL_ORDER t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C' AND t3.FCLOSESTATUS<>'B'
				                        group by
					                        FSALEORGID,
					                        t2.FMATERIALID
			                        ) NC ON NC.FSALEORGID = org.FORGID
			                        and NC.FMATERIALID = m.FMATERIALID
			                        LEFT JOIN (
				                        select
					                        SUM(FREMAINSTOCKINQTY) FREMAINSTOCKINQTY,
					                        t3.FPURCHASEORGID,
					                        t2.FMATERIALID
				                        from
					                        T_PUR_POORDERENTRY_R t1
					                        inner join T_PUR_POORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_PUR_POORDER t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C'
				                        group by
					                        t3.FPURCHASEORGID,
					                        t2.FMATERIALID
			                        ) CR ON CR.FPURCHASEORGID = org.FORGID
			                        and CR.FMATERIALID = m.FMATERIALID
			                        LEFT JOIN (
				                        select
					                        SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKJOINQTY) FINSTOCKJOINQTY,
					                        t3.FSTOCKORGID,
					                        t2.FMATERIALID
				                        from
					                        T_PUR_RECEIVEENTRY_S t1
					                        inner join T_PUR_ReceiveEntry t2 on t1.FENTRYID = t2.FENTRYID
					                        inner join T_PUR_Receive t3 on t1.FID = t3.FID
				                        where
					                        t3.FDOCUMENTSTATUS = 'C'
					                        and t2.FCHECKINCOMING = 1
				                        group by
					                        t3.FSTOCKORGID,
					                        t2.FMATERIALID
			                        ) PJ ON PJ.FSTOCKORGID = org.FORGID
			                        and PJ.FMATERIALID = m.FMATERIALID
                                LEFT JOIN 
						            (
						            SELECT  TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID,
						            SUM(ISNULL(TKE.FBASEQTY, 0)) AS FBASELOCKQTY
						            FROM    T_PLN_RESERVELINKENTRY TKE
						            left join T_PLN_RESERVELINK TK ON TKE.FID=TK.FID
						            WHERE   TK.FRESERVETYPE=1
						            AND TKE.FBASEQTY > 0
						            GROUP BY TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID
						            ) t2 on m.FMATERIALID=t2.FMATERIALID and t1.FSTOCKID=t2.FSTOCKID and t1.FSTOCKORGID=t2.FSUPPLYORGID
                                    {AddSql}
		                        group by
			                        m.FMASTERID,
									org.FORGID,
									org.FNUMBER,
									orgl.FNAME,
									sto.FSTOCKID,
									sto.FNUMBER,
									stol.FNAME,
			                        m.FNUMBER,
			                        ml.FNAME,
									u.FNUMBER,
									ul.FNAME,
									sto.FISOUTSOURCESTOCK,
                                    sto.FOUTSOURCESTOCKLOC
	                        ) datas";
			//var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			List<StockPlatEntity> list = JsonConvertUtils.DeserializeObject<List<StockPlatEntity>>(JsonConvertUtils.SerializeObject(DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray())));
			return list;
			//foreach (var data in datas)
			//{
			//    list.Add(new StockQuantityEntity
			//    {

			//        ItemNo = data["MaterialNum"].ToString(),
			//        Quantity = Convert.ToDecimal(data["FBASEQTY"]),
			//        QtyInsp = Convert.ToDecimal(data["FINSTOCKJOINQTY"]),
			//        UnQtyShipdSum = Convert.ToDecimal(data["FREMAINOUTQTY"]),
			//        OnOrderQty = Convert.ToDecimal(data["FREMAINSTOCKINQTY"]),
			//        UsableQty = Convert.ToDecimal(data["FAVBQTYL"]),
			//    });
			//}
			//return list;
		}

		/// <summary>
		/// 获取物料即时库存数
		/// </summary>
		/// <param name="ctx">上下文</param>
		/// <param name="masterID">传物料的masterID</param>
		/// <param name="orgid">组织集合，取销售订单销售组织与供货组织</param>
		/// <returns></returns>
		public DynamicObjectCollection InventoryQty(Context ctx, long masterID, List<long> orgid)
		{
			string sSql = $@"SELECT t1.FID,M.FMATERIALID,t1.FSTOCKORGID,t1.FSTOCKID,t1.FBASEUNITID,
                            ISNULL(t1.FAVBQTY, 0) as FBASEQTY,t3.FISOUTSOURCESTOCK,t3.FISDIRSTOCK FROM V_STK_INVENTORY_CUS t1
                            LEFT JOIN T_BD_MATERIAL M on t1.FMATERIALID=M.FMASTERID
                            LEFT JOIN T_BD_STOCK t3 on t1.FSTOCKID=t3.FSTOCKID
                            WHERE M.FMATERIALID={masterID} and FSTOCKORGID in ({"'" + String.Join("','", orgid) + "'"}) 
                            AND t3.FISDIRSTOCK=0 AND FNOTALLOWDELIVERY=0
                            ORDER BY t3.FISOUTSOURCESTOCK,t3.FNUMBER DESC";
			return DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
		}

		/// <summary>
		/// 获取物料可用库存(包含不良品)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="orgid"></param>
		/// <param name="masterID"></param>
		/// <returns></returns>
		public List<InventoryQtyV2Entity> InventoryQtyVStatus(Context ctx, long orgid, List<long> masterID)
		{
			string sSql = $@"SELECT M.FMATERIALID FMasterID,t1.FSTOCKORGID,t1.FSTOCKID,t1.FBASEUNITID,
                            ISNULL(SUM(t1.FAVBQTY), 0) as FAVBQTY,t3.FISOUTSOURCESTOCK,t3.FISDIRSTOCK FROM V_STK_INVENTORY_Status t1
                            LEFT JOIN T_BD_MATERIAL M on t1.FMATERIALID=M.FMASTERID
                            LEFT JOIN T_BD_STOCK t3 on t1.FSTOCKID=t3.FSTOCKID
                            WHERE M.FMATERIALID in ({"'" + String.Join("','", masterID) + "'"}) and FSTOCKORGID={orgid} 
GROUP BY M.FMATERIALID,t3.FNUMBER,t1.FSTOCKORGID,t1.FSTOCKID,t1.FBASEUNITID,t3.FISOUTSOURCESTOCK,t3.FISDIRSTOCK
                            ORDER BY t3.FISOUTSOURCESTOCK,t3.FNUMBER DESC";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);

			List<InventoryQtyV2Entity> list = new List<InventoryQtyV2Entity>();
			foreach (var data in datas)
			{
				list.Add(new InventoryQtyV2Entity
				{
					OrgId = Convert.ToInt64(data["FSTOCKORGID"].ToString()),
					StockID = Convert.ToInt64(data["FSTOCKID"]),
					ItemMasterID = Convert.ToInt64(data["FMasterID"]),
					Qty = Convert.ToDecimal(data["FAVBQTY"]),
				});
			}
			return list;
		}


		/// <summary>
		/// 云仓储根据供货组织、云存储仓库编号获取物料总库存量
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="supplyOrgCode">供货组织编码</param>
		/// <param name="cloudStockCode">仓库编码</param>
		/// <param name="itemNos">物料编码</param>
		/// <returns></returns>
		public List<CloudStockBaseQtyEntity> CloudStockBaseQty(Context ctx, CloudStockBaseQtyRequest request)
		{
			List<CloudStockBaseQtyEntity> list = new List<CloudStockBaseQtyEntity>();
			if (request.ItemNos == null)
			{
				request.ItemNos = new List<string>();
			}
			var pars = new List<SqlParam>();
			string param = string.Empty;
			var AddSql = " ";
			pars.Add(new SqlParam("@OrgCode", KDDbType.String, request.SupplyOrgCode));
			AddSql += " AND t2.FNUMBER=@OrgCode AND t4.FNUMBER=@OrgCode ";
			pars.Add(new SqlParam("@CloudStockCode", KDDbType.String, request.CloudStockCode));
			AddSql += " AND t3.FCLOUDSTOCKCODE=@CloudStockCode  ";
			if (request.ItemNos.Count > 0)
			{
				int i = 1;
				foreach (var item in request.ItemNos)
				{
					if (i == 1)
						param = "@ItemNo" + i;
					else
						param += ",@ItemNo" + i;
					pars.Add(new SqlParam("@ItemNo" + i++, KDDbType.String, item));
				}
				AddSql += $" AND M.FNUMBER in ({param}) ";
			}

			string sSql = $@"/*dialect*/SELECT t2.FNUMBER SupplyOrgCode,t3.FCloudStockCode CloudStockCode,M.FNUMBER ItemNo,SUM(t1.FBASEQTY) BASEQTY
							FROM V_STK_INVENTORY_CUS t1
                            LEFT JOIN T_BD_MATERIAL M on t1.FMATERIALID=M.FMASTERID
							LEFT JOIN T_ORG_ORGANIZATIONS t2 on t2.FORGID=t1.FSTOCKORGID
                            LEFT JOIN T_BD_STOCK t3 on t1.FSTOCKID=t3.FSTOCKID
							LEFT JOIN T_ORG_ORGANIZATIONS t4 on t4.FORGID=M.FUSEORGID
							WHERE ISNULL(t1.FBASEQTY, 0)>0 AND t3.FISDIRSTOCK=0 {AddSql}
                            GROUP BY t2.FNUMBER,t3.FCloudStockCode,M.FNUMBER ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				list.Add(new CloudStockBaseQtyEntity
				{
					SupplyOrgCode = data["SupplyOrgCode"].ToString(),
					CloudStockCode = data["CloudStockCode"].ToString(),
					ItemNo = data["ItemNo"].ToString(),
					Qty = Convert.ToDecimal(data["BaseQty"])
				});
			}
			return list;
		}

		/// <summary>
		/// 获取物料相关组织下属于直发仓即时库存数
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemMasterID"></param>
		/// <param name="orgid"></param>
		/// <returns></returns>
		public DynamicObjectCollection InventoryDirQty(Context ctx, long itemMasterID, List<long> orgid)
		{
			string sSql = $@"/*dialect*/SELECT t1.FID,M.FMATERIALID,t1.FSTOCKORGID,t1.FSTOCKID,t1.FBASEUNITID,
                            ISNULL(t1.FAVBQTY, 0) as FBASEQTY,t3.FISOUTSOURCESTOCK,t3.FISDIRSTOCK FROM V_STK_INVENTORY_CUS t1
                            LEFT JOIN T_BD_MATERIAL M on t1.FMATERIALID=M.FMASTERID
                            LEFT JOIN T_BD_STOCK t3 on t1.FSTOCKID=t3.FSTOCKID
                            WHERE M.FMATERIALID={itemMasterID} and FSTOCKORGID in ({"'" + String.Join("','", orgid) + "'"}) AND t3.FISDIRSTOCK=1
                            ORDER BY t3.FISOUTSOURCESTOCK,t3.FNUMBER DESC";
			return DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
		}


		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public List<MesFuzzyQueryStockBaseQtyEntity> GetMesFuzzyQueryStockBaseQty(Context ctx, FuzzyQueryStockBaseQtyRequest request)
		{

			var pars = new List<SqlParam>();
			string param = string.Empty;
			var AddSql = " where org.FORGID=7401803 and t1.FQty>0  ";
			if (!string.IsNullOrWhiteSpace(request.Textures))
			{
				pars.Add(new SqlParam("@Textures", KDDbType.String, request.Textures));
				AddSql += " and wlcz.FNUMBER=@Textures ";
			}
			if (!string.IsNullOrWhiteSpace(request.MaterialType))
			{
				pars.Add(new SqlParam("@MaterialType", KDDbType.String, request.MaterialType));
				AddSql += " and wlcx.FNUMBER=@MaterialType ";
			}
			if (request.StartLength != 0)
			{
				pars.Add(new SqlParam("@StartLength", KDDbType.Decimal, request.StartLength));
				AddSql += " and mb.FLENGTH>=@StartLength ";
			}
			if (request.EndLength != 0)
			{
				pars.Add(new SqlParam("@EndLength", KDDbType.String, request.EndLength));
				AddSql += " and mb.FLENGTH<=@EndLength ";
			}
			if (request.StartWidth != 0)
			{
				pars.Add(new SqlParam("@StartWidth", KDDbType.Decimal, request.StartWidth));
				AddSql += " and mb.FWIDTH>=@StartWidth ";
			}
			if (request.EndWidth != 0)
			{
				pars.Add(new SqlParam("@EndWidth", KDDbType.String, request.EndWidth));
				AddSql += " and mb.FWIDTH<=@EndWidth ";
			}
			if (request.StartHeight != 0)
			{
				pars.Add(new SqlParam("@StartHeight", KDDbType.Decimal, request.StartHeight));
				AddSql += " and mb.FHEIGHT>=@StartHeight ";
			}
			if (request.EndHeight != 0)
			{
				pars.Add(new SqlParam("@EndHeight", KDDbType.String, request.EndHeight));
				AddSql += " and mb.FHEIGHT<=@EndHeight ";
			}
			var sql = $@"/*dialect*/select
			                        m.FMASTERID as FMATERIALID,
									org.FORGID as FORGID,
									org.FNUMBER as ORGNUM,
									orgl.FNAME as ORGNAME,
									sto.FSTOCKID as STOID,
									sto.FNUMBER as STONUM,
									stol.FNAME as STONAME,
			                        m.FNUMBER as MATERIALNUM,
			                        ml.FNAME as MATERIALNAME, --物料
									u.FNUMBER as UNITNUM,
									ul.FNAME as UNITNAME,
			                        SUM(t1.FQty) FBASEQTY, --库存量(库存量库存主单位)
									wlcz.FNUMBER as TexturesCode,
									wlczl.FDATAVALUE as TexturesName,
									wlcx.FNUMBER as MaterialTypeCode,
									wlcxl.FDATAVALUE as MaterialTypeName,
									mb.FLENGTH as Length,
									mb.FWIDTH as Width,
									mb.FHEIGHT as Height,
									ml.FSpecification as Specification,
									pg.FNUMBER ParentSmallCode,pgl.FNAME ParentSmallName,
									g.FNUMBER SmallCode,gl.FNAME SmallName
		                        from
			                        V_STK_INVENTORY_CUS t1
			                        inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID = org.FORGID
			                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID = orgl.FORGID
			                        inner join T_BD_STOCK sto on t1.FSTOCKID = sto.FSTOCKID
			                        inner join T_BD_STOCK_L stol on sto.FSTOCKID = stol.FSTOCKID
			                        inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID = stos.FSTOCKSTATUSID
			                        inner join T_BD_MATERIAL m on t1.FMATERIALID = m.FMASTERID
			                        and t1.FSTOCKORGID = m.FUSEORGID
									inner join t_BD_MaterialBase mb on m.FMATERIALID=mb.FMATERIALID
			                        inner join T_BD_MATERIAL_L ml on m.FMATERIALID = ml.FMATERIALID
			                        inner join T_BD_UNIT u on t1.FStockUnitId = u.FUNITID
			                        inner join T_BD_UNIT_L ul on t1.FStockUnitId = ul.FUNITID
									left join T_BAS_ASSISTANTDATAENTRY wlcz on wlcz.FENTRYID=m.FAssistantTextures --材质
									left join T_BAS_ASSISTANTDATAENTRY_L wlczl on wlcz.FENTRYID=wlczl.FENTRYID and wlczl.FLOCALEID=2052
									left join T_BAS_ASSISTANTDATAENTRY wlcx on wlcx.FENTRYID=m.FAssistantMaterialType --材型
									left join T_BAS_ASSISTANTDATAENTRY_L wlcxl on wlcx.FENTRYID=wlcxl.FENTRYID and wlcxl.FLOCALEID=2052
									left join T_BD_MATERIALGROUP g on m.FMATERIALGROUP = g.FID 
									left join T_BD_MATERIALGROUP_L gl on g.FID = gl.FID and gl.FLOCALEID = 2052
									left join T_BD_MATERIALGROUP pg on g.FPARENTID = pg.FID 
									left join T_BD_MATERIALGROUP_L pgl on pg.FID = pgl.FID and pgl.FLOCALEID = 2052
								{AddSql}
		                        group by
			                        m.FMASTERID,
									org.FORGID,
									org.FNUMBER,
									orgl.FNAME,
									sto.FSTOCKID,
									sto.FNUMBER,
									stol.FNAME,
			                        m.FNUMBER,
			                        ml.FNAME,
									u.FNUMBER,
									ul.FNAME,
									wlcz.FNUMBER,
									wlczl.FDATAVALUE,
									wlcx.FNUMBER,
									wlcxl.FDATAVALUE,
									mb.FLENGTH,
									mb.FWIDTH,
									mb.FHEIGHT,
									ml.FSpecification,
									pg.FNUMBER,pgl.FNAME,
									g.FNUMBER,gl.FNAME ";
			List<MesFuzzyQueryStockBaseQtyEntity> list = JsonConvertUtils.DeserializeObject<List<MesFuzzyQueryStockBaseQtyEntity>>(JsonConvertUtils.SerializeObject(DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray())));
			return list;
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemNos">物料编号</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public List<MesStockPlatEntity> MesStockQuantityAction(Context ctx, List<string> itemNos)
		{
			if (itemNos == null)
			{
				itemNos = new List<string>();
			}

			var pars = new List<SqlParam>();
			string param = string.Empty;
			var AddSql = " where org.FORGID=7401803 ";

			if (itemNos.Count > 0)
			{
				int i = 1;
				foreach (var item in itemNos)
				{
					if (i == 1)
						param = "@FNUMBER" + i;
					else
						param += ",@FNUMBER" + i;
					pars.Add(new SqlParam("@FNUMBER" + i++, KDDbType.String, item));
				}
				AddSql += $" and m.FNUMBER in ({param}) ";
			}

			var sql = $@"/*dialect*/select 
			                        m.FMASTERID as FMATERIALID,
									org.FORGID as FORGID,
									org.FNUMBER as ORGNUM,
									sto.FSTOCKID as STOID,
									sto.FNUMBER as STONUM,
			                        m.FNUMBER as MATERIALNUM,
									u.FNUMBER as UNITNUM,--(库存主单位)
			                        SUM(t1.FQty) FBASEQTY --库存量(库存量库存主单位)
		                        from
			                        V_STK_INVENTORY_CUS t1
			                        inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID = org.FORGID
			                        inner join T_BD_STOCK sto on t1.FSTOCKID = sto.FSTOCKID
			                        inner join T_BD_MATERIAL m on t1.FMATERIALID = m.FMASTERID
			                        and t1.FSTOCKORGID = m.FUSEORGID
			                        left join T_BD_UNIT u on t1.FStockUnitId = u.FUNITID
                                    {AddSql}
		                        group by
			                        m.FMASTERID,
									org.FORGID,
									org.FNUMBER,
									sto.FSTOCKID,
									sto.FNUMBER,
			                        m.FNUMBER,
									u.FNUMBER";
			List<MesStockPlatEntity> list = JsonConvertUtils.DeserializeObject<List<MesStockPlatEntity>>(JsonConvertUtils.SerializeObject(DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray())));
			return list;

		}
	}
}


