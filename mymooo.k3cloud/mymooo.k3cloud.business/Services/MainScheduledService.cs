using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Model.SqlSugarCore;
using mymooo.core.Utils.Service;
using mymooo.k3cloud.core.Account;

namespace mymooo.k3cloud.business.Services
{
    [AutoInject(InJectType.Single)]
	public class MainScheduledService(KafkaSendService<KingdeeContent, User> rabbitMQService)
	{
		private readonly KafkaSendService<KingdeeContent, User> _rabbitMQService = rabbitMQService;

		public void MainStartAsync()
		{
			_rabbitMQService.Start();
		}

		public void MainStopAsync()
		{
			_rabbitMQService.Stop();
		}

		public void MonthCreateTable(MymoooSqlSugar sqlSugar)
		{
			var message = new RabbitMQMessage() { VirtualHost = "VirtualHost", Message = "", MessageId = -1, Exchange = "Exchange", Routingkey = "Routingkey" };
			sqlSugar.UseTran(()=>
			{
                sqlSugar.Insertable(message).SplitTable().ExecuteCommand();
                sqlSugar.Deleteable(message).SplitTable().ExecuteCommand();
            });
        }
    }
}
