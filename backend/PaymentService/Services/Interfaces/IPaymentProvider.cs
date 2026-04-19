using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Models;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentProvider
    {
        Task<string> CreatePaymentUrl(Payment payment);
    }
}
