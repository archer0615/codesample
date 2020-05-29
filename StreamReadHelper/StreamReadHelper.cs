using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace StreamReadHelper.Helpers
{
    public class StreamReadHelper
    {
        public static byte[] ReadStramForByte(Stream inputStream)
        {
            byte[] imageByte = new byte[inputStream.Length];
            using (Stream imageStream = inputStream)
            {
                imageStream.Read(imageByte, 0, Convert.ToInt32(inputStream.Length));
            }
            return imageByte;
        }
    }
}