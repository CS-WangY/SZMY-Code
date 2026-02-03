using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration
{
    /// <summary>
    /// 网关请求
    /// </summary>
    public class ApigatewayUtils
    {
        private static DateTime _minDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        public static ApigatewayConfig ApigatewayConfig;

        static ApigatewayUtils()
        {
            ApigatewayConfig = (ConfigurationManager.GetSection("ApigatewayConfiguration") as ApigatewayServiceConfiguration).ApigatewayConfig;
        }

        private static string CreateTimestamp()
        {
            return GetCurrentTimestamp().ToString();
        }

        private static long GetCurrentTimestamp()
        {
            return Convert.ToInt64(DateTime.UtcNow.Subtract(_minDateTime).TotalMilliseconds);
        }

        private static string Sign(string timestamp)
        {
            //拼接签名数据
            var signStr = timestamp + ApigatewayConfig.Nonce;
            //将字符串中字符按升序排序
            var sortStr = string.Concat(signStr.OrderBy(c => c));
            return Encrypt(sortStr);
        }

        private static string Encrypt(string value)
        {
            var encryptedBytes = HMACSHA256Encrypt(value);
            var sBuilder = new StringBuilder();
            foreach (var b in encryptedBytes)
            {
                sBuilder.Append($"{b:x2}");
            }
            return sBuilder.ToString();
        }

        private static IEnumerable<byte> HMACSHA256Encrypt(string value)
        {
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(ApigatewayConfig.Toekn));
            return hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

		public static string InvokePostWebService(string url, string data, string contrnyType = "application/json;charset=UTF-8", string userCode = "")
		{
			HttpWebRequest httpWebRequest = null;
			if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
			{
				ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				httpWebRequest = WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url)) as HttpWebRequest;
				httpWebRequest.ProtocolVersion = HttpVersion.Version10;
			}
			else
			{
				httpWebRequest = (HttpWebRequest)WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url));
			}
			var timestamp = CreateTimestamp();
			var signature = Sign(timestamp);
			httpWebRequest.ContentType = contrnyType;
			httpWebRequest.Method = "post";
			httpWebRequest.UserAgent = "kyySelection";
			httpWebRequest.Timeout = 60000;
			httpWebRequest.ReadWriteTimeout = 60000;
			httpWebRequest.UserAgent = "kyySelection";
			httpWebRequest.Headers.Add("timestamp", timestamp);
			httpWebRequest.Headers.Add("appId", ApigatewayConfig.AppId);
			httpWebRequest.Headers.Add("signature", signature);
			httpWebRequest.Headers.Add("userCode", userCode);
            try
            {
				var byteArray = Encoding.UTF8.GetBytes(data);
				httpWebRequest.ContentLength = byteArray.Length;
				using (var dataStream = httpWebRequest.GetRequestStream())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
				}

				using (WebResponse response = httpWebRequest.GetResponse())
				{
					StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
					string xml = streamReader.ReadToEnd();
					streamReader.Close();
					return xml;
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					return new StreamReader((e.Response as HttpWebResponse).GetResponseStream(), Encoding.UTF8).ReadToEnd();
				}
				else
				{
					throw;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="contrnyType"></param>
		/// <returns></returns>
		public static string InvokeWebService(string url, string contrnyType = "application/json;charset=UTF-8")
        {
            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                httpWebRequest = WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url)) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url));
            }
            var timestamp = CreateTimestamp();
            var signature = Sign(timestamp);
            httpWebRequest.ContentType = contrnyType;
            httpWebRequest.Method = "get";
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Timeout = 60000;
            httpWebRequest.ReadWriteTimeout = 60000;
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("timestamp", timestamp);
            httpWebRequest.Headers.Add("appId", ApigatewayConfig.AppId);
            httpWebRequest.Headers.Add("signature", signature);
            try
            {
                WebResponse response = httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string xml = streamReader.ReadToEnd();
                streamReader.Close();
                return xml;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    return new StreamReader((e.Response as HttpWebResponse).GetResponseStream(), Encoding.UTF8).ReadToEnd();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="contrnyType"></param>
        /// <returns></returns>
        public static string InvokePostRabbitService(string url, string data, string contrnyType = "application/json;charset=UTF-8")
        {
            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                httpWebRequest = WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url + ApigatewayConfig.EnvCode)) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(Path.Combine(ApigatewayConfig.Url, url + ApigatewayConfig.EnvCode));
            }
            var timestamp = CreateTimestamp();
            var signature = Sign(timestamp);
            httpWebRequest.ContentType = contrnyType;
            httpWebRequest.Method = "post";
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Timeout = 60000;
            httpWebRequest.ReadWriteTimeout = 60000;
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("timestamp", timestamp);
            httpWebRequest.Headers.Add("appId", ApigatewayConfig.AppId);
            httpWebRequest.Headers.Add("signature", signature);
            try
            {
                var byteArray = Encoding.UTF8.GetBytes(data);
                httpWebRequest.ContentLength = byteArray.Length;
                using (var dataStream = httpWebRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    string xml = streamReader.ReadToEnd();
                    streamReader.Close();
                    return xml;
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    return new StreamReader((e.Response as HttpWebResponse).GetResponseStream(), Encoding.UTF8).ReadToEnd();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
