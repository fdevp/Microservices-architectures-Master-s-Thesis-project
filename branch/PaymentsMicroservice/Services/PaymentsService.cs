using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace PaymentsMicroservice
{
    public class PaymentsService : Payments.PaymentsBase
    {
        private readonly ILogger<PaymentsService> _logger;
        public PaymentsService(ILogger<PaymentsService> logger)
        {
            _logger = logger;
        }
    }
}
