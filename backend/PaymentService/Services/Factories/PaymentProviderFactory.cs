using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Services.Interfaces;
using PaymentService.Services.Providers;

namespace PaymentService.Services.Factories
{
    public class PaymentProviderFactory : IPaymentProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProvider GetProvider(string method)
        {
            return method switch
            {
                "PayPal" => _serviceProvider.GetRequiredService<PayPalProvider>(),
                "Mock" => _serviceProvider.GetRequiredService<MockProvider>(),
                _ => throw new Exception("Unsupported payment method"),
            };
        }
    }
}
