using com.mymooo.workbench.business.Account;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.Models;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Config;
using mymooo.core.Model;
using mymooo.core.Model.Gateway;
using mymooo.core.Security;
using mymooo.core.Utils;
using mymooo.weixinWork.SDK.Application;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// home
    /// </summary>
    public class HomeController(IOptions<RsaConfig> rsaConfig, WorkbenchDbContext workbenchDbContext, WeixinWorkUtils weixinWorkUtils,
        IConfiguration configuration, TokenService tokenService, WorkbenchContext workbenchContext, ApplicationServiceClient applicationServiceClient) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WeixinWorkUtils _weixinWorkUtils = weixinWorkUtils;
        private readonly IConfiguration _configuration = configuration;
        private readonly TokenService _tokenService = tokenService;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;
        private readonly RsaConfig _rsaConfig = rsaConfig.Value;

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return Redirect("/html5/#/dashboard");
            //ViewBag.Menus = _workbenchDbContext.ThirdpartyApplicationConfig.Include(c => c.Menu).ToList();
            //return View();
        }

        /// <summary>
        /// 企业微信扫描登录
        /// </summary>
        /// <param name="appId">那个应用调用企业微信扫码登录</param>
        /// <param name="mymoooCompany">用哪个企业微信扫码登录(深圳蚂蚁,东莞蚂蚁)</param>
        /// <param name="redirectUri">回调地址</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Login(string appId = "", string mymoooCompany = "weixinwork", string redirectUri = "")
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                appId = _workbenchContext.ApigatewayConfig.SystemEnvCode;
            }
            if (!redirectUri.IsNullOrWhiteSpace())
            {
                redirectUri = HttpUtility.UrlEncode(redirectUri);
            }
            var wexinConfig = new SystemEnvironmentConfig() { SystemConfig = new SystemConfig() { SystemCode = "weixinwork-Application" }, EnvironmentConfig = new EnvironmentConfig() { EnvCode = _workbenchContext.ApigatewayConfig.EnvCode } };
            var weixinSystem = _workbenchContext.GatewayRedisCache.HashGet(wexinConfig);
            if (weixinSystem == null)
            {
                wexinConfig.EnvironmentConfig.EnvCode = "develop";
                weixinSystem = _workbenchContext.GatewayRedisCache.HashGet(wexinConfig);
            }
            var systemConfig = _workbenchContext.GatewayRedisCache.HashGet(new SystemEnvironmentConfig() { SystemEnvCode = appId }, "system");
            if (systemConfig == null)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = $"{appId}应用不存在 回调参数:{redirectUri}" });
            }
            if (_workbenchContext.User == null)
            {

                string userAgent = Request.Headers.UserAgent;
                var workbenchConfig = new SystemEnvironmentConfig() { SystemConfig = new SystemConfig() { SystemCode = "workbench" }, EnvironmentConfig = new EnvironmentConfig() { EnvCode = _workbenchContext.ApigatewayConfig.EnvCode } };
                var workbenchSystem = _workbenchContext.GatewayRedisCache.HashGet(workbenchConfig);
                if (workbenchSystem == null)
                {
                    workbenchConfig.EnvironmentConfig.EnvCode = "develop";
                    workbenchSystem = _workbenchContext.GatewayRedisCache.HashGet(workbenchConfig);
                }
                var callbakUrl = HttpUtility.UrlEncode($"{workbenchSystem.Url}home/WeiXinWorklogin?redirectUri={redirectUri}&mymoooCompany={mymoooCompany}");
                if (!string.IsNullOrWhiteSpace(userAgent) && userAgent.Contains("micromessenger", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(redirectUri) || !redirectUri.StartsWith("http") || redirectUri.StartsWith(systemConfig.Url))
                    {
                        return Redirect($"https://open.weixin.qq.com/connect/oauth2/authorize?appid={weixinSystem.SystemConfig.EnterpriseId}&redirect_uri={callbakUrl}&response_type=code&scope=snsapi_base&state={appId}#wechat_redirect");
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { errorMessage = "应用的url和回调的地址不匹配" });
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(redirectUri) || !redirectUri.StartsWith("http") || redirectUri.StartsWith(systemConfig.Url))
                    {
                        //ViewBag.weixinwork = systemConfig;
                        //ViewBag.DGweixinwork = _workbenchDbContext.ThirdpartyApplicationDetail.Include(c => c.ThirdpartyApplicationConfig).FirstOrDefault(a => a.DetailCode == WeixinWorkAppName.Application && a.AppId == _appSettingsUtils.GetEvironmentValue() + "DGweixinwork");
                        ViewBag.AppId = appId;
                        //ViewBag.mymoooCompany = mymoooCompany;
                        ViewBag.weixinworkCallbakUrl = callbakUrl;

                        return View(weixinSystem);
                        //return Redirect($"https://open.work.weixin.qq.com/wwopen/sso/qrConnect?appid={weixinSystem.SystemConfig.EnterpriseId}&agentid={weixinSystem.Token}&redirect_uri={callbakUrl}&state={appId}");
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { errorMessage = "应用的url和回调的地址不匹配" });
                    }
                }
            }
            else
            {
                if (appId.EndsWith("workbench", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(redirectUri))
                    {
                        return Redirect("/html5/#/dashboard");

                    }
                    else
                    {
                        return Redirect($"/html5/#{redirectUri}");
                        //return Redirect("http://localhost:8080/#/dashboard");//本地调试
                    }
                }
                else
                {
                    return Redirect($"{systemConfig.Url}Account/SignLogin?titck={HttpUtility.UrlEncode(RSACryptography.RsaKeyEncrypt(_workbenchContext.RsaConfig.PublicKey, _workbenchContext.User.Token))}&redirectUri={redirectUri}");
                }
            }
        }

        /// <summary>
        /// 手机企业微信登录
        /// </summary>
        /// <param name="appId">那个应用调用企业微信扫码登录</param>
        /// <param name="mymoooCompany">用哪个企业微信登录(深圳蚂蚁,东莞蚂蚁)</param>
        /// <param name="redirectUri">回调地址</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult WeChatMobileLogin(string appId = "", string mymoooCompany = "weixinwork", string redirectUri = "")
        {
            if (!redirectUri.IsNullOrWhiteSpace())
            {
                redirectUri = HttpUtility.UrlEncode(redirectUri);
            }
            var wexinConfig = new SystemEnvironmentConfig() { SystemConfig = new SystemConfig() { SystemCode = "weixinwork-Application" }, EnvironmentConfig = new EnvironmentConfig() { EnvCode = _workbenchContext.ApigatewayConfig.EnvCode } };
            var weixinSystem = _workbenchContext.GatewayRedisCache.HashGet(wexinConfig);
            if (weixinSystem == null)
            {
                wexinConfig.EnvironmentConfig.EnvCode = "develop";
                weixinSystem = _workbenchContext.GatewayRedisCache.HashGet(wexinConfig);
            }
            var callbakUrl = HttpUtility.UrlEncode($"{Request.Scheme}://{Request.Host}/home/WeiXinWorklogin?redirectUri={redirectUri}&mymoooCompany={mymoooCompany}");
            if (string.IsNullOrWhiteSpace(appId))
            {
                appId = _workbenchContext.ApigatewayConfig.SystemEnvCode;
            }
            var systemConfig = _workbenchContext.GatewayRedisCache.HashGet(new SystemEnvironmentConfig() { SystemEnvCode = appId }, "system");
            if (systemConfig == null)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = $"{appId}应用不存在 回调参数:{redirectUri}" });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(redirectUri) || !redirectUri.StartsWith("http") || redirectUri.StartsWith(systemConfig.Url))
                {
                    return Redirect($"https://open.weixin.qq.com/connect/oauth2/authorize?appid={weixinSystem.SystemConfig.EnterpriseId}&redirect_uri={callbakUrl}&response_type=code&scope=snsapi_base&state={appId}#wechat_redirect");
                }
                else
                {
                    return RedirectToAction("Error", "Home", new { errorMessage = "应用的url和回调的地址不匹配" });
                }
            }
        }

        /// <summary>
        /// 企业微信扫码登录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="mymoooCompany"></param>
        /// <param name="redirectUri"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> WeiXinWorklogin(string code, string state, string mymoooCompany = "weixinwork", string redirectUri = "")
        {
            var systemConfig = _workbenchContext.GatewayRedisCache.HashGet(new SystemEnvironmentConfig() { SystemEnvCode = state }, "system");
            if (systemConfig == null)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = $"{state}应用不存在 回调参数:{redirectUri}" });
            }
            var userid = await _applicationServiceClient.GetUserId<WorkbenchContext, User>(_workbenchContext, code);
            if (userid.IsSuccess && userid.Data.Errcode == 0)
            {
                if (string.IsNullOrWhiteSpace(userid.Data.UserId))
                {
                    userid = await _applicationServiceClient.OpenIdConvertUserId<WorkbenchContext, User>(_workbenchContext, userid.Data.OpenId);
                }
                //调用企业微信,查询用户信息
                var employeInfo = _weixinWorkUtils.GetUserInfo(userid.Data.UserId, mymoooCompany);
                //判断用户是否是管理员
                var role = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.IsAdmin);
                // 判断是否是备案专员
                var keeponAttache = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.Code == "BusinessData");

                if (employeInfo.Errcode != 0)
                {
                    return RedirectToAction("login", "Home");
                }
                //创建token
                ef.AccountContext.UserToken userToken = new()
                {
                    Token = Guid.NewGuid().ToString("N"),
                    AppId = systemConfig.SystemEnvCode,
                    Validity = systemConfig.Validity,
                    LoginDate = DateTime.Now,
                    FailureDate = DateTime.Now.AddMinutes(systemConfig.Validity),
                    UserCode = employeInfo.UserId,
                    UserId = employeInfo.Id,
                    Ip = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    MymoooCompany = mymoooCompany,
                    UserAgent = Request.Headers.UserAgent
                };
                _workbenchDbContext.UserToken.Add(userToken);
                _workbenchDbContext.SaveChanges();
                _workbenchContext.User = new User()
                {
                    Token = userToken.Token,
                    Code = employeInfo.UserId,
                    Name = employeInfo.Name,
                    Email = employeInfo.Email,
                    Mobile = employeInfo.Mobile,
                    Address = employeInfo.Address,
                    MainDepartment = employeInfo.Main_department,
                    ExternalPosition = employeInfo.ExternalPosition,
                    Gender = employeInfo.Gender,
                    OpenUserId = employeInfo.OpenUserId,
                    QrCode = employeInfo.QrCode,
                    SystemCode = systemConfig.SystemConfig.SystemCode,
                    AppId = state,
                    UserType = SystemUserType.actual,
                    MymoooCompany = mymoooCompany,
                    IsAdmin = role != null,
                    IsKeeponAttache = keeponAttache != null,
                    UserId = employeInfo.Id
                };
                _workbenchContext.RedisCache.HashSet(_workbenchContext.User, new TimeSpan(0, systemConfig.Validity, 0)); 
                HttpContext.User = new MymoooPrincipal<User>(new GenericIdentity(userid.Data.UserId, CookieAuthenticationDefaults.AuthenticationScheme)) { User = _workbenchContext.User };
                HttpContext.Response.Cookies.Append("mymooo_mall", RSACryptography.RsaKeyEncrypt(_workbenchContext.RsaConfig.PublicKey, _workbenchContext.User.Token));

                if (state.EndsWith("workbench", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(redirectUri))
                    {
                        return Redirect("/html5/#/dashboard");

                    }
                    else
                    {
                        return Redirect($"/html5/#{redirectUri}");
                        //return Redirect("http://localhost:8080/#/dashboard");//本地调试
                    }
                }
                else
                {
                    if (!redirectUri.IsNullOrWhiteSpace())
                    {
                        redirectUri = HttpUtility.UrlEncode(redirectUri);
                    }
                    return Redirect($"{systemConfig.Url}Account/SignLogin?titck={HttpUtility.UrlEncode(RSACryptography.RsaKeyEncrypt(_workbenchContext.RsaConfig.PublicKey, _workbenchContext.User.Token))}&redirectUri={redirectUri}");
                }
            }
            else
            {
                return RedirectToAction("login", "Home");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// 错误页
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error(string errorMessage = "")
        {
            if (HttpContext.Request.ContentType != null && HttpContext.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                ResponseMessage<string> result = new()
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? $"内部错误,错误代码:{HttpContext.TraceIdentifier}" : errorMessage,
                    Code = ResponseCode.Exception
                };
                return Json(result);
            }
            ViewBag.Error = errorMessage;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// 获取当前用户
        /// </summary>
        /// <returns></returns>
        public IActionResult CurrentUser()
        {
            ResponseMessage<User> message = new()
            {
                Data = _workbenchContext.User,
                Code = ResponseCode.Success
            };
            return Json(message);
        }

        /// <summary>
        /// 模糊查询员工信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ActionResult FuzzyQuery(string code, int count = 5)
        {
            ResponseMessage<dynamic> message = new();
            string mymoooCompany = _workbenchContext.User.MymoooCompany;
            var response = QueryUser(code, mymoooCompany, count);
            message.Data = response;
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        private List<dynamic> QueryUser(string code, string mymoooCompany, int count)
        {
            List<dynamic> result = [];
            var mymoooConnection = _configuration.GetConnectionString("SqlServerConnection");
            string sql = $@"SELECT top {count} u.*
                                       FROM MymoooUser AS u
                                       WHERE (u.UserId like @Code or u.Name like @Code) and u.AppId = @AppId order by u.AppId";
            using (var dr = SqlDbService.ExecuteReader(mymoooConnection, sql, new SqlParameter("@Code", $"%{code}%"), new SqlParameter("@AppId", $"%{mymoooCompany}%")))
            {
                while (dr.Read())
                {
                    result.Add(new
                    {
                        Name = dr["UserId"].ToString(),
                        Code = dr["Name"].ToString()
                    });
                }
            }
            return result;
        }
    }
}
