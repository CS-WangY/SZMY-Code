using com.mymooo.workbench.core.Utils;
using Microsoft.Extensions.Options;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    [AutoInject(InJectType.Scope)]
    public class ShunFengClient
    {
        private static ShunFengAccessToken _sfAccessToken;
        private readonly ShunFengExpressageInfo _expressageInfo;

        public ShunFengClient(IOptions<ShunFengExpressageInfo> expressageInfos)
        {
            _expressageInfo = expressageInfos.Value;
        }

        public string InvokeWebService(string serviceCode, string param)
        {
            return UseWework(() =>
            {
                return Execute(serviceCode, param);
            });
        }

        public string Md5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
   
            return Convert.ToBase64String(s);
        }

        private string UseWework(Func<string> action)
        {
            if (string.IsNullOrWhiteSpace(_sfAccessToken?.AccessToken) || _sfAccessToken.ExpiresTime < DateTime.Now)
            {
                AccessToken();
            }

            return action();
        }

        /// <summary>
        /// 跨越物流
        /// </summary>
        /// <param name="expressage"></param>
        /// <returns></returns>
        private void AccessToken()
        {
            HttpWebRequest httpWebRequest = null;
            if (_expressageInfo.TokenUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                httpWebRequest = WebRequest.Create(_expressageInfo.TokenUrl) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(_expressageInfo.TokenUrl);
            }
            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";

            var param = $"partnerID={_expressageInfo.AppKey}&secret={_expressageInfo.AppSecret}&grantType=password";
            byte[] bytes = Encoding.UTF8.GetBytes(param);
            httpWebRequest.ContentLength = (long)bytes.Length;
            using Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            using WebResponse response = httpWebRequest.GetResponse();
            using StreamReader streamReader = new(response.GetResponseStream(), Encoding.UTF8);
            string xml = streamReader.ReadToEnd();
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengAccessToken>(xml);
            if (result.ApiResultCode == "A1000")
            {
                _sfAccessToken = result;
                _sfAccessToken.ExpiresTime = DateTime.Now.AddSeconds(_sfAccessToken.ExpiresIn - 100);
            }
        }

        private string Execute(string serviceCode, string param)
        {
            HttpWebRequest httpWebRequest = null;
            if (_expressageInfo.Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                httpWebRequest = WebRequest.Create(_expressageInfo.Url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(_expressageInfo.Url);
            }
            long timestamp = (long)(DateTime.UtcNow - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalMilliseconds;

            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            param = $"partnerID={_expressageInfo.AppKey}&requestID={Guid.NewGuid()}&timestamp={timestamp}&accessToken={_sfAccessToken.AccessToken}&serviceCode={serviceCode}&msgData={param}";
            if (!string.IsNullOrWhiteSpace(param))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(param);
                httpWebRequest.ContentLength = (long)bytes.Length;
                using Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using WebResponse response = httpWebRequest.GetResponse();
            using StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }
}
