using System.IO;
using System.Web;

namespace Volvo.Fxm.Cors.Infrastructure
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