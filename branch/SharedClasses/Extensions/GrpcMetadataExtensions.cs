
using System.Linq;
using Grpc.Core;

namespace SharedClasses
{
    public static class GrpcMetadataExtensions
    {
        public static Metadata SelectCustom(this Metadata metadata)
        {
            var newMetadata = new Metadata();
            var flowIdHeader = metadata.FirstOrDefault(h => h.Key == "flowid");
            if (flowIdHeader != null)
                newMetadata.Add("flowid", flowIdHeader.Value);
            return newMetadata;
        }
    }
}