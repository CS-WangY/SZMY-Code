using Kingdee.BOS;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.Performance.Common;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.K3CloudConfiguration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kingdee.Mymooo.Core.Utils
{
    public class LoginServiceUtils
    {
        static K3CloudServiceConfiguration _k3CloudConfig;
        static LoginServiceUtils()
        {
            _k3CloudConfig = ConfigurationManager.GetSection("K3CloudConfiguration") as K3CloudServiceConfiguration;
        }

        public static Context BackgroundLogin(Context ctx)
        {
            var timestamp = DateTime.Now.GetTimestamp();
            string[] arr = new string[] { ctx.DBId, "demo", _k3CloudConfig.K3CloudConfig.AppId, _k3CloudConfig.K3CloudConfig.AppSecret, timestamp.ToString() };
            Array.Sort(arr, StringComparer.Ordinal);
            string sortdata = string.Join(string.Empty, arr);
            var signature = KDSHA256.Sha256Hex(sortdata);

            LoginInfo loginInfo = new LoginInfo
            {
                AcctID = ctx.DBId,
                AuthenticateType = AuthenticationType.SimplePassportAuthentication,
                Username = "demo",
                Password = "888888",
                Lcid = 2052,
                AppId = _k3CloudConfig.K3CloudConfig.AppId,
                Timestamp = Convert.ToInt64(timestamp),
                SignedData = signature,
                LoginType = LoginType.NormalERPLogin
            };
            LoginResult loginResult = LoginServiceHelper.Login(PerformanceContext.Create(PerfArgsCollectionType.CallDirectly), "", loginInfo);

            if (loginResult.LoginResultType == LoginResultType.Success || loginResult.LoginResultType == LoginResultType.Wanning || loginResult.LoginResultType == LoginResultType.DealWithForm)
            {
                Context result = loginResult.Context;

                //设置客户端信息，用于凭证网控
                SetRequestClientInfo(result);

                if (result.CurrentOrganizationInfo == null || result.CurrentOrganizationInfo.ID == 0)
                {
                    result.CurrentOrganizationInfo = new OrganizationInfo()
                    {
                        ID = 1,
                        Name = "深圳范思德科技有限公司",
                        AcctOrgType = "1",
                        FunctionIds = new List<long> { 101, 102, 103, 104, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115 }
                    };
                }

                return result;
            }

            return null;
        }

        public static Context SignLogin(HttpRequest request)
        {
            var timestamp = request.Headers["timestamp"] ?? DateTime.Now.GetTimestamp().ToString();
            var dataCenterNumber = request.Headers["dataCenterNumber"] ?? "65dc4566411eaf";
            var appId = request.Headers["appId"] ?? "235451_362M0wEv6JlbQZUEXZ3D2xUtyv0WSDLp";
            var signature = request.Headers["signature"];
            if (signature.IsNullOrEmptyOrWhiteSpace())
            {
#if DEBUG
                string[] arr = new string[] { dataCenterNumber, "demo", appId, "ccfcb33437fd4f599db307d3fb3c5c60", timestamp };
                Array.Sort(arr, StringComparer.Ordinal);
                string sortdata = string.Join(string.Empty, arr);
                signature = KDSHA256.Sha256Hex(sortdata);
#endif
            }

            LoginInfo loginInfo = new LoginInfo
            {
                AcctID = dataCenterNumber,
                AuthenticateType = AuthenticationType.SimplePassportAuthentication,
                Username = "demo",
                Password = "888888",
                Lcid = 2052,
                AppId = appId,
                Timestamp = Convert.ToInt64(timestamp),
                SignedData = signature,
                LoginType = LoginType.NormalERPLogin
            };
            LoginResult loginResult = LoginServiceHelper.Login(PerformanceContext.Create(PerfArgsCollectionType.CallDirectly), "", loginInfo);

            if (loginResult.LoginResultType == LoginResultType.Success || loginResult.LoginResultType == LoginResultType.Wanning || loginResult.LoginResultType == LoginResultType.DealWithForm)
            {
                Context result = loginResult.Context;

                //设置客户端信息，用于凭证网控
                SetRequestClientInfo(result);

                if (result.CurrentOrganizationInfo == null || result.CurrentOrganizationInfo.ID == 0)
                {
                    result.CurrentOrganizationInfo = new OrganizationInfo()
                    {
                        ID = 1,
                        Name = "深圳范思德科技有限公司",
                        AcctOrgType = "1",
                        FunctionIds = new List<long> { 101, 102, 103, 104, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115 }
                    };
                }
                return result;
            }

            throw new Exception($"登录失败!数据中心:{request.Headers["dataCenterNumber"]}/{dataCenterNumber} 消息:{loginResult.Message}  状态:{loginResult.LoginResultType}");
        }

        /// <summary>
        /// 设置客户端信息，用于凭证网控
        /// </summary>
        /// <param name="ctx"></param>
        private static void SetRequestClientInfo(Context ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.ComputerName) && HttpContext.Current != null && HttpContext.Current.User != null &&
                    HttpContext.Current.User.Identity != null)
            {
                ctx.ComputerName = HttpContext.Current.User.Identity.Name;
            }

            if (string.IsNullOrWhiteSpace(ctx.ComputerName))
            {
                ctx.ComputerName = ctx.LoginName;
            }

            //if (string.IsNullOrWhiteSpace(ctx.IpAddress) && HttpContext.Current != null)
            //{
            //    ctx.IpAddress = HttpContext.Current.Request.UserHostAddress;
            //}

            //避免null
            ctx.IpAddress = ctx.IpAddress + "";
        }
    }
}
