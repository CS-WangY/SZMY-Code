using mymooo.core.Attributes;
using mymooo.core.Model.SqlSugarCore;

namespace mymooo.k3cloud.business.Register
{
    [AutoInject(InJectType.Single)]
    public class KingdeeInitialize : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
