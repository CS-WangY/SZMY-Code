using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Kingdee.Mymooo.Core.Utils
{
    /// <summary>
    /// 发送企业微信消息通用接口
    /// </summary>
    public static class SendTextMessageUtils
    {
        /// <summary>
        /// 发送企业微信消息
        /// </summary>
        /// <param name="wxuserid"> 
        /// 指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。
        /// 特殊情况：指定为”@all”，则向该企业应用的全部成员发送</param>
        /// <param name="content">消息内容，最长不超过2048个字节，超过将截断（支持id转译）</param>
        /// <returns></returns>
        public static MessageHelpForCredit SendTextMessage(string wxuserid, string content)
        {
            MessageHelpForCredit response = new MessageHelpForCredit();
            try
            {
                if (string.IsNullOrWhiteSpace(wxuserid))
                {
                    response.IsSuccess = false;
                    response.Message = "微信Code不能为空";
                    return response;
                }
                if (string.IsNullOrWhiteSpace(content))
                {
                    response.IsSuccess = false;
                    response.Message = "发送的内容不能为空";
                    return response;
                }
                SendTextMessageRequest WxEntity = new SendTextMessageRequest();
                WxEntity = new SendTextMessageRequest()
                {
                    touser = wxuserid,
                    text = new SendTextMessageRequest.Text()
                    {
                        content = content
                    }
                };
                string url = $"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendTextMessage";
                var inputData = JsonConvertUtils.SerializeObject(WxEntity);
                var r = ApigatewayUtils.InvokePostWebService(url, inputData);
                var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(r);
                if (returninfo.Code != "success")
                {
                    response.IsSuccess = false;
                    response.Message = returninfo.ErrorMessage;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "发送成功";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;

        }
		/// <summary>
		/// 发送企业微信markdown消息
		/// </summary>
		/// <param name="wxuserid"></param>
		/// 指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。
		/// 特殊情况：指定为”@all”，则向该企业应用的全部成员发送</param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static MessageHelpForCredit SendMarkdownMessage(string wxuserid, string content)
		{
			MessageHelpForCredit response = new MessageHelpForCredit();
			try
			{
				if (string.IsNullOrWhiteSpace(wxuserid))
				{
					response.IsSuccess = false;
					response.Message = "微信Code不能为空";
					return response;
				}
				if (string.IsNullOrWhiteSpace(content))
				{
					response.IsSuccess = false;
					response.Message = "发送的内容不能为空";
					return response;
				}
				SendMarkdownMessageRequest WxEntity = new SendMarkdownMessageRequest();
				WxEntity = new SendMarkdownMessageRequest()
				{
					ToUser = wxuserid,
					MarkDown = new SendMarkdownMessageRequest.MarkDownMessage()
					{
						Content = content
					}
				};
				//var response = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendMarkdownMessage", JsonConvertUtils.SerializeObject(WxEntity));
				string url = $"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Application/SendMarkdownMessage";
				var inputData = JsonConvertUtils.SerializeObject(WxEntity);
				var r = ApigatewayUtils.InvokePostWebService(url, inputData);
				var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(r);
				if (returninfo.Code != "success")
				{
					response.IsSuccess = false;
					response.Message = returninfo.ErrorMessage;
				}
				else
				{
					response.IsSuccess = false;
					response.Message = "发送成功";
				}
			}
			catch (Exception ex)
			{
				response.IsSuccess = false;
				response.Message = ex.Message;
			}
			return response;

			
		}
	}
}
