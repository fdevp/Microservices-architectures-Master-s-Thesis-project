using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansWriteMicroservice.Repository;
using Microsoft.Extensions.Logging;
using PaymentsWriteMicroservice;
using static PaymentsWriteMicroservice.PaymentsWrite;

namespace LoansWriteMicroservice
{
    public class LoansWriteService : LoansWrite.LoansWriteBase
    {
        private readonly ILogger<LoansWriteService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsWriteClient paymentsClient;
        private LoansRepository loansRepository;

        public LoansWriteService(LoansRepository loansRepository, ILogger<LoansWriteService> logger, Mapper mapper, PaymentsWriteClient paymentsClient)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
        }

        public override async Task<Empty> BatchRepayInstalments(BatchRepayInstalmentsRequest request, ServerCallContext context)
        {
            var paymentsToFinish = new List<string>();
            foreach (var id in request.Ids)
            {
                var totalAmountPaid = loansRepository.RepayInstalment(id);
                if (totalAmountPaid)
                    paymentsToFinish.Add(loansRepository.Get(id).PaymentId);
            }

            var cancelPaymentsRequest = new CancelPaymentsRequest
            {
                FlowId = request.FlowId,
                Ids = { paymentsToFinish }
            };
            await paymentsClient.CancelAsync(cancelPaymentsRequest);
            return new Empty();
        }
    }
}
