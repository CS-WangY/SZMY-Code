using com.mymooo.workbench.qichacha;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace com.mymooo.workbench.core.Utils
{
    [AutoInject(InJectType.Single)]
	public class HttpUtils(IHttpClientFactory httpClientFactory, SignatureUtils signatureUtils)
	{
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly SignatureUtils _signatureUtils = signatureUtils;

		public string InvokePostWebService(string url, string param = "", string contrnyType = "application/json")
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            using HttpContent httpContent = new StringContent(param, Encoding.UTF8, contrnyType);
            using var response = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
                
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <param name="contrnyType"></param>
        /// <returns></returns>
        public T InvokePostWebService<T>(string url, string param = "", string contrnyType = "application/json")
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            using HttpContent httpContent = new StringContent(param, Encoding.UTF8, contrnyType);
            using var response = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                try
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }
                catch
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        public T InvokeGetWebService<T>(string url)
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            using var response = client.GetAsync(url).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                try
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }
                catch
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        public static string QichachaInvokeWebService(string url, string appkey, string secertKey, string method = "get", string param = "", string contrnyType = "application/json;charset=UTF-8")
        {
            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            // set header
            var timestamp = DateTimeUtility.ConvertToTimeStamp(DateTime.Now);
            httpWebRequest.Headers.Add("Timespan", timestamp.ToString());
            httpWebRequest.Headers.Add("Token", QiChaChaMd5Utils.GetHeaderVals(appkey, secertKey, timestamp));
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.ContentType = contrnyType;
            httpWebRequest.Method = method;
            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(param);
                httpWebRequest.ContentLength = (long)bytes.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            try
            {
                using WebResponse response = httpWebRequest.GetResponse();
                using StreamReader streamReader = new(response.GetResponseStream(), Encoding.UTF8);
                string json = streamReader.ReadToEnd();
                streamReader.Close();
                return json;
            }
            catch (WebException ex)
            {
                using HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using Stream data = response.GetResponseStream();
                    using StreamReader reader = new(data);
                    return reader.ReadToEnd();
                }
                return ex.Message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public T SignatureInvokePostPlatformWebService<T>(string token, string nonce, string url, string paras)
        {
            var timestamp = _signatureUtils.CreateTimestamp();
            var signature = _signatureUtils.Sign(token, nonce, timestamp, paras);

            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            using HttpContent httpContent = new StringContent(paras, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("timestamp", timestamp);
            httpContent.Headers.Add("nonce", nonce);
            httpContent.Headers.Add("appId", "workbench");
            httpContent.Headers.Add("signature", signature);
            using var response = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                try
                {
                    return JsonSerializerOptionsUtils.Deserialize<T>(result);
                }
                catch
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        public string SignatureInvokeGetPlatformWebService(string token, string nonce, string url)
        {
            var timestamp = _signatureUtils.CreateTimestamp();
            var signature = _signatureUtils.Sign(token, nonce, timestamp);

            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
			using var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("timestamp", timestamp);
			request.Headers.Add("nonce", nonce);
			request.Headers.Add("appId", "workbench");
			request.Headers.Add("signature", signature);
			var response = client.SendAsync(request).Result;

			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return response.Content.ReadAsStringAsync().Result;
			}
			else
			{
				return JsonSerializerOptionsUtils.Serialize(new
				{
					Code = "httpCodeError",
					ErrorMessage = "请求接口未正确响应"
				});
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="url"></param>
        /// <param name="paras"></param>
        /// <param name="method"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public T SignatureInvokeWebService<T>(string token, string nonce, string url, string paras = "", string userCode = "MoYiFeng", string mymoooCompany = "weixinwork")
        {
            var timestamp = _signatureUtils.CreateTimestamp();
            var signature = _signatureUtils.Sign(token, nonce, timestamp);

            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            using HttpContent httpContent = new StringContent(paras, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("timestamp", timestamp);
            httpContent.Headers.Add("nonce", nonce);
            httpContent.Headers.Add("appId", "workbench");
            httpContent.Headers.Add("signature", signature);
            httpContent.Headers.Add("userCode", userCode);
            httpContent.Headers.Add("mymoooCompany", mymoooCompany);
            using var response = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                try
                {
                    return JsonSerializerOptionsUtils.Deserialize<T>(result);
                }
                catch
				{
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static byte[] DownloadFile(string url)
        {
            // 设置参数
            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            //发送请求并获取相应回应数据
            using HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            using Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            List<byte> bytes = [];
            while (size > 0)
            {
                bytes.AddRange(bArr.Take(size));
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            responseStream.Close();

            return [.. bytes];
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

        public static string CloudstockRequest(string warehouseUrl, string warehouseToken, string path, string data, string companyId = "MYMO", string method = "POST")
        {
            try
            {
                HttpWebResponse response;
                try
                {
                    path = GetPathAndQuery(warehouseToken, path, data);
                    var url = new Uri(new Uri(warehouseUrl), path).ToString();
                    HttpWebRequest httpWebRequest = null;
                    if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                        httpWebRequest.ProtocolVersion = HttpVersion.Version10;
                    }
                    else
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                        httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                    }
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = method;
                    httpWebRequest.Timeout = 30 * 1000;
                    httpWebRequest.ReadWriteTimeout = 30 * 1000;
                    httpWebRequest.Headers.Add("CompanyCode", companyId);

                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    httpWebRequest.ContentLength = (long)bytes.Length;
                    Stream requestStream = httpWebRequest.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                    response = httpWebRequest.GetResponse() as HttpWebResponse;
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }

                if (response == null) return "";
                StreamReader streamReader = new(response.GetResponseStream(), Encoding.UTF8);
                var mssage = streamReader.ReadToEnd();
                streamReader.Close();
                return mssage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetPathAndQuery(string warehouseToken, string path, string data)
        {
            var timestamp = WarehouseApiSignature.CreateTimestamp();
            var nonce = WarehouseApiSignature.CreateNonce();
            var signature = WarehouseApiSignature.Sign(warehouseToken, timestamp, nonce, data);
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["timestamp"] = timestamp;
            query["nonce"] = nonce;
            query["appId"] = "1";
            query["sign"] = signature;
            return path + "?" + query;
        }
    }
}
