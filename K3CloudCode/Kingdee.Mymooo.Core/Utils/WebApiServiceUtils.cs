using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace Kingdee.Mymooo.Core.Utils
{
    public static class WebApiServiceUtils
    {
        /// <summary>
        /// 云平台地址
        /// </summary>
        //public readonly static string DispatchToCloudUrl = ConfigurationManager.AppSettings["DispatchToCloudUrl"];

        public static string HttpPost(string url, object Class)
        {
            string result = "";
            string AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJDbGFpbURhdGEiOnsiSWQiOm51bGwsIkhlYWRJbWFnZVVybCI6bnVsbCwiTmFtZSI6IuWGhemDqOS4k-eUqFRva2VuIiwiU2V4IjpudWxsLCJQcm92aW5jZUlkIjowLCJQcm92aW5jZSI6bnVsbCwiQ2l0eUlkIjowLCJDaXR5IjpudWxsLCJTdXJwbHVzQW1vdW50IjowLjAsIkRpc3RyaWN0SWQiOjAsIkRpc3RyaWN0IjpudWxsLCJBZGRyZXNzIjpudWxsLCJDb21wYW55IjpudWxsLCJQcm9mZXNzaW9uSWQiOm51bGwsIkVtYWlsIjpudWxsLCJNb2JpbGUiOm51bGwsIlVzZXJTdGF0dXMiOm51bGwsIlVzZXJUeXBlIjpudWxsLCJFeHBpcmVUaW1lIjoiMzAyMi0wOS0xOVQxNToxMzoxNS45MDgxMDEyKzA4OjAwIn19.MSyXaWcGkEHpPqTdCBUFproFiufMBGYFVqi8Jb_fOIM";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string json = JsonConvert.SerializeObject(Class, Newtonsoft.Json.Formatting.None, jsonSetting);

                req.Method = "POST";
                // req.TimeOut = "800";//设置请求超时时间，单位为毫秒
                req.ContentType = "application/json";

                req.Headers.Add("Authorization", AccessToken);

                byte[] data = Encoding.UTF8.GetBytes(json);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();

                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("POST异常错误", ex);
            }
        }

        /// <summary>
        /// 调用信用管理或者统一平台
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SendRobot(string url, string data)
        {
            var byteArray = Encoding.UTF8.GetBytes(data);
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.ProtocolVersion = HttpVersion.Version11;
            }
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var reslut = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return reslut;
        }
    }
}
