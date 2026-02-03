using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Log;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
	/// <summary>
	///  供应商小类检验评分-周合格率统计
	/// </summary>
	public class WeeklyPassRateStatisticsService : IScheduleService
	{
		public void Run(Context ctx, Schedule schedule)
		{
			var context = LoginServiceUtils.BackgroundLogin(ctx);
			var logs = new List<LogObject>();
			try
			{
				ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
				BasicDataSyncServiceHelper.WeeklyPassRateStatisticsService(context);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
	}
}
