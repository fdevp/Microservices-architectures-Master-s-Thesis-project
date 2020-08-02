using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace APIGateway.Middlewares
{
    public class FlowIdMiddleware
    {
        private readonly RequestDelegate next;

        public FlowIdMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var flowId = httpContext.Request.Headers.ContainsKey("flowId") ? httpContext.Request.Headers["flowId"].ToString() : FlowIdProvider.Create();
            httpContext.Items.Add("flowId", flowId);
            await next(httpContext);
        }
    }
}