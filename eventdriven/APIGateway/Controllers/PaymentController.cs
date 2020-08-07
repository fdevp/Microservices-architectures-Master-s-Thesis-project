using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Payments;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly Mapper mapper;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public PaymentController(ILogger<PaymentController> logger, Mapper mapper, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(PaymentsSetup setup)
        {
            for (int i = 0; i < setup.payments.Length; i += 10000)
            {
                var portion = setup.payments.Skip(i).Take(10000).ToArray();
                var paymentsEvent = new SetupAppendPaymentsEvent { Payments = portion.Select(p => mapper.Map<Payment>(p)).ToArray() };
                this.publishingRouter.Publish(Queues.Payments, paymentsEvent, null);
            }
            return Task.CompletedTask;
        }
    }
}