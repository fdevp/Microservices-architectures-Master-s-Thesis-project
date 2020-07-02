using SharedClasses.Models;

namespace APIGateway.Models
{
    public class BatchTransfers
    {
        public AccountTransfer[] Transfers { get; set; }
    }
}