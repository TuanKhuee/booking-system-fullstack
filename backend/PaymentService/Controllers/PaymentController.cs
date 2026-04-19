using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services.Interfaces;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var url = await _paymentService.CreatePaymentAsync(request);
            return Ok(new { paymentUrl = url });
        }

        [Authorize(Roles = "User")]
        [HttpGet("paypal-return")]
        public async Task<IActionResult> PayPalReturn(int paymentId, string token)
        {
            await _paymentService.HandlePayPalReturn(paymentId, token);
            return Ok("Payment success");
        }
    }
}
