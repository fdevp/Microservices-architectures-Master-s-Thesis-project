
using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice.Repository;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace AccountsMicroservice
{
    public class AccountsService
    {
        private readonly ILogger<AccountsService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly AccountsRepository accountsRepository;

        public AccountsService(AccountsRepository accountsRepository, ILogger<AccountsService> logger, PublishingRouter publishingRouter)
        {
            this.accountsRepository = accountsRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
        }


        [EventHandlingMethod(typeof(GetAccountsEvent))]
        public Task Get(MessageContext context, GetAccountsEvent inputEvent)
        {
            var accounts = inputEvent.Ids.Select(id => accountsRepository.Get(id))
                .Where(account => account != null)
                .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedAccountsEvent { Accounts = accounts }, context.FlowId);
            return Task.CompletedTask;
        }


        [EventHandlingMethod(typeof(GetUserAccountsEvent))]
        public Task GetUserAccounts(MessageContext context, GetUserAccountsEvent inputEvent)
        {
            var accounts = accountsRepository.GetByUser(inputEvent.UserId);
            publishingRouter.Publish(context.ReplyTo, new SelectedAccountsEvent { Accounts = accounts }, context.FlowId);
            return Task.CompletedTask;
        }


        [EventHandlingMethod(typeof(GetBalanceEvent))]
        public Task GetBalance(MessageContext context, GetBalanceEvent inputEvent)
        {
            var balances = inputEvent.Ids.Select(id => accountsRepository.Get(id))
                .Where(account => account != null)
                .Select(account => new AccountBalance { Id = account.Id, Balance = account.Balance })
                .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedBalancesEvent { Balances = balances }, context.FlowId);
            return Task.CompletedTask;
        }


        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task GetTransactions(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var filters = new FilterTransactionsEvent { Senders = inputEvent.Ids };
            publishingRouter.Publish(Queues.Transactions, filters, context.FlowId, context.ReplyTo);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(AccountTransferEvent))]
        public Task Transfer(MessageContext context, AccountTransferEvent inputEvent)
        {
            if (!accountsRepository.CanTransfer(inputEvent.Transfer.AccountId, inputEvent.Transfer.Recipient, inputEvent.Transfer.Amount))
                throw new ArgumentException("Cannot transfer founds.");

            var transfer = TransferToCreateTransactionEvent(inputEvent.Transfer);
            accountsRepository.Transfer(inputEvent.Transfer.AccountId, inputEvent.Transfer.Recipient, inputEvent.Transfer.Amount);
            
            //info do kogo ma wrocic
            publishingRouter.Publish(Queues.Transactions, transfer, context.FlowId, context.ReplyTo);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchTransferEvent))]
        public Task BatchTransfer(MessageContext context, BatchTransferEvent inputEvent)
        {
            if (inputEvent.Transfers.Any(r => !accountsRepository.CanTransfer(r.AccountId, r.Recipient, r.Amount)))
                throw new ArgumentException("Cannot transfer founds.");

            var transfers = inputEvent.Transfers.Select(r => TransferToCreateTransactionEvent(r)).ToArray();
            var batchAddTransactionsEvent = new BatchCreateTransactionEvent
            {
                Requests = transfers
            };

            foreach (var t in inputEvent.Transfers)
                accountsRepository.Transfer(t.AccountId, t.Recipient, t.Amount);

            //info do kogo ma wrocic
            publishingRouter.Publish(Queues.Transactions, batchAddTransactionsEvent, context.FlowId, context.ReplyTo);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupAccountsEvent))]
        public Task Setup(MessageContext context, SetupAccountsEvent inputEvent)
        {
            accountsRepository.Setup(inputEvent.Accounts);
            return Task.CompletedTask;
        }

        private CreateTransactionEvent TransferToCreateTransactionEvent(AccountTransfer inputEvent)
        {
            var account = accountsRepository.Get(inputEvent.AccountId);
            var recipient = accountsRepository.Get(inputEvent.Recipient);

            var transcation = new CreateTransactionEvent
            {
                Sender = inputEvent.AccountId,
                Recipient = inputEvent.Recipient,
                Title = inputEvent.Title,
                Amount = inputEvent.Amount
            };

            return transcation;
        }
    }
}
