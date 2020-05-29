using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptHelper
{
    public class SHA512CryptService : CryptHelper<SHA512CryptoServiceProvider>
    {
        public override string hashKeyWord { get; set; }
        /// <param name="hashKeyWord">可不填hashKeyWord</param>
        public SHA512CryptService(string hashKeyWord = "") : base(hashKeyWord)
        {
        }
    }
    public abstract class CryptHelper<THash> where THash : HashAlgorithm, new()
    {
        private const string AoccHashKey = "@aoccdbcon!";
        private TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
        public abstract string hashKeyWord { get; set; }
        private enum RunMode
        {
            EncryptMode,
            DecryptMode
        }
        public CryptHelper(string hashKeyWord)
        {
            SetCryptoServiceProviderMode();
            SetHashKeyWord(hashKeyWord);
        }
        private void SetHashKeyWord(string hashKeyWord)
        {
            this.hashKeyWord = string.IsNullOrWhiteSpace(hashKeyWord) ? AoccHashKey : hashKeyWord;
        }
        private void SetCryptoServiceProviderMode()
        {
            des.Mode = CipherMode.ECB;
        }
        private byte[] GetASCIIBytes(string stringVal, RunMode runMode)
        {
            byte[] result;
            switch (runMode)
            {
                case RunMode.DecryptMode:
                    result = Convert.FromBase64String(stringVal);
                    break;
                case RunMode.EncryptMode:
                default:
                    result = ASCIIEncoding.ASCII.GetBytes(stringVal);
                    break;
            }
            return result;
        }
        private byte[] SplitPwdHash(byte[] pwdhash)
        {
            byte[] trimmedBytes = new byte[24];
            System.Buffer.BlockCopy(pwdhash, 0, trimmedBytes, 0, 24);
            return trimmedBytes;
        }
        public virtual string Encrypt(string original)
        {
            string result = string.Empty;
            byte[] pkey, buff;
            try
            {
                using (var algorithm = new THash())
                {
                    pkey = algorithm.ComputeHash(GetASCIIBytes(hashKeyWord, RunMode.EncryptMode));
                    if (pkey.Length > 24) pkey = SplitPwdHash(pkey);
                    des.Key = pkey;
                    buff = GetASCIIBytes(original, RunMode.EncryptMode);
                    result = Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public virtual string Decrypt(string original)
        {
            string result = string.Empty;
            byte[] pkey, buff;
            try
            {
                using (var algorithm = new THash())
                {
                    pkey = algorithm.ComputeHash(GetASCIIBytes(hashKeyWord, RunMode.EncryptMode));
                    if (pkey.Length > 24) pkey = SplitPwdHash(pkey);
                    des.Key = pkey;
                    buff = GetASCIIBytes(original, RunMode.DecryptMode);
                    result = ASCIIEncoding.ASCII.GetString(des.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
