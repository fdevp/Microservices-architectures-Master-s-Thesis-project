using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace APIGateway
{
    public static class HttpContextExtensions
    {
        public static Metadata CreateHeadersWithFlowId(this HttpContext context)
        {
            var flowId = context.Items["flowId"]?.ToString();
            if (string.IsNullOrEmpty(flowId))
                return null;

            var metadata = new Metadata();
            metadata.Add("flowid", flowId);
            return metadata;
        }
    }
}