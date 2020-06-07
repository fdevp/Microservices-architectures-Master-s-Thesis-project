using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsWriteMicroservice.Repository;
using static LoansWriteMicroservice.LoansWrite;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace PaymentsWriteMicroservice
{
    public class PaymentsWriteService : PaymentsWrite.PaymentsWriteBase
    {
        private readonly ILogger<PaymentsWriteService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsWriteClient transactionsClient;
        private readonly LoansWriteClient loansClient;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentsWriteService(PaymentsRepository paymentsRepository, ILogger<PaymentsWriteService> logger, Mapper mapper, TransactionsWriteClient transactionsClient, LoansWriteClient loansClient)
        {
            this.paymentsRepository = paymentsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
            this.loansClient = loansClient;
        }
        public override Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = paymentsRepository.Create(request.Amount, request.StartTimestamp, request.Interval, request.AccountId, request.Recipient);
            return Task.FromResult(new CreatePaymentResult { Payment = mapper.Map<Payment>(payment) });
        }

        public override Task<Empty> Cancel(CancelPaymentsRequest request, ServerCallContext context)
        {
            foreach (var id in request.Ids)
                paymentsRepository.Cancel(id);
            return Task.FromResult(new Empty());
        }
    }
}
