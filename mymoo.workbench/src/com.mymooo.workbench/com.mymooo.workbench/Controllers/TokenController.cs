using com.mymooo.workbench.business.Account;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class TokenController(TokenService tokenService, WeixinWorkUtils weixinWorkUtils, WorkbenchDbContext workbenchDbContext, WorkbenchContext workbenchContext) : Controller
    {
        private readonly TokenService _tokenService = tokenService;
        private readonly WeixinWorkUtils _weixinWorkUtils = weixinWorkUtils;
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

        /// <summary>
        /// 蚂蚁后台不能直接读取到redis非默认端口号的数据,所以这里提供这个接口,仅供蚂蚁后台调用
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetUser(string token)
        {
            ResponseMessage<User> response = new()
            {
                Data = await _workbenchContext.RedisCache.HashGetAsync(new User() { Token = token })
            };
            return Json(response);
        }

        /// <summary>
        /// 蚂蚁后台不能直接读取到redis非默认端口号的数据,所以这里提供这个接口,仅供蚂蚁后台调用
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SetUserToken([FromBody] User user)
        {
            await _workbenchContext.RedisCache.HashSetAsync(user, new TimeSpan(0, 1000, 0));
            return Ok();
        }

        public IActionResult Verify(string token)
        {
            return Json(_tokenService.Verify(token));
        }

        public IActionResult VerifyAndLogin(string token)
        {
            var result = _tokenService.VerifyAndLogin(token);
            if (result.Code == ResponseCode.Success)
            {
                return base.Json(new ResponseMessage<User>() { Code = ResponseCode.Success, Data = result.Data });
            }
            else
            {
                return Json(result);
            }
        }

        public IActionResult GetUserInfo(string userCode, string mymoooCompany = "weixinwork")
        {
            return Json(_tokenService.GetUserInfo(userCode, mymoooCompany));
        }

        public IActionResult TokenOrUsercodeLogin(string appId, string keyword, string mymoooCompany = "weixinwork")
        {
            if (string.IsNullOrWhiteSpace(mymoooCompany))
            {
                mymoooCompany = "weixinwork";
            }
            var result = _tokenService.Verify(keyword);
            string userCode = keyword;
            string token = keyword;
            var app = _workbenchDbContext.ThirdpartyApplicationConfig.FirstOrDefault(c => c.AppId == appId);
            if (result.Code == ResponseCode.TokenInvalid)
            {
                if (app == null)
                {
                    result.Code = ResponseCode.NoThirdConfig;
                    result.ErrorMessage = "没有第三方app配置信息";
                    return Json(result);
                }
            }
            else
            {
                userCode = result.Data.UserCode;
            }

            var user = _tokenService.GetUserInfo(userCode, mymoooCompany);
            if (user.Code == ResponseCode.Success)
            {
                if (result.Code == ResponseCode.TokenInvalid)
                {
                    //创建token
                    ef.AccountContext.UserToken userToken = new()
                    {
                        Token = Guid.NewGuid().ToString("N"),
                        AppId = app.AppId,
                        MymoooCompany = mymoooCompany,
                        Validity = app.Validity,
                        LoginDate = DateTime.Now,
                        FailureDate = DateTime.Now.AddMinutes(app.Validity),
                        UserCode = user.Data.Code,
                        Ip = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        UserAgent = Request.Headers["User-Agent"],
                        UserId = user.Data.UserId
                    };
                    _workbenchDbContext.UserToken.Add(userToken);
                    _workbenchDbContext.SaveChanges();
                    token = userToken.Token;
                }
                user.Data.Token = token;
                user.Data.AppId = appId;
                return Json(user);
            }
            else
            {
                return Json(user);
            }
        }

        public IActionResult Logout(string token)
        {
            var tokenInfo = _workbenchDbContext.UserToken.FirstOrDefault(c => c.Token == token);
            if (tokenInfo != null)
            {
                tokenInfo.FailureDate = DateTime.Now.AddDays(-1);
                _workbenchDbContext.SaveChanges();
            }
            ResponseMessage<string> message = new ResponseMessage<string>();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
    }
}
