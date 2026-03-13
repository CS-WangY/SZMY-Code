using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mymooo.core.Account;
using mymooo.core.Utils;
using mymooo.weixinWork.SDK.AddressBook.Model;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthorizationUserMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, WorkbenchContext workbenchContext, WorkbenchDbContext workbenchDbContext, WeixinWorkUtils weixinWorkUtils)
        {
            var userCode = context.Request.Headers["userCode"].ToString();
            if (!userCode.IsNullOrWhiteSpace() && workbenchContext.User != null)
            {
                var user = workbenchContext.RedisCache.HashGet(new MymoooUser() { UserId = userCode });
                if (user != null)
                {
                    //判断用户是否是管理员
                    var role = workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == user.Id && p.Role.IsAdmin);
                    // 判断是否是备案专员
                    var keeponAttache = workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == user.Id && p.Role.Code == "BusinessData");
                    workbenchContext.User = new User()
                    {
                        Code = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Mobile = user.Mobile,
                        Address = user.Address,
                        MainDepartment = user.MainDepartmentId,
                        ExternalPosition = user.ExternalPosition,
                        Gender = user.Gender,
                        OpenUserId = user.OpenUserid,
                        QrCode = user.QrCode,
                        MymoooCompany = "weixinwork",
                        IsAdmin = role != null,
                        IsKeeponAttache = keeponAttache != null,
                        UserId = user.Id
                    };
                }
            }
            await _next(context);
        }
    }
}
