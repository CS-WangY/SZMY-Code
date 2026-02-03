using Elastic.Clients.Elasticsearch.Core.Search;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Model.Gateway;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ProductionModel;
using mymooo.k3cloud.core.StockModel;
using mymooo.product.selection;
using mymooo.product.selection.SelectionModel;
using Mymooo.Threed.WebService.Core.ThreedModel;
using Mymooo.Threed.WebService.SDK;
using static Community.CsharpSqlite.Sqlite3;

namespace mymooo.k3cloud.business.Services.Production
{
	/// <summary>
	/// 生产管理服务
	/// </summary>
	[AutoInject(InJectType.Scope)]
	public class ProductionService(KingdeeContent kingdeeContent, ThreedServiceClient<KingdeeContent, User> threedService, ProductCache<KingdeeContent, User> productCache)
	{
		//private readonly KingdeeContent _kingdeeContent = kingdeeContent;
		public async Task<ResponseMessage<MakeDispatchRequest>> SendMakeDispatch(MakeDispatchRequest request)
		{
			ResponseMessage<MakeDispatchRequest> response = new();
			var threedUrl = kingdeeContent.GatewayRedisCache.HashGet(new SystemEnvironmentConfig() { SystemEnvCode = "threed" }, "system");
			if (threedUrl == null)
			{
				response.Code = ResponseCode.NotFound;
				response.ErrorMessage = "3D服务不存在";
				return response;
			}
			response.Data = request;
			SourceConfig sourceConfig = new(new SourceFilter() { Includes = new string[] { "productId", "pdfUrl", "isRelease", "threedParamSeq", "brandId", "brandName", "brandEnName", "threedModelFolder" } });
			foreach (var detail in request.Details)
			{
				ProductParameterValueRequest parameterValueRequest = new() { Number = detail.DwgNo };
				var product = await productCache.GetProductParameterValue(kingdeeContent, parameterValueRequest, sourceConfig);
				if (product != null)
				{
					detail.ProductId = product.ProductId;
					detail.PlaneUrl = product.PdfUrl?.Replace("/pdfview/pdfView.html?file=..", "");
					detail.ParameterValues = product.ParameterValues;
					var threedResponse = await threedService.GenerateThreedFile(kingdeeContent, product, ThreedFileType.STP);
					if (threedResponse.IsSuccess && threedResponse.Data != null)
					{
						detail.ThreeUrl = threedUrl.Url + "Threed/DownLoad3dFile?file=" + threedResponse.Data.MeshFile;
						detail.ThreeVer = threedResponse.Data.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
					}
				}
			}

			return response;
		}

		public ResponseMessage<List<MesRawMaterialsRequest>> GetMesRawMaterialsPrice(List<MesRawMaterialsRequest> request)
		{
			ResponseMessage<List<MesRawMaterialsRequest>> result = new() { Data = null };
			foreach (var item in request)
			{
				var sql = $@"/*dialect*/SELECT t1.FSALEORDERID,t1.FSALEORDERENTRYID FROM dbo.T_PRD_MOENTRY t1
					INNER JOIN dbo.T_PRD_MO t2 ON t2.FID = t1.FID
					WHERE t1.FID=@FID AND t1.FENTRYID=@FENTRYID";
				var list = kingdeeContent.SqlSugar.Ado.SqlQuerySingle<MesRawSalInfo>(sql, new { FID = item.moId, FENTRYID = item.moEntryId });

				foreach (var details in item.rawMtlInfo)
				{
					if (list is null)
					{
						details.taxPrice = 0;
					}
					else
					{
						sql = $@"SELECT t2.FTAXPRICE FROM t_PUR_POOrderEntry_R t1
					INNER JOIN t_PUR_POOrderEntry_F t2 ON t2.FENTRYID = t1.FENTRYID
					INNER JOIN t_PUR_POOrderEntry t3 ON t1.FENTRYID = t3.FENTRYID
					LEFT JOIN dbo.T_BD_MATERIAL t4 ON t3.FMATERIALID=t4.FMATERIALID
					WHERE FDEMANDBILLENTRYID=@FDEMANDBILLENTRYID AND t4.FNUMBER=@FNUMBER";
						var taxprice = kingdeeContent.SqlSugar.Ado.SqlQuerySingle<decimal>(sql,
							new
							{
								FDEMANDBILLENTRYID = list.FSALEORDERENTRYID,
								FNUMBER = details.mtlCode
							});
						if (taxprice <= 0)
						{
							sql = $@"SELECT TOP 1 t2.FTAXPRICE FROM t_PUR_POOrderEntry_R t1
					INNER JOIN t_PUR_POOrderEntry_F t2 ON t2.FENTRYID = t1.FENTRYID
					INNER JOIN t_PUR_POOrderEntry t3 ON t1.FENTRYID = t3.FENTRYID
					INNER JOIN dbo.T_PUR_POORDER t4 ON t3.FID=t4.FID
					LEFT JOIN dbo.T_BD_MATERIAL t5 ON t3.FMATERIALID=t5.FMATERIALID
					WHERE t5.FNUMBER=@FNUMBER AND t4.FDOCUMENTSTATUS='C'
					ORDER BY t4.FDATE DESC";
							taxprice = kingdeeContent.SqlSugar.Ado.SqlQuerySingle<decimal>(sql,
							new
							{
								FNUMBER = details.mtlCode
							});
							details.taxPrice = taxprice;
						}
						else
						{
							details.taxPrice = taxprice;
						}
					}
				}
			}

			result.Data = request;
			result.Code = ResponseCode.Success;
			return result;
		}
	}
}
