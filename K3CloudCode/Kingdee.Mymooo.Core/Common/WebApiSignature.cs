using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    public static class WebApiSignature
    {
        private const string TOKEN = "c26d78b3d9785249caac9a6d43c76bf9";
        private static readonly DateTime minDateTime = new DateTime(1970, 1, 1);

        public static string CreateTimestamp()
        {
            return GetCurrentTimestamp().ToString();
        }

        private static long GetCurrentTimestamp()
        {
            return Convert.ToInt64(DateTime.UtcNow.Subtract(minDateTime).TotalMilliseconds);
        }

        public static string CreateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string Sign(string timestamp, string nonce, string data)
        {
            //拼接签名数据
            var signStr = timestamp + nonce + data;
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
                sBuilder.Append(string.Format("{0:x2}", b));
            }
            return sBuilder.ToString();
        }

        private static byte[] HMACSHA256Encrypt(string value)
        {
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(TOKEN));
            var hashValue = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return hashValue;
        }

        public static bool Validate(string timestamp, string nonce, string signature, string data)
        {
            if (ParametersError(timestamp, nonce, signature))
            {
                return false;
            }

            var responseTimestamp = GetCurrentTimestamp();
            try
            {
                var requestTimestamp = long.Parse(timestamp);
                // 5分钟之内才合法
                if (Timeout(responseTimestamp, requestTimestamp))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return ValidSignature(timestamp, nonce, data, signature);
        }

        private static bool ValidSignature(string timestamp, string nonce, string data, string signature)
        {
            var sign = Sign(timestamp, nonce, data);
            return sign.Equals(signature);
        }

        private static bool Timeout(long responseTimestamp, long requestTimestamp)
        {
            return responseTimestamp - requestTimestamp > 300000;
        }

        private static bool ParametersError(string timestamp, string nonce, string signature)
        {
            return string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp);
        }
    }
}
