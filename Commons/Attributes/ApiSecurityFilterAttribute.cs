using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ROG.DataDefine.Commons;
using ROG.DataDefine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ROG.DataDefine.Enums.EnumClass;

namespace ROG.Commons.Attributes
{
    //使用方式
    //[ServiceFilter(typeof(ApiSecurityFilterAttribute))]
    public class ApiSecurityFilterAttribute : ActionFilterAttribute
    {
        private ProjectConfigOptions projectConfig;
        public List<string> allowedClientIps;
        private List<string> GetAllowedClientIpsConfig() => this.projectConfig.AllowedClientIps;
        public ApiSecurityFilterAttribute(IOptions<ProjectConfigOptions> setting)
        {
            this.projectConfig = JsonConvert.DeserializeObject<ProjectConfigOptions>(JsonConvert.SerializeObject(setting.Value));
            this.allowedClientIps = this.GetAllowedClientIpsConfig();
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Domain來源
            context.HttpContext.Request.Headers.TryGetValue("Referer", out StringValues api_keyDatas);

            string tmpDomain = $"{context.HttpContext.Request.Scheme }://{context.HttpContext.Request.Host}";
            string requestDomain = (!string.IsNullOrWhiteSpace(api_keyDatas.FirstOrDefault())) ? api_keyDatas.FirstOrDefault() : tmpDomain;
            if (!allowedClientIps.Contains(requestDomain))
                context.Result = this.SetContentResult(EnumApiStatus.NoAuth);
            //如要求更嚴謹管控時可發放API Key，並要求附於Request Header
            //在此可檢查API Key是否合法，甚至API Key再綁定特定IP使用
            //...request.Cookies["X-Api-Key"]...
        }
        private ContentResult SetContentResult(EnumApiStatus apiStatus)
        {
            ApiResponse<object> apiResponse = new ApiResponse<object>();
            apiResponse.ErrorSetting(apiStatus);
            var returnStr = JsonConvert.SerializeObject(apiResponse);
            ContentResult Result = new ContentResult()
            {
                ContentType = "json/application",
                Content = returnStr,
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            return Result;
        }
    }
}
