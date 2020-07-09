using SharedClasses.Models;

namespace APIGateway.Models
{
    public class BatchProcess
    {
        public Transfer[] Transfers { get; set; }
        public MessageDTO[] Messages { get; set; }
        public string[] RepaidInstalmentsIds { get; set; }
    }
}