using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using mymooo.core.Attributes;
using System;

namespace com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute
{
    /// <summary>
    /// 应用消息处理
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class ApplicationMessageExecute
    {

        /// <summary>
        /// 应用消息处理
        /// </summary>
        /// <param name="message"></param>
        public void Execute(WeiXinMessage message)
        {
            dynamic messageInfo = XmlUtils.GetAnonymousType(message.Message);
            //if (!messageInfo.MsgType.Equals("event", StringComparison.OrdinalIgnoreCase))
            //{
            //    _weixinWorkService.SendTextMessage(WeixinWorkAppName.Application, new SendTextMessageRequest()
            //    {
            //        touser = messageInfo.FromUserName,
            //        text = new SendTextMessageRequest.Text()
            //        {
            //            content = "您的消息我们已经收到,非常感谢您的留言!"
            //        }
            //    }, message.ApplicationDetail.AppId);
            //}
            message.IsComplete = true;
            message.Result = "完成";
            message.CompleteDate = DateTime.Now;
        }
	}
}
