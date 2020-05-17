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
        private readonly ILogger<PanelsBranchService> _logger;
        public PanelsBranchService(ILogger<PanelsBranchService> logger)
        {
            _logger = logger;
        }
    }
}
