using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.Product;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.Service;
using Newtonsoft.Json;

namespace com.mymooo.mall.business.Service
{
	[AutoInject(InJectType.Scope)]
	public class ErpService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;


		//public List<StockPlatEntity>? GetErpStockDataBySupplyOrg(List<Tuple<string, string>> products)
		//{
		//	//获取即时库存
		//	try
		//	{
		//		List<KeyValue<string, string>> p = new List<KeyValue<string, string>>();
		//		foreach (var product in products)
		//		{
		//			if (!string.IsNullOrEmpty(product.Item1) && !string.IsNullOrEmpty(product.Item2))
		//			{
		//				KeyValue<string, string> i = new()
		//				{
		//					Key = product.Item1,
		//					Value = product.Item2
		//				};
		//				p.Add(i);
		//			}
		//		}
		//		var erpList = _httpService.InvokeWebService($"k3cloud/{_mymoooContext.ApigatewayConfig.EnvCode}/Kingdee.Mymooo.webapi.ServicesStub.StockManagementService.GetStockPlatform.common.kdsvc", JsonConvert.SerializeObject(p), "text/plain");
		//		var result = JsonConvert.DeserializeObject<MessagesHelp<List<StockPlatEntity>>>(erpList);

		//		if (result != null && result.IsSuccess)
		//		{
		//			if (result.Data == null) return null;
		//			foreach (var item in result.Data)
		//			{
		//				item.FOutSourceStockLoc = convert(item.FOutSourceStockLoc);
		//			}
		//			return result.Data;
		//		}
		//		else
		//		{
		//			return null;
		//		}
		//	}
		//	catch (Exception)
		//	{
		//		return null;
		//	}
		//	string convert(string code)
		//	{
		//		switch (code.ToUpper())
		//		{
		//			case "HS":
		//				return "惠山";

		//			case "DLS":
		//				return "大岭山";

		//			default:
		//				return "";
		//		}
		//	}
		//}

		private class KeyValue<T1, T2>
		{
			public required T1 Key { get; set; }
			public required T2 Value { get; set; }
		}
	}
}
