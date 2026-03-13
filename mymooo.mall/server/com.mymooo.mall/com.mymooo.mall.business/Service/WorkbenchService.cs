using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.WeChat;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;

namespace com.mymooo.mall.business.Service
{
	[AutoInject(InJectType.Scope)]
	public class WorkbenchService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;


		/// <summary>
		/// 获取当前业务员的上级,企业微信
		/// </summary>
		/// <param name="WechatCode"></param>
		/// <returns></returns>
        public List<WeUserResponse> GetHigherUps(string WechatCode)
		{
			string sResult;
			try
			{
				sResult = _httpService.InvokeWebService($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/WeiXinWork/GetHigherUps?userCode={WechatCode}", string.Empty);
			}
			catch  
			{
				return new List<WeUserResponse>();
			}

			var result = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<List<WeUserResponse>>>(sResult);
			if (result != null)
			{
				if (result.Data != null)
				{
					return result.Data;
				}
			}
			return [];
		}

		/// <summary>
		/// 获取相关业务助理的企业微信
		/// </summary>
		/// <param name="WechatCode"></param>
		/// <param name="isAssistant"></param>
		/// <returns></returns>
		public async Task<List<WeUserAssistant>> GetUserAssistantByWxCode(string WechatCode, bool isAssistant = true)
		{
			string response;
			try
			{
				response = await _httpService.InvokeWebServiceAsync($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/SystemManage/GetUserAssistantByWxCode?isAssistant=true&userWxCode={WechatCode}", string.Empty);
			}
			catch
			{
				return [];
			}

			var result = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<List<WeUserAssistant>>>(response);
			if (result != null && result.IsSuccess)
			{
				if (result.Data != null)
				{
					return result.Data;
				}
			}
			return [];
		}

        public  string SendTextMessage(string inputData)
        {
            string response;

            response = _httpService.InvokeWebService($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/WeiXinWork/SendTextMessage", inputData);

            var result = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<dynamic>>(response);

			return result == null ? "" : result.Code;
        }		
    }
}
