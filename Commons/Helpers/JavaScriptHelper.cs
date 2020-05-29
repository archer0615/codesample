using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ROG.Commons.Helpers
{
    public static class JavaScriptHelper
    {
        public static string Json(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[]
                {
                    new StringEnumConverter(),
                },
                //StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}
