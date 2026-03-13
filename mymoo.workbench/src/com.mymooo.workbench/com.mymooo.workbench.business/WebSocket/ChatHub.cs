using com.mymooo.workbench.business.WebSocket.InputData;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.WebSocket.InputData;
using Microsoft.AspNetCore.SignalR;
using mymooo.core.Account;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Threading.Tasks;

namespace com.mymooo.workbench.business.WebSocket
{
	public class ChatHub(InputDataExecute inputDataExecute, WorkbenchContext workbenchContext) : Hub
    {
        private readonly InputDataExecute _inputDataExecute = inputDataExecute;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

		/// <summary>
		/// 发送消息给全部用户
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messageType"></param>
		/// <returns></returns>
		public async Task SendMessageToAllUser(string message, string messageType = "ReceiveMessage")
        {
            if (Context.User != null)
            {
                _workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
            }
            await Clients.All.SendAsync(messageType, message);
        }

        /// <summary>
        /// 发送消息给单用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public async Task SendMessageToUser(string user, string message, string messageType = "ReceiveMessage")
        {
            if (Context.User != null)
            {
                _workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
            }
            await Clients.Group(user).SendAsync(messageType, message);
        }

        /// <summary>
        /// 发送消息给当前连接用户
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public async Task SendMessageToConnection(string message, string messageType = "ReceiveMessage")
        {
            if (Context.User != null)
            {
                _workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
            }
            await this.Clients.Client(Context.ConnectionId).SendAsync(messageType, message);
        }

        public async Task InputData(string message)
        {
            if (Context.User != null)
            {
                _workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
            }
            //InputDataExecute execute = new InputDataExecute();
            await _inputDataExecute.Execute(JsonSerializerOptionsUtils.Deserialize<InputDataMessageRequest>(message), this, Context);
        }

        public override Task OnConnectedAsync()
        {
			//没有登录情况下,直接关闭连接
			if (Context.User != null)
			{
				_workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
				this.Groups.AddToGroupAsync(Context.ConnectionId, _workbenchContext.User.Code);
			}
			else
			{
				this.Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", JsonSerializerOptionsUtils.Serialize(new { ErrorMessage = "未登录情况下,不能连接!" }));
				Context.Abort();
			}
			return base.OnConnectedAsync();
        }
        
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User != null)
            {
                _workbenchContext.User = (Context.User as MymoooPrincipal<User>).User;
                Groups.RemoveFromGroupAsync(Context.ConnectionId, _workbenchContext.User.Code);
			}
			return base.OnDisconnectedAsync(exception);
        }
    }
}
