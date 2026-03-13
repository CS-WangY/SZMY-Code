using com.mymooo.workbench.business.WebSocket.HandleInputData;
using com.mymooo.workbench.core.Enum;
using com.mymooo.workbench.core.WebSocket.InputData;
using Microsoft.AspNetCore.SignalR;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Threading.Tasks;

namespace com.mymooo.workbench.business.WebSocket.InputData
{
    [AutoInject(InJectType.Scope)]
	public class InputDataExecute
    {
        private readonly HandleInputDataExecute _handleInputDataExecute;
        public InputDataExecute(HandleInputDataExecute handleInputDataExecute)
        { 
         _handleInputDataExecute = handleInputDataExecute;
        }
        public async Task Execute(InputDataMessageRequest message,ChatHub chatHub, HubCallerContext context)
        {
            InputDataMessageResponse<dynamic> response = new InputDataMessageResponse<dynamic>();
            try
            {
                switch (message.Type)
                {
                    case WebSocketImportType.AdressBook:
                        await _handleInputDataExecute.HandleSynchronizeing(context, async (callback) => {
                            response.Progress = callback.Progress;
                            response.ErrorMessage = callback.ErrorMessage;
                            response.Code = callback.Code;
                            response.IsEnd = callback.IsEnd;
                            await chatHub.SendMessageToConnection(JsonSerializerOptionsUtils.Serialize(response), "InputData");
                        });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {

                response.Progress = 100;
                response.ErrorMessage = e.Message;
                await chatHub.SendMessageToConnection(JsonSerializerOptionsUtils.Serialize(response), "InputData");
            }
            
        }
    }
}
