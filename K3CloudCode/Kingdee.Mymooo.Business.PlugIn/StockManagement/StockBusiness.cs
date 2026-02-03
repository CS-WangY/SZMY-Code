using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.ALI;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingdee.Mymooo.Core.StockManagement.WarehousesInventoryQueryEntity.Goods;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
	/// <summary>
	/// 库存管理
	/// </summary>
	public class StockBusiness
	{
		public ResponseMessage<dynamic> GetStockPlatform(Context ctx, string message)
		{
			List<KeyValue> request = JsonConvert.DeserializeObject<List<KeyValue>>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (request.Count == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "请传物料号";
				return response;
			}
			//List<StockQuantityEntity> list = new List<StockQuantityEntity>();

			var lists = StockQuantityServiceHelper.StockPlatformAction(ctx, request);

			response.Data = lists;
			response.Code = ResponseCode.Success;
			response.Message = "获取成功";
			return response;
		}
		/// <summary>
		/// 获取即时库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetStockQuantity(Context ctx, string message)
		{
			List<string> request = JsonConvert.DeserializeObject<List<string>>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (request.Count == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "请传物料号";
				return response;
			}
			//List<StockQuantityEntity> list = new List<StockQuantityEntity>();

			var list = StockQuantityServiceHelper.StockQuantityActionV2(ctx, request);
			//var lists = list.GroupBy(p => p.ItemNo).Select(t => new StockQuantityData
			//{
			//    ItemNo = t.Key,
			//    Quantity = t.Sum(s => s.Quantity),
			//    AvailableQuantity = t.Sum(s => s.UsableQty),
			//    StockInfo = new StockInfo
			//    {
			//        UnQtyShipdSum = t.Sum(s => s.UnQtyShipdSum),
			//        UsableQty = t.Sum(s => s.UsableQty),
			//        OnOrderQty = t.Sum(s => s.OnOrderQty),
			//        QtyInsp = t.Sum(s => s.QtyInsp)
			//    }
			//});

			response.Data = list;
			response.Code = ResponseCode.Success;
			response.Message = "获取成功";
			return response;
		}

		/// <summary>
		/// 获取仓库列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetWarehouseList(Context ctx)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			List<WarehouseEntity> entityList = new List<WarehouseEntity>();
			var sql = $@"/*dialect*/select sto.FSTOCKID,stol.FNAME,orgl.FNAME OrgName from t_BD_Stock sto
                        inner join T_BD_STOCK_L stol on sto.FSTOCKID=stol.FSTOCKID
                        left  join t_org_organizations org on sto.FUseOrgId=org.FORGID
						left  join T_ORG_ORGANIZATIONS_L orgl on orgl.FORGID=org.FORGID
                        where sto.FForbidStatus='A' and sto.FDocumentStatus='C'  and org.FFORBIDSTATUS='A' and org.FORGID<>'4093663'
                        order by sto.FUseOrgId asc ";

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				WarehouseEntity entity = new WarehouseEntity();
				entity.WarehouseId = Convert.ToString(data["FSTOCKID"]);
				if (!string.IsNullOrWhiteSpace(Convert.ToString(data["OrgName"])))
				{
					entity.WarehouseName = Convert.ToString(data["FNAME"]) + "(" + Convert.ToString(data["OrgName"]) + ")";
				}
				else
				{
					entity.WarehouseName = Convert.ToString(data["FNAME"]);
				}
				entityList.Add(entity);
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 云仓储根据供货组织、云存储仓库编号获取物料总库存量
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetCloudStockBaseQty(Context ctx, string message)
		{
			CloudStockBaseQtyRequest request = JsonConvert.DeserializeObject<CloudStockBaseQtyRequest>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrWhiteSpace(request.SupplyOrgCode))
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "供货组织编码不能为空";
				return response;
			}
			if (string.IsNullOrWhiteSpace(request.CloudStockCode))
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "云仓储仓库编码不能为空";
				return response;
			}
			var list = StockQuantityServiceHelper.CloudStockBaseQty(ctx, request);
			response.Data = list;
			response.Code = ResponseCode.Success;
			response.Message = "获取成功";
			return response;
		}


		/// <summary>
		/// MES生成盘盈盘亏单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> MesGenerateStkCountGainAndLoss(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrWhiteSpace(message))
			{
				response.Message = "参数信息不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			MesGenerateStkCountGainAndLossRequest data = JsonConvertUtils.DeserializeObject<MesGenerateStkCountGainAndLossRequest>(message);
			if (string.IsNullOrWhiteSpace(data.BillNo))
			{
				response.Message = "单据编号不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			return StockOrderServiceHelper.MesGenerateStkCountGainAndLossService(ctx, data);
		}

		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetMesFuzzyQueryStockBaseQty(Context ctx, string message)
		{
			FuzzyQueryStockBaseQtyRequest request = JsonConvertUtils.DeserializeObject<FuzzyQueryStockBaseQtyRequest>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrEmpty(request.Textures) && string.IsNullOrEmpty(request.MaterialType) &&
				request.StartLength == 0 && request.EndLength == 0 &&
				request.StartWidth == 0 && request.EndWidth == 0 &&
				request.StartHeight == 0 && request.EndHeight == 0
				)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "参数不能全部为空";
				return response;
			}
			var list = StockQuantityServiceHelper.GetMesFuzzyQueryStockBaseQty(ctx, request);
			response.Data = list;
			response.Code = ResponseCode.Success;
			response.Message = "获取成功";
			return response;
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetMesStockQuantity(Context ctx, string message)
		{
			List<string> request = JsonConvert.DeserializeObject<List<string>>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (request.Count == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "请传物料号";
				return response;
			}

			var list = StockQuantityServiceHelper.MesStockQuantityAction(ctx, request);
			response.Data = list;
			response.Code = ResponseCode.Success;
			response.Message = "获取成功";
			return response;
		}
	}
}
