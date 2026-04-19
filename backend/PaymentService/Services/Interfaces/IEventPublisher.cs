using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Services.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(string queue, object message);
    }
}
