using Microsoft.AspNetCore.Http;
using ROG.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ROG.Commons.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private LogService oLogService;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;       
        }
        public async Task Invoke(HttpContext context, LogService _LogService)
        {
            this.oLogService = _LogService;

            DateTime requestStartTime = DateTime.Now;

            Stream originalBody = context.Response.Body;
            try
            {
                if (context.Request.Path.HasValue && 
                    (context.Request.Path.Value.IndexOf("/api/") > -1 || context.Request.Path.Value.IndexOf("/search-api/") > -1))
                {
                    await HandleApiResponseAsync(context, requestStartTime);
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                if (context.Request.Path.HasValue && context.Request.Path.Value.IndexOf("/api/") > -1)
                {
                    this.HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    this.HandleOtherExceptionAsync(context, ex);
                }
                List<string> strList = new List<string> { ex.Message, context.Request.Path.Value };
                context.Features.Set(strList);
                
                context.Request.Path = "/error/";
                context.Response.Redirect("/error");
                await _next(context);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
        private async Task HandleApiResponseAsync(HttpContext context, DateTime requestStartTime)
        {
            Stream originalBody = context.Response.Body;
            using (var memStream = new MemoryStream())
            {
                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;

                string responseBodyString = new StreamReader(memStream).ReadToEnd();

                if (!string.IsNullOrWhiteSpace(responseBodyString))
                {
                    string requestUrl = context.Request.Path + context.Request?.QueryString ?? "";

                    oLogService.InsertRequestLog(requestUrl, responseBodyString, requestStartTime, DateTime.Now);
                }
                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);
            }
        }

        private void HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            string runName = "";
            string serviceURL = context.Request.Path;
            string paramInput = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
            string messge = exception.Message;
            oLogService.InsertErrorLog(runName, serviceURL, paramInput, exception.StackTrace, messge);
        }

        private void HandleOtherExceptionAsync(HttpContext context, Exception exception)
        {
            string serviceURL = context.Request.Path;
            string paramInput = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
            string messge = exception.Message;
            oLogService.InsertErrorLog("", serviceURL, paramInput, exception.StackTrace, messge);
        }
    }
}
