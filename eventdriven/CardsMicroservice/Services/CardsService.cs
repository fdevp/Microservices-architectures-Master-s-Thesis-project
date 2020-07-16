
using System.Threading.Tasks;
using CardsMicroservice.Repository;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Accounts;
using SharedClasses.Messaging;
using SharedClasses.Events;
using System.Linq;
using System;
using SharedClasses.Models;
using SharedClasses.Events.Transactions;

namespace CardsMicroservice
{
    public class CardsService
    {
        private readonly ILogger<CardsService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;
        private CardsRepository cardsRepository;

        public CardsService(CardsRepository cardsRepository, ILogger<CardsService> logger, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter)
        {
            this.cardsRepository = cardsRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        [EventHandlingMethod(typeof(GetCardsEvent))]
        public Task Get(MessageContext context, GetCardsEvent inputEvent)
        {
            var cards = inputEvent.Ids.Select(id => cardsRepository.GetCard(id))
                            .Where(card => card != null)
                            .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedCardsEvent { Cards = cards }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetCardsByAccountsEvent))]
        public Task GetByAccounts(MessageContext context, GetCardsByAccountsEvent inputEvent)
        {
            var cards = cardsRepository.GetByAccounts(inputEvent.AccountsIds).ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedCardsEvent { Cards = cards }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetBlocksEvent))]
        public Task GetBlocks(MessageContext context, GetBlocksEvent inputEvent)
        {
            var blocks = cardsRepository.GetBlocks(inputEvent.CardId);
            publishingRouter.Publish(context.ReplyTo, new SelectedBlocksEvent { Blocks = blocks }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task GetTransactions(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var cardsIds = inputEvent.Ids != null && inputEvent.Ids.Length > 0 ? inputEvent.Ids : cardsRepository.GetIds();
            var getTransactionsEvent = new FilterTransactionsEvent { Cards = cardsIds, TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo };
            publishingRouter.Publish(Queues.Transactions, getTransactionsEvent, context.FlowId, context.ReplyTo);
            return Task.CompletedTask;
        }


        [EventHandlingMethod(typeof(TransferEvent))]
        public async Task Transfer(MessageContext context, TransferEvent inputEvent)
        {
            var card = cardsRepository.GetCard(inputEvent.Transfer.CardId);
            if (card == null)
                throw new ArgumentException("Card not found.");

            var blockRequestTime = DateTime.UtcNow;
            var title = $"{DateTime.UtcNow} card usage for a transfer worth {inputEvent.Transfer.Amount} EUR";

            var transfer = new Transfer
            {
                AccountId = card.AccountId,
                CardId = card.Id,
                Recipient = inputEvent.Transfer.Recipient,
                Amount = inputEvent.Transfer.Amount,
                Title = title
            };

            var accountTransferEvent = new TransferEvent { Transfer = transfer };
            var reply = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(context.FlowId, () => publishingRouter.Publish(Queues.Accounts, accountTransferEvent, context.FlowId, Queues.Cards));
            var transaction = reply.Transactions.Single();
            cardsRepository.CreateBlock(card.Id, transaction.Id, blockRequestTime);
            if (context.ReplyTo != null)
                publishingRouter.Publish(context.ReplyTo, transaction, context.FlowId);
        }

        [EventHandlingMethod(typeof(SetupCardsEvent))]
        public Task Setup(MessageContext context, SetupCardsEvent inputEvent)
        {
            cardsRepository.Setup(inputEvent.Cards, inputEvent.Blocks);
            return Task.CompletedTask;
        }
    }
}
