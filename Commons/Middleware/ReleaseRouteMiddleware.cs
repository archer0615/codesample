using Microsoft.AspNetCore.Http;
using ROG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.Middleware
{
    public class ReleaseRouteMiddleware
    {
        private readonly RequestDelegate next;
        private WebsiteService oWebsiteService;

        public ReleaseRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task Invoke(HttpContext context, WebsiteService _WebsiteService)
        {
            bool IsInRoute = false;
            this.oWebsiteService = _WebsiteService;

            List<string> ReleaseRouteList = new List<string>();
            ReleaseRouteList.AddRange(this.getRouteList());

            if (context.Request.Path.HasValue)
            {
                string requestPath = context.Request.Path.Value.ToLower();
                foreach (string routeItem in ReleaseRouteList)
                {
                    if (requestPath.StartsWith(routeItem)) IsInRoute = true;

                    if (IsInRoute) break;
                }

                if (!IsInRoute)
                {
                    List<string> strList = new List<string> { "", context.Request.Path.Value };
                    context.Features.Set(strList);

                    context.Request.Path = "/elite/error/";
                    context.Response.Redirect("/elite/error");
                }
            }

            await next.Invoke(context);
        }
        private List<string> getRouteList()
        {
            List<string> ReleaseRouteList = new List<string>();
                       
            ReleaseRouteList.Add("/css");
            ReleaseRouteList.Add("/dist");
            ReleaseRouteList.Add("/js");
            ReleaseRouteList.Add("/public");
            ReleaseRouteList.Add("/websites");

            ReleaseRouteList.Add("/api");
            //ReleaseRouteList.Add("/swagger");
            ReleaseRouteList.Add("/elite");
            ReleaseRouteList.Add("/404");
            ReleaseRouteList.Add("/error");
            ReleaseRouteList.Add("/DbState");

            var dbCountryList = oWebsiteService.GetEliteReleaseCountryList();

            foreach (string websitePath in dbCountryList)
            {
                ReleaseRouteList.Add($"/{websitePath.ToLower()}/elite");
            }
            return ReleaseRouteList;
        }
    }
}
