using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
//using Linkage.ERP.Common;

namespace Kingdee.Mymooo.Core.Common
{
    public class CommonApiRequest
    {
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

        /// <summary>
        /// 统一平台
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="signature"></param>
        /// <param name="url"></param>
        /// <param name="paras"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string WorkbenchSignatureInvokeWebService(string timestamp, string signature, string url, string paras = "", string method = "get", string userCode = "", string mymoooCompany = "weixinwork")
        {
            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            //httpWebRequest.ContentType = "charset=UTF-8";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = method;
            httpWebRequest.Timeout = 120000;
            httpWebRequest.ReadWriteTimeout = 120000;
            httpWebRequest.UserAgent = "kyySelection";
            httpWebRequest.Headers.Add("timestamp", timestamp);
            httpWebRequest.Headers.Add("appId", WebUIConfigHelper.WorkbenchForMqMesAppId);//"testmymoooerp"
            httpWebRequest.Headers.Add("signature", signature);
            httpWebRequest.Headers.Add("userCode", userCode);
            httpWebRequest.Headers.Add("mymoooCompany", mymoooCompany);
            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(paras);
                httpWebRequest.ContentLength = (long)bytes.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }
            try
            {
                WebResponse response = httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string json = streamReader.ReadToEnd();
                streamReader.Close();
                return json;
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    //using 
                    Stream data = response.GetResponseStream();
                    //using 
                    StreamReader reader = new StreamReader(data);
                    return reader.ReadToEnd();
                }
                return ex.Message;
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
