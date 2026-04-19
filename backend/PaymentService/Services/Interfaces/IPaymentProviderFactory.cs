using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentProviderFactory
    {
        IPaymentProvider GetProvider(string method);
    }
}