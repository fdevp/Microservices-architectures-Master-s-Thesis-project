using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsWriteMicroservice.Repository;
using SharedClasses.Messaging;
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
        private readonly RabbitMqPublisher projectionChannel;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentsWriteService(PaymentsRepository paymentsRepository, ILogger<PaymentsWriteService> logger, Mapper mapper, TransactionsWriteClient transactionsClient, LoansWriteClient loansClient, RabbitMqPublisher projectionChannel)
        {
            this.paymentsRepository = paymentsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
            this.loansClient = loansClient;
            this.projectionChannel = projectionChannel;
        }
        public override Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = paymentsRepository.Create(request.Amount, request.StartTimestamp, request.Interval, request.AccountId, request.Recipient);
            projectionChannel.Publish(request.FlowId.ToString(), new DataProjection<Repository.Payment, string> { Upsert = new[] { payment } });
            return Task.FromResult(new CreatePaymentResult { Payment = mapper.Map<Payment>(payment) });
        }

        public override Task<Empty> UpdateRepayTimestamp(UpdateRepayTimestampRequest request, ServerCallContext context)
        {
            paymentsRepository.UpdateLastRepayTimestamp(request.Ids, request.RepayTimestamp);
            var updatedPayments = request.Ids.Select(id => paymentsRepository.Get(id)).ToArray();
            projectionChannel.Publish(request.FlowId.ToString(), new DataProjection<Repository.Payment, string> { Upsert = updatedPayments });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> Cancel(CancelPaymentsRequest request, ServerCallContext context)
        {
            foreach (var id in request.Ids)
                paymentsRepository.Cancel(id);
            var cancelledPayments = request.Ids.Select(id => paymentsRepository.Get(id)).ToArray();
            projectionChannel.Publish(request.FlowId.ToString(), new DataProjection<Repository.Payment, string> { Upsert = cancelledPayments });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> Setup(SetupRequest request, Grpc.Core.ServerCallContext context)
        {
            var payments = request.Payments.Select(p => mapper.Map<Repository.Payment>(p));
            paymentsRepository.Setup(payments);
            projectionChannel.Publish(null, new DataProjection<Repository.Payment, string> { Upsert = payments.ToArray() });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, Grpc.Core.ServerCallContext context)
        {
            var payments = request.Payments.Select(p => mapper.Map<Repository.Payment>(p));
            paymentsRepository.SetupAppend(payments);
            projectionChannel.Publish(null, new DataProjection<Repository.Payment, string> { Upsert = payments.ToArray() });
            return Task.FromResult(new Empty());
        }
    }
}
