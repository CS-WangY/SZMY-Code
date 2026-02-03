using mymooo.core;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.StockModel;

namespace mymooo.k3cloud.business.Services.Stock
{
    /// <summary>
    /// 仓库服务
    /// </summary>
    /// <param name="kingdeeContent"></param>
    [AutoInject(InJectType.Scope)]
	public class StockService(KingdeeContent kingdeeContent)
	{
		private readonly KingdeeContent _kingdeeContent = kingdeeContent;

		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <returns></returns>
		public ResponseMessage<List<MesFuzzyQueryStockBaseQtyEntity>> GetMesFuzzyQueryStockBaseQty(FuzzyQueryStockBaseQtyRequest request)
		{
			ResponseMessage<List<MesFuzzyQueryStockBaseQtyEntity>> response = new();

			var AddSql = " where org.FORGID=7401803 and t1.FQty>0  ";
			if (!string.IsNullOrWhiteSpace(request.Textures))
			{
				AddSql += $" and wlcz.FNUMBER='{request.Textures}' ";
			}
			if (!string.IsNullOrWhiteSpace(request.MaterialType))
			{
				AddSql += $" and wlcx.FNUMBER='{request.MaterialType}' ";
			}
			if (request.StartLength != 0)
			{
				AddSql += $" and mb.FLENGTH>={request.StartLength} ";
			}
			if (request.EndLength != 0)
			{
				AddSql += $" and mb.FLENGTH<={request.EndLength} ";
			}
			if (request.StartWidth != 0)
			{
				AddSql += $" and mb.FWIDTH>={request.StartWidth} ";
			}
			if (request.EndWidth != 0)
			{
				AddSql += $" and mb.FWIDTH<={request.EndWidth} ";
			}
			if (request.StartHeight != 0)
			{
				AddSql += $" and mb.FHEIGHT>={request.StartHeight} ";
			}
			if (request.EndHeight != 0)
			{
				AddSql += $" and mb.FHEIGHT<={request.EndHeight} ";
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
			                        SUM(t1.FQty) QTY, --库存量(库存量库存主单位)
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
			var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<MesFuzzyQueryStockBaseQtyEntity>(sql).ToList();
			response.Data = datas;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <returns></returns>
		public ResponseMessage<List<MesStockPlatEntity>> GetMesStockQuantity(List<string> itemNos)
		{
			ResponseMessage<List<MesStockPlatEntity>> result = new() { Data = [] };

			itemNos ??= [];
			var AddSql = " where org.FORGID=7401803 ";
			if (itemNos.Count > 0)
			{
				AddSql += $" and m.FNUMBER in ('{string.Join("','", itemNos)}') ";
			}
			var sql = $@"/*dialect*/select 
			                        m.FMASTERID as FMATERIALID,
									org.FORGID as FORGID,
									org.FNUMBER as ORGNUM,
									sto.FSTOCKID as STOID,
									sto.FNUMBER as STONUM,
			                        m.FNUMBER as MATERIALNUM,
									u.FNUMBER as UNITNUM,--(库存主单位)
			                        SUM(t1.FQty) QTY --库存量(库存量库存主单位)
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
			var datas = _kingdeeContent.SqlSugar.Ado.SqlQuery<MesStockPlatEntity>(sql).ToList();
			result.Data = datas;
			result.Code = ResponseCode.Success;
			return result;
		}

	}
}
