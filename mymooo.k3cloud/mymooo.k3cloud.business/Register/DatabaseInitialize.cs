using mymooo.core.Attributes;
using mymooo.core.Attributes.Redis;
using mymooo.core.Model.SqlSugarCore;
using mymooo.core.Utils;

namespace mymooo.k3cloud.business.Register
{
    [AutoInject(InJectType.Transient)]
    [RedisKey("mymooo-database-initialize", 0)]
    public class DatabaseInitialize : IDisposable
    {
        /// <summary>
        /// 初始化表
        /// </summary>
        [SystemInitialize(1)]
        public void InitTable(MymoooSqlSugar sqlSugar, MymoooVersion mymoooVersion)
        {
            if (mymoooVersion.Version.VersionComparison("1.0.1.1"))
            {
                //sqlSugar.CodeFirst.SplitTables().InitTables<RabbitMQMessage>();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
