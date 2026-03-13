using com.mymooo.workbench.business.WeixinWork.Approval;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute;
using Microsoft.Extensions.DependencyInjection;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using System;
using System.Threading.Tasks;

namespace com.mymooo.workbench.business.QuartzTask.WeiXinWork
{
    /// <summary>
    /// 后台消息处理
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class MessageScheduled(IServiceProvider serviceProvider, RedisCache redisCache)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly RedisCache _redisCache = redisCache;
        public async Task ExecuteAsync(WorkbenchContext workbenchContext)
        {
            await ExecWeiXinMessage(workbenchContext);
        }

        public async Task MainStartAsync(WorkbenchContext workbenchContext)
        {
            await ExecWeiXinMessage(workbenchContext);
        }

        public async Task ExecWeiXinMessage(WorkbenchContext workbenchContext)
        {
            if (_redisCache.Lock<WeiXinMessage>(new TimeSpan(0, 10, 0)))
            {
                try
                {
                    var dateE = DateTime.Now.AddMinutes(-1);
                    var dateS = DateTime.Now.AddDays(-1);
                    var messages = workbenchContext.SqlSugar.Queryable<WeiXinMessage>().Includes(c => c.ApplicationDetail).Where(p => p.IsComplete == false && p.CreateDate < dateE && p.CreateDate >= dateS).OrderBy(p => p.Id).ToList();
                    foreach (var message in messages)
                    {
                        await ExecWeiXinMessage(message, workbenchContext);
                    }
                }
                catch
                {
                }
                finally
                {
                    _redisCache.RemoveLock<WeiXinMessage>();
                }
            }
        }

        public async Task ExecWeiXinMessage(WeiXinMessage message, WorkbenchContext workbenchContext)
        {
            if (string.IsNullOrWhiteSpace(message.ApplicationDetail.MessageExecute))
            {
                message.IsComplete = true;
                message.CompleteDate = DateTime.Now;
                message.Result = "未配置操作类";
            }
            else
            {
                try
                {
                    using var scope = _serviceProvider.CreateAsyncScope();
                    if (message.ApplicationDetail.DetailCode.Equals("Approval", StringComparison.OrdinalIgnoreCase))
                    {
                        var approvalService = scope.ServiceProvider.GetRequiredService<ApprovalService>();
                        message.IsComplete = await approvalService.WeiXinCallBack(message);
                    }
                    else if (message.ApplicationDetail.DetailCode.Equals("AddressBook", StringComparison.OrdinalIgnoreCase))
                    {
                        var type = Type.GetType(message.ApplicationDetail.MessageExecute);
                        var executeService = scope.ServiceProvider.GetRequiredService<AddressBookMessageExecute>();
                        await executeService.Execute(message);
                    }
                    else if (message.ApplicationDetail.DetailCode.Equals("Application", StringComparison.OrdinalIgnoreCase))
                    {
                        var type = Type.GetType(message.ApplicationDetail.MessageExecute);
                        var executeService = scope.ServiceProvider.GetRequiredService<ApplicationMessageExecute>();
                        executeService.Execute(message);
                    }
                }
                catch (Exception e)
                {
                    //message.IsComplete = true;
                    message.Result = e.Message;
                    message.StackTrace = e.StackTrace;
                }
                workbenchContext.SqlSugar.Updateable(message).UpdateColumns(c => new { c.Status, c.Spno, c.IsComplete, c.CompleteDate, c.Result, c.StackTrace }).ExecuteCommand();
            }
        }
    }
}
