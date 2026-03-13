using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.SystemManage;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using mymooo.core.Attributes;
using Org.BouncyCastle.Ocsp;

namespace com.mymooo.mall.business.Service.SystemService
{
	[AutoInject(InJectType.Scope)]
	public class SystemWebParamService(MallContext mymoooContext)
	{
		private readonly MallContext _mymoooContext = mymoooContext;

		public List<WebParamModel> GetAllWebParam()
        {
            return _mymoooContext.SqlSugar.Queryable<WebParamConfig>()
                 .Select(r => new WebParamModel() { WContent = r.WContent, WKey = r.WKey, WValue = r.WValue, WKeyType = r.WKeyType })
                 .ToList();
        }

        public string SaveSiteParamConfig(KeyValuePair<string, string> req)
        {
            string sResult = "保存成功,并已成功更新缓存";

            var record = _mymoooContext.SqlSugar.Queryable<WebParamConfig>().Where(r => r.WKey.ToLower() == req.Key.ToLower()).First();

            if (record != null)
            {
                record.WValue = req.Value;
                _mymoooContext.SqlSugar.Updateable<WebParamConfig>(record).UpdateColumns(r=> r.WValue).ExecuteCommand();
                SiteParamCache();
            }
            else
            {
                sResult = "对应的Key没有找到";
            }
            return sResult;
        }

        private void SiteParamCache()
        {
            var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
            var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<WebParamConfig>();
            var filter = " [RowVersion] <= @EndTimeStamp";
            if (startTimeStamp != null)
            {
                filter += " and [RowVersion] > @StartTimeStamp ";
            }
            var profiles = _mymoooContext.SqlSugar.Queryable<WebParamConfig>().Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).ToList();

            foreach (var profile in profiles)
            {
                _mymoooContext.RedisCache.HashSet(profile, p =>  p.WValue);
            }
            _mymoooContext.RedisCache.SetTimestamp<WebParamConfig>(timeStamp);
        }

    }
}
