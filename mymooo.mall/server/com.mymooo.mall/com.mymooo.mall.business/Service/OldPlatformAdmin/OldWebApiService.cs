using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.OldPlatformAdmin.Selection;
using com.mymooo.mall.core.Model.Product;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;

namespace com.mymooo.mall.business.Service.OldPlatformAdmin
{
	[AutoInject(InJectType.Scope)]
	public class OldWebApiService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;

		public async Task<ProductionSelectionResponse?> ProductionSelection(ProductionSelectionResponse.Querys querys, string pn)
		{
			var result = await _httpService.InvokeWebServiceAsync($"webapi/{_mymoooContext.ApigatewayConfig.EnvCode}/CalculateMaster/GetCalculateModelMaster?PN={pn}", JsonSerializerOptionsUtils.Serialize(querys));

			return JsonSerializerOptionsUtils.Deserialize<ProductionSelectionResponse>(result);
		}

		public async Task<ShortNumberselectionResponse?> GetProductParameterValue(long productId, string productNumber, List<ShortNumberselectionRequest> requests)
		{
			var result = await _httpService.InvokeWebServiceAsync($"feeApi/{_mymoooContext.ApigatewayConfig.EnvCode}/ProductModel/GetProductParameterValue?productId={productId}&number={productNumber}"
				, JsonSerializerOptionsUtils.Serialize(requests));

			var response = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<ShortNumberselectionResponse>>(result);
			if (response != null && response.IsSuccess)
			{
				return response.Data;
			}
			return null;
		}
	}
}
