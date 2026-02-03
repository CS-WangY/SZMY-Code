using Kingdee.BOS;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServiceHelper
{
    public class DemoTestServiceServiceHelper
    {
		public static void DemoTestAction(Context ctx)
		{
			IDemoTestService service = ServiceFactory.GetService<IDemoTestService>(ctx);
			try
			{
				service.DemoTestAction(ctx);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				ServiceFactory.CloseService(service);
			}
		}
	}
}
