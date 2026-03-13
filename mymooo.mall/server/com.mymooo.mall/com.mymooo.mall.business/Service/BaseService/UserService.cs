using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Attributes;
using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using mymooo.core.Attributes;

namespace com.mymooo.mall.business.Service.BaseService
{
	[AutoInject(InJectType.Scope)]
	public class UserService(MallContext mymoooContext)
	{
		private readonly MallContext _mymoooContext = mymoooContext;

		public void ReloadCache()
		{
			var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
			var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<ManagementUser>();
			var filter = " [RowVersion] <= @EndTimeStamp";
			if (startTimeStamp != null)
			{
				filter += " and [RowVersion] > @StartTimeStamp ";
			}
			int pageIndex = 1;
			var users = _mymoooContext.SqlSugar.Queryable<ManagementUser>().Includes(p => p.UserInfo).Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).OrderBy(p => p.UserId).ToOffsetPage(pageIndex, 100);
			while (users.Count > 0)
			{
				foreach (var user in users)
				{
					_mymoooContext.RedisCache.HashSet(user);
				}
				pageIndex++;
				users = _mymoooContext.SqlSugar.Queryable<ManagementUser>().Includes(p => p.UserInfo).Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).OrderBy(p => p.UserId).ToOffsetPage(pageIndex, 100);
			}
			_mymoooContext.RedisCache.SetTimestamp<ManagementUser>(timeStamp);
		}


		/// <summary>
		/// 记录用户行为日志,用于性能分析
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
        public string SysLog(SysLogReq req)
        {
            if (req == null)
            {
                return string.Empty;
            }
            _mymoooContext.SqlSugar.Insertable<LogUserAction>(req).ExecuteCommand();
            return "ok";

        }
    }
}
