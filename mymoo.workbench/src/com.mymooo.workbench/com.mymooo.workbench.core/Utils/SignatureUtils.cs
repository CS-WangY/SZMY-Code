using mymooo.core.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace com.mymooo.workbench.core.Utils
{
    [AutoInject(InJectType.Single)]
    public class SignatureUtils
    {
		private static readonly DateTime minDateTime = new(1970, 1, 1);

        public string CreateTimestamp()
        {
            return GetCurrentTimestamp().ToString();
        }

        private long GetCurrentTimestamp()
        {
            return Convert.ToInt64(DateTime.UtcNow.Subtract(minDateTime).TotalMilliseconds);
        }

        public string CreateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Sign(string token, string nonce, string timestamp, string data = "")
        {
            //拼接签名数据
            var signStr = timestamp + nonce + data;
            //将字符串中字符按升序排序
            var sortStr = string.Concat(signStr.OrderBy(c => c));
            return Encrypt(token, sortStr);
        }

        private string Encrypt(string token, string value)
        {
            var encryptedBytes = HMACSHA256Encrypt(token, value);
            var sBuilder = new StringBuilder();
            foreach (var b in encryptedBytes)
            {
                sBuilder.Append(string.Format("{0:x2}", b));
            }
            return sBuilder.ToString();
        }

        private byte[] HMACSHA256Encrypt(string token, string value)
        {
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(token));
            var hashValue = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return hashValue;
        }

        public bool Validate(string token, string nonce, string timestamp, string signature, string data = "")
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

            return ValidSignature(token, nonce, timestamp, data, signature);
        }

        private bool ValidSignature(string token, string nonce, string timestamp, string data, string signature)
        {
            var sign = Sign(token, nonce, timestamp, data);
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
