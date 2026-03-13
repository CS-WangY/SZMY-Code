using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using mymooo.core.Attributes;

namespace com.mymooo.mall.business.Service.SystemService
{
	[AutoInject(InJectType.Scope)]
	public class SystemProfileService(MallContext mymoooContext)
	{
		private readonly MallContext _mymoooContext = mymoooContext;

		public void ReloadCache()
		{
			var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
			var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<SystemProfile>();
			var filter = " [RowVersion] <= @EndTimeStamp";
			if (startTimeStamp != null)
			{
				filter += " and [RowVersion] > @StartTimeStamp ";
			}
			var profiles = _mymoooContext.SqlSugar.Queryable<SystemProfile>().Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).ToList();
			foreach (var profile in profiles)
			{
				_mymoooContext.RedisCache.HashSet(profile, p => p.Value);
			}
			_mymoooContext.RedisCache.SetTimestamp<SystemProfile>(timeStamp);
		}
	}
}
