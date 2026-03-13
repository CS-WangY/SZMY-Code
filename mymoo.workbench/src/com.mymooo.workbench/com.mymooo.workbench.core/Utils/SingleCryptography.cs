using System;
using System.Security.Cryptography;
using System.Text;

namespace com.mymooo.workbench.core.Utils
{
	/// <summary>
	/// 单点登录的加密解密算法
	/// </summary>
	public class SingleCryptography
    {
        private static string Password = "b80b9c1148434c8fb975185238a7965a";
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text">需要加密的文本</param>
        /// <param name="key">加密密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string text, string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Password;
            }
            byte[] raw = Encoding.UTF8.GetBytes(key);
            byte[] encrypt = Encoding.UTF8.GetBytes(text);

            Aes aes = Aes.Create("AES");
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = raw;
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] cryptData = cTransform.TransformFinalBlock(encrypt, 0, encrypt.Length);
            string CryptString = Convert.ToBase64String(cryptData);
            return CryptString;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text">密文</param>
        /// <param name="key">解密密钥</param>
        /// <returns>明文</returns>
        public static string Decryptor(string text, string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Password;
            }
            byte[] raw = Encoding.UTF8.GetBytes(key);

            Aes aes = Aes.Create("AES");
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = raw;
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] encryptedData = Convert.FromBase64String(text);
            byte[] cryptData = cTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(cryptData);
        }
    }
}
