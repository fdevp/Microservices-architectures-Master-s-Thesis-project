using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using Grpc.Core;
using LoansMicroservice;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using UsersMicroservice;
using static AccountsMicroservice.Accounts;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static UsersMicroservice.Users;

namespace BatchesBranchMicroservice
{
    public class BatchesBranchService : BatchesBranch.BatchesBranchBase
    {
        private readonly ILogger<BatchesBranchService> logger;
        private readonly AccountsClient accountsClient;
        private readonly LoansClient loansClient;
        private readonly PaymentsClient paymentsClient;
        private readonly UsersClient usersClient;

        public BatchesBranchService(ILogger<BatchesBranchService> logger,
            AccountsClient accountsClient,
            LoansClient loansClient,
            PaymentsClient paymentsClient,
            UsersClient usersClient)
        {
            this.logger = logger;
            this.accountsClient = accountsClient;
            this.loansClient = loansClient;
            this.paymentsClient = paymentsClient;
            this.usersClient = usersClient;
        }

        public override async Task<GetDataToProcessResponse> Get(GetDataToProcessRequest request, ServerCallContext context)
        {
            var paymentsAndLoansRequest = new GetPaymentsWithLoansRequest { FlowId = request.FlowId, Mod = request.Mod };
            var paymentsAndLoans = await paymentsClient.GetWithLoansAsync(paymentsAndLoansRequest);

            var paymentAccounts = paymentsAndLoans.Payments.Select(p => p.AccountId);
            var accountsRequest = new GetBalanceRequest { FlowId = request.FlowId, Ids = { paymentAccounts } };
            var balances = await accountsClient.GetBalanceAsync(accountsRequest);

            return new GetDataToProcessResponse
            {
                Payments = { paymentsAndLoans.Payments },
                Loans = { paymentsAndLoans.Loans },
                Balances = { balances.Balances }
            };
        }

        public override async Task<Empty> Process(ProcessBatchRequest request, ServerCallContext context)
        {
            var tasks = new List<Task>();
            tasks.Add(new Task(async () => await usersClient.BatchAddMessagesAsync(new BatchAddMessagesRequest
            {
                FlowId = request.FlowId,
                Messages = { request.Messages }
            })));

            tasks.Add(new Task(async () => await accountsClient.BatchTransferAsync(new BatchTransferRequest
            {
                FlowId = request.FlowId,
                Transfers = { request.Transfers }
            })));

            tasks.Add(new Task(async () => await loansClient.BatchRepayInstalmentsAsync(new BatchRepayInstalmentsRequest
            {
                FlowId = request.FlowId,
                Ids = { request.RepaidInstalmentsIds }
            })));

            await Task.WhenAll(tasks.ToArray());

            return new Empty();
        }
    }
}
