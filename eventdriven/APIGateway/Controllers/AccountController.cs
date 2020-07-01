using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        // private readonly ILogger<AccountController> logger;
        // private readonly Mapper mapper;

        // public AccountController(ILogger<AccountController> logger,Mapper mapper)
        // {
        //     this.logger = logger;
        //     this.mapper = mapper;
        // }

        // [HttpGet]
        // [Route("{userId}")]
        // public async Task<IEnumerable<AccountDTO>> Get(string userId)
        // {
        //     var request = new GetUserAccountsRequest { FlowId = (long)HttpContext.Items["flowId"], UserId = userId };
        //     var response = await accountsReadClient.GetUserAccountsAsync(request);
        //     var accounts = response.Accounts.Select(a => mapper.Map<AccountDTO>(a));
        //     return accounts;
        // }

        // [HttpGet]
        // [Route("balance/{accountId}")]
        // public async Task<float> Balance(string accountId)
        // {
        //     var request = new GetBalanceRequest { FlowId = (long)HttpContext.Items["flowId"], Ids = { accountId } };
        //     var repsonse = await accountsReadClient.GetBalanceAsync(request);
        //     var balance = repsonse.Balances.Single();
        //     return balance.Balance_;
        // }

        // [HttpGet]
        // [Route("balances")]
        // public async Task<BalanceDTO[]> Balance(string[] ids)
        // {
        //     var request = new GetBalanceRequest { FlowId = (long)HttpContext.Items["flowId"], Ids = { ids } };
        //     var response = await accountsReadClient.GetBalanceAsync(request);
        //     return response.Balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray();
        // }

        // [HttpPost]
        // [Route("transfer")]
        // public async Task<TransactionDTO> Transfer(AccountTransfer data)
        // {
        //     var transfer = mapper.Map<Transfer>(data);
        //     var request = new TransferRequest { FlowId = (long)HttpContext.Items["flowId"], Transfer = transfer };
        //     var response = await accountsWriteClient.TransferAsync(request);
        //     var transcation = mapper.Map<TransactionDTO>(response.Transaction);
        //     return transcation;
        // }

        // [HttpPost]
        // [Route("batchTransfer")]
        // public async Task<TransactionDTO> BatchTransfer(BatchTransfers data)
        // {
        //     var transfers = data.Transfers.Select(t => mapper.Map<Transfer>(t));
        //     var request = new BatchTransferRequest { FlowId = (long)HttpContext.Items["flowId"], Transfers = { transfers } };
        //     var response = await accountsWriteClient.BatchTransferAsync(request);
        //     var transcation = mapper.Map<TransactionDTO>(response.Transactions);
        //     return transcation;
        // }

        // [HttpPost]
        // [Route("setup")]
        // public async Task Setup(AccountsSetup setup)
        // {
        //     var request = mapper.Map<SetupRequest>(setup);
        //     await accountsWriteClient.SetupAsync(request);
        // }
    }
}