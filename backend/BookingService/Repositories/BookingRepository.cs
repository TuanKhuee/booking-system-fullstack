using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings.ToListAsync();
        }

        public Task<Booking?> GetByIdAsync(int id)
        {
            return _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<List<Booking>> GetByUserIdAsync(int userId)
        {
            return _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
        }

        public Task<bool> IsRoomBooked(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId
                && b.Status != BookingStatus.Cancelled
                && (
                    (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate)
                    || (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate)
                    || (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                )
            );
        }

        public async Task<List<Booking>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Bookings.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
