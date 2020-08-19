using System;
using SharedClasses.Models;

namespace APIGateway.Models
{
    public class BatchProcess
    {
        public DateTime ProcessingTimestamp { get; set; }
        public string[] ProcessedPaymentsIds { get; set; }
        public Transfer[] Transfers { get; set; }
        public MessageDTO[] Messages { get; set; }
        public string[] RepaidInstalmentsIds { get; set; }
    }
}