using System.IO;
using System.Web;
using Volvo.Fxm.Cors.Infrastructure;

namespace Volvo.Fxm.Infrastructure.HttpHandlers
{
    public class FxmFontHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string requestedFilePath = context.Request.PhysicalPath;

            CorsEnabler ce = new CorsEnabler();
            ce.AddCorsHeaders(context);

            context.Response.ContentType = $"application/font-{Path.GetExtension(requestedFilePath).ToLower()}";
            context.Response.WriteFile(requestedFilePath);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}