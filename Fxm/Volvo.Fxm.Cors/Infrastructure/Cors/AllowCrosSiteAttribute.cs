using System;
using System.Web.Mvc;

namespace Volvo.Fxm.Cors.Infrastructure
{
    public class AllowCrosSiteAttribute : ActionFilterAttribute
    {
        private string _hostName;
        private string _allowHeaders;
        private string _credentials;
        public AllowCrosSiteAttribute(string hostName, string allowHeaders, string credentials)
        {
            _hostName = string.IsNullOrEmpty(hostName) ? "*" : hostName;
            _allowHeaders = string.IsNullOrEmpty(allowHeaders) ? "*" : allowHeaders;
            _credentials = string.IsNullOrEmpty(credentials) ? "*" : credentials;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", _hostName);
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", _allowHeaders);
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", _credentials);

            base.OnActionExecuting(filterContext);
        }
    }
}



