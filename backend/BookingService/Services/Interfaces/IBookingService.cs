using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.DTOs;

namespace BookingService.Services.Interfaces
{
    public interface IBookingService
    {
        Task<int> CreateBookingAsync(int userId, CreateBookingRequest request);
        Task<List<BookingResponse>> GetBookingsByUserIdAsync(int userId);
        Task<List<BookingResponse>> GetAllBookingsAsync();
        Task CancelBookingAsync(int bookingId, int userId);
    }
}