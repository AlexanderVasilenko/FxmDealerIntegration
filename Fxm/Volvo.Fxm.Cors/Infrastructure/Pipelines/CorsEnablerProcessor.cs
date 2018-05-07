using Sitecore.Pipelines.HttpRequest;

namespace Volvo.Fxm.Cors.Infrastructure.Pipelines
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