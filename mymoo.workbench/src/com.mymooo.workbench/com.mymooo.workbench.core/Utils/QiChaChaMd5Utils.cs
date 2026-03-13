using com.mymooo.workbench.core.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace com.mymooo.workbench.qichacha
{
    public class QiChaChaMd5Utils
    {
        /// <summary>
        /// 设置请求Header信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="secertKey"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string GetHeaderVals(string appkey, string secertKey, long timeSpan)
        {
            var token = MD5Encrypt(appkey + timeSpan + secertKey, Encoding.UTF8);
            return token.ToUpper();

        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        private static string MD5Encrypt(string input, Encoding encode)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(encode.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
