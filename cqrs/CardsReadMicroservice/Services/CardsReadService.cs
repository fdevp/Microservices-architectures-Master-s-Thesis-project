using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CardsReadMicroservice.Repository;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
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

        public override async Task<AggregateOverallResponse> AggregateOverall(AggregateOverallRequest request, ServerCallContext context)
        {
            var cardsIds = cardsRepository.GetCardsIds();
            var transactionsResponse = await transactionsReadClient.FilterAsync(new FilterTransactionsRequest { Cards = { cardsIds }, TimestampTo = request.TimestampTo, TimestampFrom = request.TimestampFrom });
            var aggregations = Aggregations.CreateOverallCsvReport(new OverallReportData { Aggregations = request.Aggregations.ToArray(), Granularity = request.Granularity, Transactions = transactionsResponse.Transactions.ToArray() });
            return new AggregateOverallResponse { Portions = { aggregations } };
        }

        public override async Task<AggregateUserActivityResponse> AggregateUserActivity(AggregateUserActivityRequest request, ServerCallContext context)
        {
            var cards = cardsRepository.GetByAccounts(request.AccountsIds);
            var cardsIds = cards.Select(p => p.Id).ToArray();
            var transactionsResponse = await transactionsReadClient.FilterAsync(new FilterTransactionsRequest { Cards = { cardsIds }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo });
            var transactions = transactionsResponse.Transactions.ToArray();
            var aggregated = cards.SelectMany(c => AggregateUserTransactions(c, transactions.Where(t => t.CardId == c.Id).ToArray(), request.Granularity));
            return new AggregateUserActivityResponse { Portions = { aggregated } };
        }

        private IEnumerable<UserReportPortion> AggregateUserTransactions(Repository.Card card, Transaction[] transactions, Granularity granularity)
        {
            var withTimestamps = transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var portions = Aggregations.GroupByPeriods(granularity, withTimestamps);
            var ordered = portions.OrderBy(p => p.Key);
            foreach (var portion in ordered)
            {
                var debits = portion.Where(p => p.Transaction.CardId == card.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = card.Number };
            }
        }
    }
}
