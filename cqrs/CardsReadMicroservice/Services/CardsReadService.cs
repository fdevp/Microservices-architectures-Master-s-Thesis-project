using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsReadMicroservice.Repository;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsReadMicroservice;
using static TransactionsReadMicroservice.TransactionsRead;

namespace CardsReadMicroservice
{
    public class CardsReadService : CardsRead.CardsReadBase
    {
        private readonly ILogger<CardsReadService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsReadClient transactionsReadClient;
        private CardsRepository cardsRepository;
        public CardsReadService(CardsRepository cardsRepository, ILogger<CardsReadService> logger, Mapper mapper, TransactionsReadClient transactionsReadClient)
        {
            this.cardsRepository = cardsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsReadClient = transactionsReadClient;
        }

        public override Task<GetCardsResponse> Get(GetCardsRequest request, ServerCallContext context)
        {
            var cards = request.Ids.Select(id => cardsRepository.GetCard(id))
                            .Where(card => card != null)
                            .Select(transaction => mapper.Map<Card>(transaction))
                            .ToArray();
            return Task.FromResult(new GetCardsResponse { Cards = { cards } });
        }

        public override Task<GetCardsResponse> GetByAccounts(GetCardsByAccountsRequest request, ServerCallContext context)
        {
            var cards = cardsRepository.GetByAccounts(request.AccountIds)
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
            var transactionsRequest = new FilterTransactionsRequest { FlowId = request.FlowId, Cards = { request.Ids } };
            var transactionsResponse = await transactionsReadClient.FilterAsync(transactionsRequest);
            return new GetTransactionsResponse { Transactions = { transactionsResponse.Transactions } };
        }
    }
}
