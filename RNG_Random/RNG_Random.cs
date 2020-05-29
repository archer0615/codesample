using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RNG_Random
{
    /// <summary>
    /// RNGCryptoServiceProvider Next 
    /// </summary>
    public static class RNG_Random
    {
        private static RNGCryptoServiceProvider rngp = new RNGCryptoServiceProvider();
        private static byte[] rb = new byte[4];
        /// <summary>
        /// 產生一個非負數且最大值 max 以下的亂數
        /// </summary>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static int RNG_Next(int max)
        {
            byte[] rb = new byte[4];
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            value = value % (max + 1);
            if (value < 0) value = -value;
            return value;
        }
        /// <summary>
        /// 產生一個非負數且最小值在 min 以上最大值在 max 以下的亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public static int RNG_Next(int min, int max)
        {
            int value = RNG_Next(max - min) + min;
            return value;
        }
        /// <summary>
        /// 產生一個非負數的亂數
        /// </summary>
        public static int RNG_Next()
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            if (value < 0) value = -value;
            return value;
        }
    }
}
