using Microsoft.AspNetCore.Http;
using ROG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.Middleware
{
    public class IgnoreRouteMiddleware
    {
        private readonly RequestDelegate next;
        private WebsiteService oWebsiteService;
        public IgnoreRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, WebsiteService _WebsiteService)
        {
            this.oWebsiteService = _WebsiteService;

            List<string> IgnoreRouteList = this.setIgnoreRouteList();
            IgnoreRouteList.AddRange(this.getEliteReleaseCountryList());

            foreach (var routeItem in IgnoreRouteList)
            {
                if (context.Request.Path.HasValue &&
                    context.Request.Path.Value.ToLower().StartsWith(routeItem))
                {

                    context.Response.StatusCode = 404;

                    return;
                }
            }
            await next.Invoke(context);
        }
        private List<string> getEliteReleaseCountryList()
        {
            List<string> IgnoreCountryList = new List<string>();
            var dbCountryList = oWebsiteService.GetEliteReleaseCountryList();
            foreach (var item in dbCountryList)
            {
                IgnoreCountryList.Add($"/{item}/elite");
            }
            return IgnoreCountryList;
        }
        private List<string> setIgnoreRouteList()
        {
            List<string> IgnoreRouteList = new List<string>();

            IgnoreRouteList.Add("/elite");
            IgnoreRouteList.Add("/support");

            return IgnoreRouteList;
        }
    }
}
