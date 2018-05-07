using Sitecore.Pipelines.HttpRequest;
using Volvo.Fxm.Cors.Infrastructure;

namespace Volvo.Fxm.Infrastructure.Pipelines.Cors
{
    public class CorsEnablerProcessor
    {
        public void Process(HttpRequestArgs args)
        {
            CorsEnabler ce = new CorsEnabler();
            ce.AddCorsHeaders(args.Context);
        }
        
    }
}