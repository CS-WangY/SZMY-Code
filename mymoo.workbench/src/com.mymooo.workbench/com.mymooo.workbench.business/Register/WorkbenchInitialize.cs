using mymooo.core.Attributes;
using mymooo.core.Model.SqlSugarCore;
using System;

namespace com.mymooo.workbench.business.Register
{
    [AutoInject(InJectType.Transient)]
    public class WorkbenchInitialize : IDisposable
    {
        /// <summary>
        /// 初始化任务表
        /// </summary>
        /// <param name="sqlSugar"></param>
        [SystemInitialize]
        public void InitScheduledTask(MymoooSqlSugar sqlSugar)
        {
            sqlSugar.CodeFirst.SplitTables().InitTables<RabbitMQMessage>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}