using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ROG.Commons.ExternalAPI;
using ROG.Commons.Helpers;
using ROG.DataDefine.ApiRequests.TokenSystem;
using ROG.DataDefine.Commons;
using ROG.DataDefine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static ROG.DataDefine.Enums.EnumClass;

namespace ROG.Commons.Attributes
{
    public class ApiTokenFilterAttribute : ActionFilterAttribute
    {
        private TokenSystemConfig tokenConfig;
        private IMemoryCache memoryCache;
        public ApiTokenFilterAttribute(ExternalAPI_URLService externalAPI_URLService, IMemoryCache _memoryCache)
        {
            tokenConfig = externalAPI_URLService.SetTokenSystemConfig();
            memoryCache = _memoryCache;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool requestTokenIsNull = false;
            bool requestTokenIsEqualsCacheToken = false;

            //Domain來源
            context.HttpContext.Request.Headers.TryGetValue("Origin", out StringValues api_keyDatas);

            string tmpDomain = $"{context.HttpContext.Request.Scheme }://{context.HttpContext.Request.Host}";
            string requestDomain = (!string.IsNullOrWhiteSpace(api_keyDatas.FirstOrDefault())) ? api_keyDatas.FirstOrDefault() : tmpDomain;

            //取得resquest token
            requestTokenIsNull = this.GetRequestHeaderData(context, out string api_key, out string token);

            if (requestTokenIsNull)
            {
                context.Result = this.SetContentResult(EnumApiStatus.API_ParameterError);
            }
            else
            {
                //取得相同api_key 的memoryToken
                TokenObjApiRequest cacheToken = this.GetMemoryCacheToken(api_key);

                //與memoryToken 判斷是否相同
                requestTokenIsEqualsCacheToken = this.RequestTokenEqualsMemoryCache(token, cacheToken?.Token);
                if (!requestTokenIsEqualsCacheToken)//不同
                {
                    //至TokenSystem以api_key與token 取得資料
                    ApiResponse<TokenResultApiRequest> apiResponse = VerifyTokenByTokenSystem(api_key, token);
                    if (apiResponse.Status == "0")
                    {
                        //將新token資料 存入memoryCache
                        this.SetMemoryCacheToken(api_key, apiResponse);
                        //重新取得CacheToken
                        cacheToken = this.GetMemoryCacheToken(api_key);
                    }
                    else
                    {
                        this.ClearMemoryCacheToken(api_key);
                        context.Result = this.SetContentResult((EnumApiStatus)Convert.ToInt32(apiResponse.Status));
                    }
                }
                else
                {
                    //以Domain來源判斷權限
                    bool clientDomainIsAllow = ClientDomainIsAllow(cacheToken.AllowList, requestDomain);
                    if (!clientDomainIsAllow)//權限不足
                    {
                        context.Result = this.SetContentResult(EnumApiStatus.NoAuth);
                    }
                    //判斷token是否過期
                    bool tokenIsNotExpire = TokenIsNotExpire(cacheToken);
                    if (!tokenIsNotExpire)//過期
                    {
                        context.Result = this.SetContentResult(EnumApiStatus.Token_Expire);
                    }
                }
            }

            base.OnActionExecuting(context);
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
        private void ClearMemoryCacheToken(string api_key)
        {
            memoryCache.Remove(api_key);
        }
        private bool TokenIsNotExpire(TokenObjApiRequest tokenData)
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(tokenData.ExpireDate)) return true;//無過期日期值接回傳
            if (DateTime.Now > Convert.ToDateTime(tokenData.ExpireDate))
            {
                result = false;
            }
            return result;
        }
        private bool ClientDomainIsAllow(string allowList, string clientDomain)
        {
            if (!string.IsNullOrWhiteSpace(allowList))//沒有設定則不判斷
            {
                var domainList = allowList.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => CheckSplitString(x)).ToList();
                if (!domainList.Contains(clientDomain))
                {
                    return false;
                }
            }
            return true;
        }
        private string CheckSplitString(string domainString)
        {
            string result = domainString;
            if (domainString.LastIndexOf('/') == domainString.Length - 1)
            {
                result = result.Substring(0, domainString.Length - 1);
            }
            return result;
        }
        private bool GetRequestHeaderData(ActionExecutingContext context, out string requestApi_key, out string requestToken)
        {
            //嘗試取得 Header 中 "Api_Key" 的資料
            var hasApi_key = context.HttpContext.Request.Headers.TryGetValue("api_key", out StringValues api_keyDatas);
            requestApi_key = api_keyDatas.FirstOrDefault();
            //嘗試取得 Header 中 "Token" 的資料
            var hasToken = context.HttpContext.Request.Headers.TryGetValue("Token", out StringValues tokenDatas);
            requestToken = tokenDatas.FirstOrDefault();

            return (string.IsNullOrWhiteSpace(requestApi_key) && string.IsNullOrWhiteSpace(requestToken));
        }
        private TokenObjApiRequest GetMemoryCacheToken(string api_key)
        {
            TokenObjApiRequest tokenObj = null;
            bool tokenCacheIsNotNull = memoryCache.TryGetValue(api_key, out tokenObj);
            return tokenObj;
        }
        private void SetMemoryCacheToken(string api_key, ApiResponse<TokenResultApiRequest> apiResponse)
        {
            TokenObjApiRequest tokenObj = apiResponse.Result.Obj.FirstOrDefault();

            bool expireDateIsNull = (string.IsNullOrWhiteSpace(tokenObj.ExpireDate));
            //根據來源設定過期時間
            DateTime expireTime = (expireDateIsNull) ? DateTime.Now.AddDays(15) : Convert.ToDateTime(tokenObj.ExpireDate);

            memoryCache.Set(api_key, tokenObj, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expireTime));
        }
        private ApiResponse<TokenResultApiRequest> VerifyTokenByTokenSystem(string api_key, string token)
        {
            Dictionary<string, string> headerData = new Dictionary<string, string>();
            headerData.Add("api_key", api_key);
            headerData.Add("Token", token);
            string responseString = WebAPIHelper.SendRequestWithHeader(tokenConfig.VerifyTokenSystemURL, RequestMethod.POST, headerData);
            ApiResponse<TokenResultApiRequest> responseObj = JsonConvert.DeserializeObject<ApiResponse<TokenResultApiRequest>>(responseString);
            return responseObj;
        }
        private bool RequestTokenEqualsMemoryCache(string token, string cacheToken)
        {
            if (token.Equals(cacheToken))
            {
                return true;
            }
            return false;
        }
    }
}
