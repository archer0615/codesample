### 連線字串加密

### **使用方法**
```csharp
        protected string SUPPORTConnStr = decrypt(WebConfigurationManager.AppSettings["support_sitesql"]);

        /// <summary>
        /// 連線字串解密
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static string Decrypt(string original)
        {
            SHA512CryptService oCrpt = new SHA512CryptService();
            try
            {
                string result = oCrpt.Decrypt(original);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

```