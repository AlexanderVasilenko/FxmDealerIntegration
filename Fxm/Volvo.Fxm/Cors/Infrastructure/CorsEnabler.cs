using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Volvo.Fxm.Cors.Domain;

namespace Volvo.Fxm.Cors.Infrastructure
{
    public class CorsEnabler
    {
        private const string allowOrigin = "Access-Control-Allow-Origin";
        private const string allowHeaders = "Access-Control-Allow-Headers";
        private const string allowCredentials = "Access-Control-Allow-Credentials";
        public void AddCorsHeaders(HttpContext context)
        {
            string origin = context.Request.Headers.Get("Origin");
            if (string.IsNullOrEmpty(origin)) return;

            CorsClientSettings settings = new CorsClientSettings();
            CorsClient dealer = settings.GetCorsClient(origin);
            if (dealer == null) return;

            if (!context.Response.Headers.AllKeys.Contains(allowOrigin))
            {
                context.Response.Headers.Add(allowOrigin, dealer.HostName);
                context.Response.Headers.Add(allowHeaders, dealer.AllowHeaders);
                context.Response.Headers.Add(allowCredentials, dealer.Credentials);

            }
        }
    }
}