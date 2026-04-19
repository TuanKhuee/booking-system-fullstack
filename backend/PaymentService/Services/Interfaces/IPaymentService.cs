using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.DTOs;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentAsync(CreatePaymentRequest request);
        Task HandlePayPalReturn(int paymentId, string token);
    }
}
