using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.Helpers
{
    public static class DateTimeConvertHelper
    {
        public static Tuple<bool, string> UnixTimeStampToDateTime(double? unixTimeStamp)
        {
            if (unixTimeStamp.HasValue)
            {
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp.Value).ToLocalTime();
                return new Tuple<bool, string>(true, dtDateTime.ToString(GenericStringSetting.DateTimeLong));
            }
            else
            {
                return new Tuple<bool, string>(false, "");
            }
        }
        public static Tuple<bool, string> UnixTimeStampToDateTime(string unixTimeStamp)
        {
            if (!string.IsNullOrWhiteSpace(unixTimeStamp))
            {
                double tmpTimeStamp;
                Double.TryParse(unixTimeStamp, out tmpTimeStamp);
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(tmpTimeStamp).ToLocalTime();
                return new Tuple<bool, string>(true, dtDateTime.ToString(GenericStringSetting.DateTimeLong));
            }
            else
            {
                return new Tuple<bool, string>(false, "");
            }
        }
    }
}
