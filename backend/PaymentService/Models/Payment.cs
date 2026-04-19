using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "Mock";
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
