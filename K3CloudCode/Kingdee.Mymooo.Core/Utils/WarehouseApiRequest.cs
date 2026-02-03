using Kingdee.BOS.Core.Permission;
using Kingdee.Mymooo.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kingdee.Mymooo.Core.Utils
{
    public class WarehouseApiRequest
    {
        public readonly static string CloudStockUrl = System.Configuration.ConfigurationManager.AppSettings["cloudStockUrl"];

        public readonly static string CloudStockToken = "9D841AB1C71C32D2C86E8FE1E9490FD9";

        public static string Request(string warehouseUrl, string warehouseToken, string path, string data, string companyId = "MYMO", string method = "POST")
        {
            try
            {
                HttpWebResponse response;
                try
                {
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
                    var timestamp = WarehouseApiSignature.CreateTimestamp();
                    var nonce = WarehouseApiSignature.CreateNonce();
                    var signature = WarehouseApiSignature.Sign(warehouseToken, timestamp, nonce, "");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = method;
                    httpWebRequest.Timeout = 30 * 1000;
                    httpWebRequest.ReadWriteTimeout = 30 * 1000;
                    httpWebRequest.Headers.Add("timestamp", timestamp);
                    httpWebRequest.Headers.Add("nonce", nonce);
                    httpWebRequest.Headers.Add("signature", signature);
                    httpWebRequest.Headers.Add("CompanyCode", companyId);
                    httpWebRequest.Headers.Add("appId", "kingdee");

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
                StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var mssage = streamReader.ReadToEnd();
                streamReader.Close();
                return JsonConvertUtils.SerializeObject(new MessageHelp()
                {
                    IsSuccess = (response.StatusCode == HttpStatusCode.OK ||
                                 response.StatusCode == HttpStatusCode.Created),
                    Message = mssage,
                    Data = Convert.ToInt32(response.StatusCode)
                });
            }
            catch (Exception e)
            {
                return JsonConvertUtils.SerializeObject(new MessageHelp() { IsSuccess = false, Message = e.ToString() });
            }
        }

        public static string RequestData(string warehouseUrl, string warehouseToken, string path, string data, string companyId = "MYMO", string method = "POST")
        {
            try
            {
                HttpWebResponse response;
                try
                {
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
                    var timestamp = WarehouseApiSignature.CreateTimestamp();
                    var nonce = WarehouseApiSignature.CreateNonce();
                    var signature = WarehouseApiSignature.Sign(warehouseToken, timestamp, nonce, "");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = method;
                    httpWebRequest.Timeout = 30 * 1000;
                    httpWebRequest.ReadWriteTimeout = 30 * 1000;
                    httpWebRequest.Headers.Add("timestamp", timestamp);
                    httpWebRequest.Headers.Add("nonce", nonce);
                    httpWebRequest.Headers.Add("signature", signature);
                    httpWebRequest.Headers.Add("CompanyCode", companyId);
                    httpWebRequest.Headers.Add("appId", "kingdee");

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

                if (response == null)  return JsonConvertUtils.SerializeObject(new MessageHelp() { IsSuccess = false, Message = "云存储请求错误" }) ;
                StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var mssage = streamReader.ReadToEnd();
                streamReader.Close();
                return mssage;
            }
            catch (Exception e)
            {
                return JsonConvertUtils.SerializeObject(new MessageHelp() { IsSuccess = false, Message = e.ToString() });
            }
        }
    }
}
