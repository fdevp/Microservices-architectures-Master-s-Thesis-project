using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace PanelsBranchMicroservice
{
    public class PanelsBranchService : PanelsBranch.PanelsBranchBase
    {
        private readonly ILogger<PanelsBranchService> logger;
        public PanelsBranchService(ILogger<PanelsBranchService> logger)
        {
            this.logger = logger;
        }
    }
}
