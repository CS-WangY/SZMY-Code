using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Utils
{
    public class K3CloudUtils
    {
        private static DateTime _minDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        private static long GetCurrentTimestamp()
        {
            return Convert.ToInt64(DateTime.UtcNow.Subtract(_minDateTime).TotalMilliseconds) / 1000;
        }
        /// <summary>
        /// 调用金蝶服务
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string K3cloudWebService(string url, string data, K3CloudConfig k3CloudConfig)
        {
            url = k3CloudConfig.Url + url;
            var timestamp = GetCurrentTimestamp().ToString();
            string[] arr = new string[] { k3CloudConfig.DataCenterNumber, k3CloudConfig.Username, k3CloudConfig.AppId, k3CloudConfig.AppSecret, timestamp };
            Array.Sort(arr, StringComparer.Ordinal);
            string sortdata = string.Join(string.Empty, arr);
            var signedData = KDSHA256.Sha256Hex(sortdata);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("timestamp", timestamp);
            request.Headers.Add("dataCenterNumber", k3CloudConfig.DataCenterNumber);
            request.Headers.Add("appId", k3CloudConfig.AppId);
            request.Headers.Add("signature", signedData);
            request.Method = "get";
            request.ReadWriteTimeout = 600000;
            if (!string.IsNullOrWhiteSpace(data))
            {
                request.Method = "POST";
                var byteArray = Encoding.UTF8.GetBytes(data);
                request.ContentLength = byteArray.Length;
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(dataStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }

                return ex.Message;
            }
        }
    }
}
