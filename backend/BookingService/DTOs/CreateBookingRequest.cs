using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.DTOs
{
    public class CreateBookingRequest
    {
        public int RoomId {get; set;}
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}