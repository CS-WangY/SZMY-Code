using com.mymooo.workbench.core.Expressage.SuTeng;
using Microsoft.Extensions.Options;
using mymooo.core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    [AutoInject(InJectType.Scope)]
    public class JiaYunMeiClient
    {
        private readonly JiaYunMeiExpressageInfo _expressageInfo;
        public JiaYunMeiClient(IOptions<JiaYunMeiExpressageInfo> expressageInfos)
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
            string sign = Md5(param + _expressageInfo.Key);

            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            param = $"sign={sign}&cooperatorCode={_expressageInfo.CooperatorCode}&data={param}";
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

        public string InvokeWebServicePic(string method, string param)
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
            string sign = Md5(param + _expressageInfo.Key);

            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            param = $"sign={sign}&data={param}&siteCode={_expressageInfo.SendSiteCode}&method=Query_Pic_Info";
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

        public string InvokeWebServiceRouter(string method, string param)
        {
            HttpWebRequest httpWebRequest = null;
            string url = Path.Combine(_expressageInfo.RouterUrl, method);
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
            string sign = Md5(param + _expressageInfo.RouterKey);

            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            param = $"sign={sign}&data={param}";
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

        public WebResponse GetPicByUrl(string url)
        {
            HttpWebRequest httpWebRequest = null;
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
            //httpWebRequest.ContentType = "charset=UTF-8";
            httpWebRequest.ContentType = "application/pdf";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 120000;
            httpWebRequest.ReadWriteTimeout = 120000;
            httpWebRequest.UserAgent = "kyySelection";
            try
            {
                WebResponse response = httpWebRequest.GetResponse();
                return response;
            }
            catch (WebException ex)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
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
