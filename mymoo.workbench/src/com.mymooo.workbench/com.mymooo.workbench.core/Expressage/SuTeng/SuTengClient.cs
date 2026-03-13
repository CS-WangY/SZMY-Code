using Microsoft.Extensions.Options;
using mymooo.core.Attributes;
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

namespace com.mymooo.workbench.core.Expressage.SuTeng
{
    [AutoInject(InJectType.Scope)]
    public class SuTengClient
    {
        private readonly SuTengExpressageInfo _expressageInfo;

        public SuTengClient(IOptions<SuTengExpressageInfo> expressageInfos)
        {
            _expressageInfo = expressageInfos.Value;
        }

        public string InvokeWebService(string method, string param)
        {
            HttpWebRequest httpWebRequest = null;
            string url = Path.Combine(_expressageInfo.Url, method);
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            //获取签名
            long timestamp = (long)(DateTime.UtcNow - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalMilliseconds;
            var nonce = Guid.NewGuid().ToString("N");
            string sign = Md5(_expressageInfo.AppId + nonce + timestamp.ToString() + _expressageInfo.AppKey );

            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("appid", _expressageInfo.AppId);
            httpWebRequest.Headers.Add("nonce", nonce);
            httpWebRequest.Headers.Add("timestamp", timestamp.ToString());
            httpWebRequest.Headers.Add("sign", sign);
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

        public string Md5(string str)
        {
            string req = "";
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < s.Length; i++)
            {
                req = req + s[i].ToString("X2").ToLower();
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
