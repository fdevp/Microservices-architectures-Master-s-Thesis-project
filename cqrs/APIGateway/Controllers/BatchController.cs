using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice;
using AccountsWriteMicroservice;
using APIGateway.Models;
using AutoMapper;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using LoansReadMicroservice;
using LoansWriteMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice;
using PaymentsWriteMicroservice;
using UsersMicroservice;
using static AccountsReadMicroservice.AccountsRead;
using static AccountsWriteMicroservice.AccountsWrite;
using static LoansReadMicroservice.LoansRead;
using static LoansWriteMicroservice.LoansWrite;
using static PaymentsReadMicroservice.PaymentsRead;
using static PaymentsWriteMicroservice.PaymentsWrite;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly ILogger<BatchController> logger;
        private readonly Mapper mapper;
        private readonly AccountsReadClient accountsReadClient;
        private readonly LoansReadClient loansReadClient;
        private readonly PaymentsReadClient paymentsReadClient;
        private readonly PaymentsWriteClient paymentsWriteClient;
        private readonly UsersClient usersClient;
        private readonly LoansWriteClient loansWriteClient;
        private readonly AccountsWriteClient accountsWriteClient;

        public BatchController(ILogger<BatchController> logger, Mapper mapper,
         AccountsReadClient accountsReadClient,
         LoansReadClient loansReadClient,
         PaymentsReadClient paymentsReadClient,
         PaymentsWriteClient paymentsWriteClient,
         UsersClient usersClient,
         LoansWriteClient loansWriteClient,
         AccountsWriteClient accountsWriteClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.accountsReadClient = accountsReadClient;
            this.loansReadClient = loansReadClient;
            this.paymentsReadClient = paymentsReadClient;
            this.paymentsWriteClient = paymentsWriteClient;
            this.usersClient = usersClient;
            this.loansWriteClient = loansWriteClient;
            this.accountsWriteClient = accountsWriteClient;
        }

        [HttpGet]
        public async Task<BatchData> Get([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
        {
            var flowId = HttpContext.Items["flowId"].ToString();

            RepeatedField<Loan> loans = new RepeatedField<Loan>();
            RepeatedField<Payment> payments = new RepeatedField<Payment>();
            RepeatedField<AccountBalance> balances = new RepeatedField<AccountBalance>();

            var paymentsResponse = await paymentsReadClient.GetPartAsync(new GetPartRequest { Part = part, TotalParts = total }, HttpContext.CreateHeadersWithFlowId());
            payments = paymentsResponse.Payments;

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsIds = payments.Select(p => p.Id);
                var loansResponse = await loansReadClient.GetByPaymentsAsync(new GetLoansRequest { Ids = { paymentsIds } }, HttpContext.CreateHeadersWithFlowId());
                loans = loansResponse.Loans;
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var accountsIds = payments.Select(p => p.AccountId).Distinct();
                var balancesResponse = await accountsReadClient.GetBalancesAsync(new GetBalancesRequest { Ids = { accountsIds } }, HttpContext.CreateHeadersWithFlowId());
                balances = balancesResponse.Balances;
            }));

            await Task.WhenAll(parallelTasks);

            return new BatchData
            {
                Balances = balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray(),
                Loans = loans.Select(l => mapper.Map<LoanDTO>(l)).ToArray(),
                Payments = payments.Select(p => mapper.Map<PaymentDTO>(p)).ToArray()
            };
        }

        [HttpPost]
        public async Task Process(BatchProcess data)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var messages = data.Messages.Select(m => mapper.Map<Message>(m)).ToArray();
            var transfers = data.Transfers.Select(t => mapper.Map<Transfer>(t)).ToArray(); ;
            var instalments = data.RepaidInstalmentsIds;

            var parallelTasks = new List<Task>();

            if (messages.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await usersClient.BatchAddMessagesAsync(new BatchAddMessagesRequest { Messages = { messages } }, HttpContext.CreateHeadersWithFlowId());
                }));
            }

            if (transfers.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await accountsWriteClient.BatchTransferAsync(new BatchTransferRequest { Transfers = { transfers } }, HttpContext.CreateHeadersWithFlowId());
                }));

                parallelTasks.Add(Task.Run(async () =>
                {
                    var paymentIds = transfers.Select(t => t.PaymentId);
                    var request = new UpdateRepayTimestampRequest { Ids = { paymentIds }, RepayTimestamp = Timestamp.FromDateTime(data.RepayTimestamp) };
                    await paymentsWriteClient.UpdateRepayTimestampAsync(request, HttpContext.CreateHeadersWithFlowId());
                }));
            }

            if (instalments.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await loansWriteClient.BatchRepayInstalmentsAsync(new BatchRepayInstalmentsRequest { Ids = { instalments } }, HttpContext.CreateHeadersWithFlowId());
                }));
            }

            if (parallelTasks.Count > 0)
                await Task.WhenAll(parallelTasks);
        }
    }
}