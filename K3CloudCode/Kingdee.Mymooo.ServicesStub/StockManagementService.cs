using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class StockManagementService : KDBaseService
    {
        public StockManagementService(KDServiceContext context)
        : base(context)
        {
        }
        public string GetStockPlatform()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                StockBusiness stockBusiness = new StockBusiness();
                response = stockBusiness.GetStockPlatform(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }
        /// <summary>
        /// 获取即时库存
        /// </summary>
        /// <returns></returns>
        public string GetStockQuantityNew()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                StockBusiness stockBusiness = new StockBusiness();
                response = stockBusiness.GetStockQuantity(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 云仓储根据供货组织、云存储仓库编号获取物料总库存量
        /// </summary>
        /// <returns></returns>
        public string GetCloudStockBaseQty()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                StockBusiness stockBusiness = new StockBusiness();
                response = stockBusiness.GetCloudStockBaseQty(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// MES生成盘盈盘亏单
        /// </summary>
        /// <returns></returns>
        public string MesGenerateStkCountGainAndLoss()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                StockBusiness stockBusiness = new StockBusiness();
                response = stockBusiness.MesGenerateStkCountGainAndLoss(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
		/// <summary>
		/// MES生成其他入库单
		/// </summary>
		/// <returns></returns>
		public string MesStkMiscellaneousService()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var request = JsonConvertUtils.DeserializeObject<Mes_STK_MiscellaneousRequest>(data);
                response = StockOrderServiceHelper.MesStkMiscellaneousService(ctx, request);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <returns></returns>
		public string GetMesFuzzyQueryStockBaseQty()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				StockBusiness stockBusiness = new StockBusiness();
				response = stockBusiness.GetMesFuzzyQueryStockBaseQty(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <returns></returns>
		public string GetMesStockQuantity()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				StockBusiness stockBusiness = new StockBusiness();
				response = stockBusiness.GetMesStockQuantity(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}
	}
}
