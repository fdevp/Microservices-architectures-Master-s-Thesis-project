using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CardsMicroservice
{
    public class CardsService : Cards.CardsBase
    {
        private readonly ILogger<CardsService> _logger;
        public CardsService(ILogger<CardsService> logger)
        {
            _logger = logger;
        }
    }
}
