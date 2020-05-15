using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace LoansMicroservice
{
    public class LoansService : Loans.LoansBase
    {
        private readonly ILogger<LoansService> _logger;
        public LoansService(ILogger<LoansService> logger)
        {
            _logger = logger;
        }
    }
}
