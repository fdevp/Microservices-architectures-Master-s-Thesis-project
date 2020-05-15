using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace TransactionsMicroservice
{
    public class TransactionsService : Transactions.TransactionsBase
    {
        private readonly ILogger<TransactionsService> _logger;
        public TransactionsService(ILogger<TransactionsService> logger)
        {
            _logger = logger;
        }
    }
}
