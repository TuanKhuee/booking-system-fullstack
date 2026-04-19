using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Services.Interfaces;
using PaymentService.Services.Providers;

namespace PaymentService.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPaymentProviderFactory _providerFactory;
        private readonly IEventPublisher _eventPublisher;

        public PaymentService(
            AppDbContext dbContext,
            IPaymentProviderFactory providerFactory,
            IEventPublisher eventPublisher
        )
        {
            _dbContext = dbContext;
            _providerFactory = providerFactory;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            var provider = _providerFactory.GetProvider(request.PaymentMethod);
            return await provider.CreatePaymentUrl(payment);
        }

        public async Task HandlePayPalReturn(int paymentId, string token)
        {
            var payment = await _dbContext.Payments.FindAsync(paymentId);
            if (payment == null)
                return;

            if (payment.Status == "Success")
                return;

            var provider = _providerFactory.GetProvider("PayPal") as PayPalProvider;

            var result = await provider.Capture(token);

            if (result == "COMPLETED")
            {
                payment.Status = "Success";
                payment.TransactionId = token;

                await _eventPublisher.PublishAsync(
                    "payment.success",
                    new { BookingId = payment.BookingId }
                );
            }
            else
            {
                payment.Status = "Failed";
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
