using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Utils
{
    public class KDSHA256
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Sha256Hex(string data)
        {
            return Sha256Hex(data, Encoding.UTF8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sha256Hex(string data, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(data);
            return Sha256Hex(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string Sha256Hex(byte[] bytes)
        {
            using (SHA256 sHA = SHA256.Create())
            {
                byte[] data = sHA.ComputeHash(bytes);
                return ToHexString(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="toLowerCase"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] data, bool toLowerCase = true)
        {
            string text = BitConverter.ToString(data).Replace("-", string.Empty);
            if (!toLowerCase)
            {
                return text.ToUpperInvariant();
            }

            return text.ToLowerInvariant();
        }
    }
}
