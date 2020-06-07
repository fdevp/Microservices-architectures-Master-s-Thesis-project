using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsWriteMicroservice.Repository;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static AccountsWriteMicroservice.AccountsWrite;

namespace CardsWriteMicroservice
{
    public class CardsWriteService : CardsWrite.CardsWriteBase
    {
        private readonly ILogger<CardsWriteService> logger;
        private readonly Mapper mapper;
        private readonly AccountsWriteClient accountsClient;
        private CardsRepository cardsRepository;
        public CardsWriteService(CardsRepository cardsRepository, ILogger<CardsWriteService> logger, Mapper mapper, AccountsWriteClient accountsClient)
        {
            this.cardsRepository = cardsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.accountsClient = accountsClient;
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            var card = cardsRepository.GetCard(request.CardId);
            if (card == null)
                throw new ArgumentException("Card not found.");

            var blockRequestTime = DateTime.UtcNow;
            var title = $"{DateTime.UtcNow} card usage for a transfer worth {request.Amount} EUR";

            var transfer = new Transfer
            {
                AccountId = card.AccountId,
                Recipient = request.Recipient,
                Amount = request.Amount,
                Title = title
            };
            var transferRequest = new AccountsWriteMicroservice.TransferRequest
            {
                FlowId = request.FlowId,
                Transfer = transfer,
            };

            var transferResponse = await accountsClient.TransferAsync(transferRequest);
            cardsRepository.CreateBlock(card.Id, transferResponse.Transaction.Id, blockRequestTime);

            return new TransferResponse { Transaction = transferResponse.Transaction };
        }
    }
}
