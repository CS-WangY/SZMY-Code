using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.Price.CalcPriceList;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace com.mymooo.mall.business.Service
{
	[AutoInject(InJectType.Scope)]
	public class CrmService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;

		public List<PriceListCalcResult> GetCrmCustomerPriceList(PriceListCalcRequest req)
		{
			string sResult;
			try
			{
				sResult = _httpService.InvokeWebService($"crm/{_mymoooContext.ApigatewayConfig.EnvCode}/SalesMatrixPriceList/CalcPriceListPrice", JsonConvert.SerializeObject(req));
			}
			catch  // CRM如果调用失败,继续处理。 gateway 会记录日志. 业务影响是无法应用客户价目表.
			{
				return new List<PriceListCalcResult>();
			}

			var result = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<List<PriceListCalcResult>>>(sResult);
			if (result != null)
			{
				if (result.Data != null)
				{
					return result.Data;
				}
			}
			return [];
		}

		public async Task<ConcurrentDictionary<string, ProductSelectionPriceIndex>> GetFullCompanyPriceList(string companyCode, List<string> productModels)
		{
			string response;
			try
			{
				response = await _httpService.InvokeWebServiceAsync($"crm/{_mymoooContext.ApigatewayConfig.EnvCode}/SalesMatrixPriceList/GetFullPriceList", JsonConvert.SerializeObject(new { companyCode, productModels }));
			}
			catch
			{
				return [];
			}

			var result = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<ConcurrentDictionary<string, ProductSelectionPriceIndex>>>(response);
			if (result != null && result.IsSuccess)
			{
				if (result.Data != null)
				{
					return result.Data;
				}
			}
			return [];
		}

		public async Task<ConcurrentDictionary<string, List<ProductSelectionPriceIndex>>> GetFullPriceList(List<string> productModels)
		{
			string response;
			try
			{
				response = await _httpService.InvokeWebServiceAsync($"crm/{_mymoooContext.ApigatewayConfig.EnvCode}/SalesMatrixPriceList/GetFullPriceList", JsonConvert.SerializeObject(new { productModels }));
			}
			catch
			{
				return [];
			}

			var result = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<ConcurrentDictionary<string, List<ProductSelectionPriceIndex>>>>(response);
			if (result != null && result.IsSuccess)
			{
				if (result.Data != null)
				{
					return result.Data;
				}
			}
			return [];
		}
	}
}
