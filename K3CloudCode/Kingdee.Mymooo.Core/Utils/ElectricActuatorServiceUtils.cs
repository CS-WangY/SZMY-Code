using Kingdee.Mymooo.Core.K3CloudConfiguration;
using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Kingdee.Mymooo.Core.Utils
{
	public class ElectricActuatorServiceUtils
	{
		private static DateTime _minDateTime;
		private static K3CloudServiceConfiguration _k3CloudConfig;
		private static HttpClient _httpClient;
		static ElectricActuatorServiceUtils()
		{
			_minDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_k3CloudConfig = ConfigurationManager.GetSection("ElectricActuatorConfiguration") as K3CloudServiceConfiguration; 
			
			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri(_k3CloudConfig.K3CloudConfig.Url);
			_httpClient.DefaultRequestHeaders.Add("dataCenterNumber", _k3CloudConfig.K3CloudConfig.DataCenterNumber);
			_httpClient.DefaultRequestHeaders.Add("appId", _k3CloudConfig.K3CloudConfig.AppId);
			_httpClient.Timeout = new TimeSpan(0, 10, 0);
		}

		public static string InvokePostWebService(string path, string data)
		{
			var timestamp = GetCurrentTimestamp().ToString();
			string[] arr = new string[] { _k3CloudConfig.K3CloudConfig.DataCenterNumber, _k3CloudConfig.K3CloudConfig.Username, _k3CloudConfig.K3CloudConfig.AppId, _k3CloudConfig.K3CloudConfig.AppSecret, timestamp };
			Array.Sort(arr, StringComparer.Ordinal);
			string sortdata = string.Join(string.Empty, arr);
			var signedData = Sha256Hex(sortdata);

			using (HttpContent httpContent = new StringContent(data, Encoding.UTF8))
			{
				httpContent.Headers.Add("timestamp", timestamp);
				httpContent.Headers.Add("signature", signedData);
				var response = _httpClient.PostAsync(path, httpContent).GetAwaiter().GetResult();

				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return response.Content.ReadAsStringAsync().Result;
				}
				else
				{
					return JsonConvertUtils.SerializeObject(new
					{
						Code = "httpCodeError",
						ErrorMessage = "请求接口未正确响应"
					});
				}
			}
		}

		public static string InvokeWebService(string path)
		{
			var timestamp = GetCurrentTimestamp().ToString();
			string[] arr = new string[] { _k3CloudConfig.K3CloudConfig.DataCenterNumber, _k3CloudConfig.K3CloudConfig.Username, _k3CloudConfig.K3CloudConfig.AppId, _k3CloudConfig.K3CloudConfig.AppSecret, timestamp };
			Array.Sort(arr, StringComparer.Ordinal);
			string sortdata = string.Join(string.Empty, arr);
			var signedData = Sha256Hex(sortdata);

			using (var request = new HttpRequestMessage(HttpMethod.Get, path))
			{
				request.Headers.Add("timestamp", timestamp);
				request.Headers.Add("signature", signedData);
				var response = _httpClient.SendAsync(request).Result;

				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return response.Content.ReadAsStringAsync().Result;
				}
				else
				{
					return JsonConvertUtils.SerializeObject(new
					{
						Code = "httpCodeError",
						ErrorMessage = "请求接口未正确响应"
					});
				}
			}
		}

		private static string Sha256Hex(string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			using (SHA256 sHA = SHA256.Create())
			{
				byte[] shadata = sHA.ComputeHash(bytes);
				string text = BitConverter.ToString(shadata).Replace("-", string.Empty);
				return text.ToUpperInvariant();
			}
		}

		private static long GetCurrentTimestamp()
		{
			return Convert.ToInt64(DateTime.UtcNow.Subtract(_minDateTime).TotalMilliseconds) / 1000;
		}
	}
}
