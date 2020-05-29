using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.Helpers
{
    public static class StringHelper
    {
        public static string StringToLower(this string inputStr)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(inputStr))
            {
                result = inputStr.ToLower();
            }
            return result;
        }
    }
}
