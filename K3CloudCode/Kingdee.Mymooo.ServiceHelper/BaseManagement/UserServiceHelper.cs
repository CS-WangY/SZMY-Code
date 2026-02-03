using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Authentication;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Contracts.BaseManagement;

namespace Kingdee.Mymooo.ServiceHelper.BaseManagement
{
    public class UserServiceHelper
    {
        public static UserInfo GetUserInfoForUserID(Context ctx, long supplyOrgId)
        {
            IUserService service = ServiceFactory.GetService<IUserService>(ctx);
            try
            {
                return service.GetUserInfoForUserID(ctx, supplyOrgId);
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
        public static string GetUserWxCode(Context ctx, long userid)
        {
            IUserService service = ServiceFactory.GetService<IUserService>(ctx);
            try
            {
                return service.GetUserWxCode(ctx, userid);
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
