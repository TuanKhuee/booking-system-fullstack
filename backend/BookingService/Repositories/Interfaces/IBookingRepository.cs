using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking);
        Task<List<Booking>> GetByUserIdAsync(int userId);
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<bool> IsRoomBooked(int roomId, DateTime checkIn, DateTime checkOut);
        Task SaveChangesAsync();
    }
}
