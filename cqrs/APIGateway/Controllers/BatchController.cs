using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice;
using AccountsWriteMicroservice;
using APIGateway.Models;
using AutoMapper;
using Google.Protobuf.Collections;
using LoansReadMicroservice;
using LoansWriteMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice;
using UsersMicroservice;
using static AccountsReadMicroservice.AccountsRead;
using static AccountsWriteMicroservice.AccountsWrite;
using static LoansReadMicroservice.LoansRead;
using static LoansWriteMicroservice.LoansWrite;
using static PaymentsReadMicroservice.PaymentsRead;
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
        private readonly UsersClient usersClient;
        private readonly LoansWriteClient loansWriteClient;
        private readonly AccountsWriteClient accountsWriteClient;

        public BatchController(ILogger<BatchController> logger, Mapper mapper,
         AccountsReadClient accountsReadClient,
         LoansReadClient loansReadClient,
         PaymentsReadClient paymentsReadClient,
         UsersClient usersClient,
         LoansWriteClient loansWriteClient,
         AccountsWriteClient accountsWriteClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.accountsReadClient = accountsReadClient;
            this.loansReadClient = loansReadClient;
            this.paymentsReadClient = paymentsReadClient;
            this.usersClient = usersClient;
            this.loansWriteClient = loansWriteClient;
            this.accountsWriteClient = accountsWriteClient;
        }

        [HttpGet]
        public async Task<BatchData> Get([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
        {
            var flowId = (long)HttpContext.Items["flowId"];

            RepeatedField<Loan> loans = new RepeatedField<Loan>();
            RepeatedField<Payment> payments = new RepeatedField<Payment>();
            RepeatedField<Balance> balances = new RepeatedField<Balance>();

            var paymentsResponse = await paymentsReadClient.GetPartAsync(new GetPartRequest { FlowId = flowId, Part = part, TotalParts = total });
            payments = paymentsResponse.Payments;

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsIds = payments.Select(p => p.Id);
                var loansResponse = await loansReadClient.GetPaymentsLoansAsync(new GetPaymentsLoansRequest { FlowId = flowId, PaymentsIds = { paymentsIds } });
                loans = loansResponse.Loans;
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var accountsIds = payments.Select(p => p.AccountId);
                var balancesRequest = await accountsReadClient.GetBalanceAsync(new GetBalanceRequest { FlowId = flowId, Ids = { accountsIds } });
                balances = balancesRequest.Balances;
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
            var flowId = (long)HttpContext.Items["flowId"];
            var messages = data.Messages.Select(m => mapper.Map<Message>(m)).ToArray();
            var transfers = data.Transfers.Select(t => mapper.Map<Transfer>(t)).ToArray(); ;
            var instalments = data.RepaidInstalmentsIds;

            var parallelTasks = new List<Task>();

            if (messages.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await usersClient.BatchAddMessagesAsync(new BatchAddMessagesRequest { FlowId = flowId, Messages = { messages } });
                }));
            }

            if (transfers.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await accountsWriteClient.BatchTransferAsync(new BatchTransferRequest { FlowId = flowId, Transfers = { transfers } });
                }));
            }

            if (instalments.Length > 0)
            {
                parallelTasks.Add(Task.Run(async () =>
                {
                    await loansWriteClient.BatchRepayInstalmentsAsync(new BatchRepayInstalmentsRequest { FlowId = flowId, Ids = { instalments } });
                }));
            }


            await Task.WhenAll(parallelTasks);
        }
    }
}