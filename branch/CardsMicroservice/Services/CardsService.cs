using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsMicroservice.Repository;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static AccountsMicroservice.Accounts;

namespace CardsMicroservice
{
    public class CardsService : Cards.CardsBase
    {
        private readonly ILogger<CardsService> logger;
        private readonly Mapper mapper;
        private readonly AccountsClient accountsClient;
        private CardsRepository repository = new CardsRepository();
        public CardsService(ILogger<CardsService> logger, Mapper mapper, AccountsClient accountsClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.accountsClient = accountsClient;
        }

        public override Task<GetCardsResponse> Get(GetCardsRequest request, ServerCallContext context)
        {
            var cards = request.Ids.Select(id => repository.GetCard(id))
                            .Where(card => card != null)
                            .Select(transaction => mapper.Map<Card>(transaction))
                            .ToArray();
            return Task.FromResult(new GetCardsResponse { Cards = { cards } });
        }

        public override Task<GetBlocksResponse> GetBlocks(GetBlocksRequest request, ServerCallContext context)
        {
            var blocks = repository.GetBlocks(request.CardId).Select(b => mapper.Map<Block>(b));
            return Task.FromResult(new GetBlocksResponse { Blocks = { blocks } });
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var cardAccountsIds = request.Ids.Select(id => repository.GetCard(id))
                .Where(card => card != null)
                .Select(card => card.AccountId)
                .ToHashSet();

            var transactionsRequest = new AccountsMicroservice.GetTransactionsRequest { FlowId = request.FlowId, Ids = { cardAccountsIds } };
            var accountTransactions = await accountsClient.GetTransactionsAsync(transactionsRequest);
            var transactions = accountTransactions.Transactions.Where(t => cardAccountsIds.Contains(t.CardId));
            return new GetTransactionsResponse { Transactions = { transactions } };
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            var card = repository.GetCard(request.CardId);
            if (card == null)
                throw new ArgumentException("Card not found.");

            var blockRequestTime = DateTime.UtcNow;
            var title = $"{DateTime.UtcNow} card usage for a transfer worth {request.Amount} EUR";

            var transferRequest = new AccountsMicroservice.TransferRequest
            {
                FlowId = request.FlowId,
                AccountId = card.AccountId,
                Recipient = request.Recipient,
                Amount = request.Amount,
                Title = title
            };

            var transferResponse = await accountsClient.TransferAsync(transferRequest);
            repository.CreateBlock(card.Id, transferResponse.Transaction.Id, blockRequestTime);

            return new TransferResponse { Transaction = transferResponse.Transaction };
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var cards = request.Cards.Select(c => mapper.Map<Repository.Card>(c));
            var blocks = request.Blocks.Select(b => mapper.Map<Repository.Block>(b));
            repository.Setup(cards, blocks);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            repository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}
