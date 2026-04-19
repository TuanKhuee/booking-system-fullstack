using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PaymentService.Services.Interfaces;

namespace PaymentService.Services.Messaging
{
    public class RabbitMQPublisher : IEventPublisher
    {
        public Task PublishAsync(string queue, object message)
        {
            // publish RabbitMQ
            return Task.CompletedTask;
        }
    }
}
