using System;

namespace Requester
{
    public class BatchProcess
    {
        public DateTime ProcessingTimestamp { get; set; }
        public string[] ProcessedPaymentsIds { get; set; }
        public AccountTransfer[] Transfers { get; set; }
        public MessageDTO[] Messages { get; set; }
        public string[] RepaidInstalmentsIds { get; set; }
    }
}