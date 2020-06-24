using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsWriteMicroservice.Repository;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses.Messaging;
using static AccountsWriteMicroservice.AccountsWrite;

namespace CardsWriteMicroservice
{
    public class CardsWriteService : CardsWrite.CardsWriteBase
    {
        private readonly ILogger<CardsWriteService> logger;
        private readonly Mapper mapper;
        private readonly AccountsWriteClient accountsClient;
        private readonly RabbitMqPublisher projectionChannel;
        private CardsRepository cardsRepository;

        public CardsWriteService(CardsRepository cardsRepository, ILogger<CardsWriteService> logger, Mapper mapper, AccountsWriteClient accountsClient, RabbitMqPublisher projectionChannel)
        {
            this.cardsRepository = cardsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.accountsClient = accountsClient;
            this.projectionChannel = projectionChannel;
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
            var block = cardsRepository.CreateBlock(card.Id, transferResponse.Transaction.Id, blockRequestTime);
            var upsert = new CardsUpsert { Block = block };
            projectionChannel.Publish(new DataProjection<CardsUpsert, string> { Upsert = new[] { upsert } });

            return new TransferResponse { Transaction = transferResponse.Transaction };
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var cards = request.Cards.Select(c => mapper.Map<Repository.Card>(c));
            var blocks = request.Blocks.Select(b => mapper.Map<Repository.Block>(b));
            cardsRepository.Setup(cards, blocks);

            var upsert = cards.Select(c => new CardsUpsert { Card = c });
            upsert = upsert.Concat(blocks.Select(b => new CardsUpsert { Block = b }));
            projectionChannel.Publish(new DataProjection<CardsUpsert, string> { Upsert = upsert.ToArray() });
            return Task.FromResult(new Empty());
        }
    }
}
