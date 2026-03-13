using Microsoft.Extensions.Options;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    [AutoInject(InJectType.Scope)]
	public class KuayueClient(IOptions<KuayueExpressageInfo> expressageInfos)
	{
        private static KyeAccessToken _kyeAccessToken;
        private static readonly object _lockObject = new();
        private readonly KuayueExpressageInfo _expressageInfo = expressageInfos.Value;

		public string InvokeWebService(string method, string param)
        {
            return UseWework(() =>
            {
                return Execute(method, param);
            });
        }

        private string UseWework(Func<string> action)
        {
            if (string.IsNullOrWhiteSpace(_kyeAccessToken?.Token) || _kyeAccessToken.ExpiresTime < DateTime.Now)
            {
                lock(_lockObject)
                {
                    if (string.IsNullOrWhiteSpace(_kyeAccessToken?.Token) || _kyeAccessToken.ExpiresTime < DateTime.Now)
                    {
                        AccessToken();
                    }
                }
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
            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("X-from", "openapi_app");

            var param = JsonSerializerOptionsUtils.Serialize(new { appkey = _expressageInfo.AppKey, appsecret = _expressageInfo.AppSecret});
            byte[] bytes = Encoding.UTF8.GetBytes(param);
            httpWebRequest.ContentLength = (long)bytes.Length;
            using Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            using WebResponse response = httpWebRequest.GetResponse();
            using StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string xml = streamReader.ReadToEnd();
           var result = JsonSerializerOptionsUtils.Deserialize<AccessTokenKyeResponse>(xml);
            if (result.Success)
            {
                _kyeAccessToken = result.Data;
                _kyeAccessToken.ExpiresTime = DateTime.Now.AddSeconds(_kyeAccessToken.Expire_time - 100);
            }
        }

        private string Execute(string method, string param)
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
            //获取签名
            long timestamp = (long)(DateTime.UtcNow - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalMilliseconds;
            string sign = Md5(_expressageInfo.AppSecret + timestamp.ToString() + param);

            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("appkey", _expressageInfo.AppKey);
            httpWebRequest.Headers.Add("method", method);
            httpWebRequest.Headers.Add("X-from", "openapi_app");
            httpWebRequest.Headers.Add("sign", sign);
            httpWebRequest.Headers.Add("timestamp", timestamp.ToString());
            httpWebRequest.Headers.Add("token", _kyeAccessToken.Token);
            if (!string.IsNullOrWhiteSpace(param))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(param);
                httpWebRequest.ContentLength = (long)bytes.Length;
                using Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using WebResponse response = httpWebRequest.GetResponse();
            using StreamReader streamReader = new(response.GetResponseStream(), Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        public static string Md5(string str)
		{
			string req = "";
			byte[] s = MD5.HashData(Encoding.UTF8.GetBytes(str));
			for (int i = 0; i < s.Length; i++)
			{
				req += s[i].ToString("X2");
			}
			return req;
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
