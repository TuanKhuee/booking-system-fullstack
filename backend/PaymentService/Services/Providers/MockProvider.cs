using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Models;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services.Providers
{
    public class MockProvider : IPaymentProvider
    {
        public Task<string> CreatePaymentUrl(Payment payment)
        {
            return Task.FromResult($"https://mockpayment.com/pay?amount={payment.Amount}&id={payment.Id}");
        }
    }
}
