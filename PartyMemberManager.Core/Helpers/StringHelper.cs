using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PartyMemberManager.Core.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// 口令加密
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static string EncryPassword(string password)
        {
            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            byte[] encryPasswordBytes = MD5.Create().ComputeHash(passwordBytes);
            string encryPassword = Convert.ToBase64String(encryPasswordBytes);
            return encryPassword;
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="key">8位字符的密钥字符串</param>
        /// <param name="iv">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string data, string key, string iv)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <returns></returns>
        public static string DESEncrypt(string data)
        {
            string key = "lzufepar";
            string iv = "878541!@";
            return DESEncrypt(data, key, iv);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <param name="key">8位字符的密钥字符串(需要和加密时相同)</param>
        /// <param name="iv">8位字符的初始化向量字符串(需要和加密时相同)</param>
        /// <returns></returns>
        public static string DESDecrypt(string data, string key, string iv)
        {
            try
            {
                byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
                byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);

                byte[] byEnc;
                try
                {
                    byEnc = Convert.FromBase64String(data);
                }
                catch
                {
                    return data;
                }

                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream(byEnc);
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cst);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                //如果解密失败，则直接返回原字符串
                return data;
            }
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <returns></returns>
        public static string DESDecrypt(string data)
        {
            string key = "lzufepar";
            string iv = "878541!@";
            return DESDecrypt(data, key, iv);
        }

        /// <summary>
        ///  验证身份证号
        /// </summary>
        /// <returns></returns>
        public static bool ValidateIdNumber(string idNumber)
        {
            string pattern = @"(^[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$)|(^[1-9]\d{5}\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{2}$)";
            if (Regex.IsMatch(idNumber, pattern))
                return true;
            else
                return false;
        }

        /// <summary>
        ///  验证学号/工号
        /// </summary>
        /// <returns></returns>
        public static bool ValidateJobNo(string jobNo)
        {
            string pattern = @"(^\d{12}$)|(^10741\d{5}$)";
            if (Regex.IsMatch(jobNo, pattern))
                return true;
            else
                return false;
        }
    }
}
