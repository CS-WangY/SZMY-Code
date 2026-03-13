using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Utils
{
    /// <summary>
    /// 解密顺丰推送的图片
    /// </summary>
    public class ShunFengPictureDecrypt
    {
        public static string DecryptContent(string content,string encryptKey)
        {
            byte[] decode = Convert.FromBase64String(content);
            decode = AES_Decode(decode, encryptKey);
            return Encoding.UTF8.GetString(decode);
        }

        private static byte[] AES_Decode(byte[] decryptStr, string encryptKey)
        {
            byte[] keyArray = GetAesKeyBytes(encryptKey);
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform cTransform = rm.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(decryptStr, 0, decryptStr.Length);
            return resultArray;
        }

        private static byte[] GetAesKeyBytes(string key)
        {
            byte[] keyArray = null;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] hash = sha1.ComputeHash(Convert.FromBase64String(key));
                var rd = sha1.ComputeHash(hash);
                keyArray = rd.Take(16).ToArray();
            }

            return keyArray;
        }
    }
}
