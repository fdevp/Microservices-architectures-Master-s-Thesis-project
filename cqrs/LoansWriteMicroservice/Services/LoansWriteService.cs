using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LoansWriteMicroservice.Repository;
using Microsoft.Extensions.Logging;
using PaymentsWriteMicroservice;
using SharedClasses;
using SharedClasses.Messaging;
using static PaymentsWriteMicroservice.PaymentsWrite;

namespace LoansWriteMicroservice
{
    public class LoansWriteService : LoansWrite.LoansWriteBase
    {
        private readonly ILogger<LoansWriteService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsWriteClient paymentsClient;
        private readonly RabbitMqPublisher projectionChannel;
        private LoansRepository loansRepository;

        public LoansWriteService(LoansRepository loansRepository, ILogger<LoansWriteService> logger, Mapper mapper, PaymentsWriteClient paymentsClient, RabbitMqPublisher projectionChannel)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
            this.projectionChannel = projectionChannel;
        }

        public override async Task<Empty> BatchRepayInstalments(BatchRepayInstalmentsRequest request, ServerCallContext context)
        {
            var paymentsToFinish = RepayInstalments(request);

            if (paymentsToFinish.Any())
            {
                var cancelPaymentsRequest = new CancelPaymentsRequest { Ids = { paymentsToFinish } };
                await paymentsClient.CancelAsync(cancelPaymentsRequest, context.RequestHeaders.SelectCustom());
            }

            var repaidInstalments = request.Ids.Select(id => loansRepository.Get(id)).ToArray();
            if (repaidInstalments.Length > 0)
                projectionChannel.Publish(context.RequestHeaders.GetFlowId(), new DataProjection<Models.Loan, string> { Upsert = repaidInstalments });
            return new Empty();
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Models.Loan>(l));
            loansRepository.Setup(loans);
            projectionChannel.Publish(null, new DataProjection<Models.Loan, string> { Upsert = loans.ToArray() });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Models.Loan>(l));
            loansRepository.SetupAppend(loans);
            projectionChannel.Publish(null, new DataProjection<Models.Loan, string> { Upsert = loans.ToArray() });
            return Task.FromResult(new Empty());
        }

        private IEnumerable<string> RepayInstalments(BatchRepayInstalmentsRequest request)
        {
            foreach (var id in request.Ids)
            {
                var totalAmountPaid = loansRepository.RepayInstalment(id);
                if (totalAmountPaid)
                    yield return loansRepository.Get(id).PaymentId;
            }
        }
    }
}
