
using System.Linq;
using Grpc.Core;

namespace SharedClasses
{
    public static class GrpcMetadataExtensions
    {
        public static Metadata SelectCustom(this Metadata metadata)
        {
            var flowId = metadata.GetFlowId();
            if (flowId == null)
                return null;
            var newMetadata = new Metadata();
            newMetadata.Add("flowid", flowId);
            return newMetadata;
        }
		
		public static string GetFlowId(this Metadata metadata) => metadata.FirstOrDefault(h => h.Key == "flowid")?.Value;
    }
}