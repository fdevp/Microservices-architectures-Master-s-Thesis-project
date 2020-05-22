using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ReportsBranchMicroservice
{
    public class ReportsBranchService : ReportsBranch.ReportsBranchBase
    {
        private readonly ILogger<ReportsBranchService> logger;
        public ReportsBranchService(ILogger<ReportsBranchService> logger)
        {
            this.logger = logger;
        }
    }
}
