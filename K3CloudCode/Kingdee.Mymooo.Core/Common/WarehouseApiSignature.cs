using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    public class WarehouseApiSignature
    {
        /// <summary>
        /// Create a sign hash
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string CreateSignHash(string warehouseToken, string data)
        {
            byte[] keyButter = Encoding.UTF8.GetBytes(warehouseToken);
            byte[] butter = Encoding.UTF8.GetBytes(data);
            HMACSHA256 sha256 = new HMACSHA256(keyButter);
            byte[] hash = sha256.ComputeHash(butter);
            return BitConverter.ToString(hash).Replace("-", "");
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string CreateTimestamp()
        {
            DateTime min = new DateTime(1970, 1, 1);
            return Convert.ToInt64(DateTime.UtcNow.Subtract(min).TotalMilliseconds).ToString();
        }
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <returns></returns>
        public static string CreateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="warehouseToken"></param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static string Sign(string warehouseToken, string timestamp, string nonce, string data)
        {
            //拼接签名数据
            var signStr = $"{timestamp}{nonce}{data}";
            //将字符串中字符按升序排序
            var sortStr = string.Concat(signStr.OrderBy(c => c));
            return CreateSignHash(warehouseToken, sortStr);
        }
        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="warehouseToken"></param>
        /// <param name="sign">签名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static bool ValidateSign(string warehouseToken, string sign, string timestamp, string nonce, string data)
        {
            var encryptedSign = Sign(warehouseToken, timestamp, nonce, data);
            return encryptedSign.Equals(sign);
        }

    }
}
