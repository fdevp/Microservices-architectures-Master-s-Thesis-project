using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ReportsMicroservice
{
    public class ReportsBranchService : Reports.ReportsBase
    {
        private readonly ILogger<ReportsBranchService> logger;
        public ReportsBranchService(ILogger<ReportsBranchService> logger)
        {
            this.logger = logger;
        }
    }
}
