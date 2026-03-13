using com.mymooo.mall.core.Account;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.Service;

namespace com.mymooo.mall.business.Service
{
	[AutoInject(InJectType.Scope)]
	public class GatewayService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;

		public async Task SendMessage(string rabbitCode, string message)
		{
			try
			{
				await _httpService.InvokeWebServiceAsync($"RabbitMQ/SendMessage?rabbitCode={rabbitCode}{_mymoooContext.ApigatewayConfig.EnvCode}", message);
			}
			catch
			{
			}
		}
	}
}
