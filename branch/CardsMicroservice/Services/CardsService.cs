using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsMicroservice.Repository;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsMicroservice;
using static AccountsMicroservice.Accounts;
using static TransactionsMicroservice.Transactions;

namespace CardsMicroservice
{
    public class CardsService : Cards.CardsBase
    {
        private readonly ILogger<CardsService> logger;
        private readonly Mapper mapper;
        private readonly AccountsClient accountsClient;
        private readonly TransactionsClient transactionsClient;
        private CardsRepository cardsRepository;
        public CardsService(CardsRepository cardsRepository, ILogger<CardsService> logger, Mapper mapper, AccountsClient accountsClient, TransactionsClient transactionsClient)
        {
            this.cardsRepository = cardsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.accountsClient = accountsClient;
            this.transactionsClient = transactionsClient;
        }

        public override Task<GetCardsResponse> Get(GetCardsRequest request, ServerCallContext context)
        {
            var cards = request.Ids.Select(id => cardsRepository.GetCard(id))
                            .Where(card => card != null)
                            .Select(transaction => mapper.Map<Card>(transaction))
                            .ToArray();
            return Task.FromResult(new GetCardsResponse { Cards = { cards } });
        }

        public override Task<GetCardsResponse> GetByAccounts(GetCardsRequest request, ServerCallContext context)
        {
            var cards = cardsRepository.GetByAccounts(request.Ids)
                                        .Select(transaction => mapper.Map<Card>(transaction))
                                        .ToArray();
            return Task.FromResult(new GetCardsResponse { Cards = { cards } });
        }

        public override Task<GetBlocksResponse> GetBlocks(GetBlocksRequest request, ServerCallContext context)
        {
            var blocks = cardsRepository.GetBlocks(request.CardId).Select(b => mapper.Map<Block>(b));
            return Task.FromResult(new GetBlocksResponse { Blocks = { blocks } });
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var ids = request.Ids.Any() ? request.Ids.ToArray() : cardsRepository.GetIds();
            var transactionsRequest = new FilterTransactionsRequest { Cards = { ids }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest, context.RequestHeaders.SelectCustom());
            return new GetTransactionsResponse { Transactions = { transactionsResponse.Transactions } };
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
                CardId = card.Id,
                Recipient = request.Recipient,
                Amount = request.Amount,
                Title = title
            };
            var transferRequest = new AccountsMicroservice.TransferRequest { Transfer = transfer };

            var transferResponse = await accountsClient.TransferAsync(transferRequest, context.RequestHeaders.SelectCustom());
            cardsRepository.CreateBlock(card.Id, transferResponse.Transaction.Id, blockRequestTime);

            return new TransferResponse { Transaction = transferResponse.Transaction };
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var cards = request.Cards.Select(c => mapper.Map<Repository.Card>(c));
            var blocks = request.Blocks.Select(b => mapper.Map<Repository.Block>(b));
            cardsRepository.Setup(cards, blocks);
            return Task.FromResult(new Empty());
        }
    }
}
