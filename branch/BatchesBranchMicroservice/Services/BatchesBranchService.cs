using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BatchesBranchMicroservice
{
    public class BatchesBranchService : BatchesBranch.BatchesBranchBase
    {
        private readonly ILogger<BatchesBranchService> logger;
        public BatchesBranchService(ILogger<BatchesBranchService> logger)
        {
            this.logger = logger;
        }
    }
}
