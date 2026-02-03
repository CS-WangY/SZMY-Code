using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn
{
    public abstract class AbstractRabbitMQOperationServicePlugIn : AbstractOperationServicePlugIn
	{

		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			Task.Factory.StartNew(() =>
            {
                //晚5个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(5000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");

			});
		}
	}
}
