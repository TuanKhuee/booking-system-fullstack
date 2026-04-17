using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using BookingService.DTOs;
using BookingService.ExternalServices.Interfaces;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using BookingService.Hubs;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomServiceClient _roomServiceClient;
        private readonly IEmailService _emailService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly IAuthServiceClient _authServiceClient;

        public BookingService(
            IBookingRepository bookingRepository,
            IRoomServiceClient roomServiceClient,
            IEmailService emailService,
            IHubContext<BookingHub> hubContext,
            IAuthServiceClient authServiceClient
        )
        {
            _bookingRepository = bookingRepository;
            _roomServiceClient = roomServiceClient;
            _emailService = emailService;
            _hubContext = hubContext;
            _authServiceClient = authServiceClient;
        }

        public async Task CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking is null || booking.UserId != userId)
            {
                throw new Exception("Booking not found");
            }
            if (booking.UserId != userId)
            {
                throw new Exception("Unauthorized");
            }

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task<int> CreateBookingAsync(int userId, CreateBookingRequest request)
        {
            if (request.CheckOutDate <= request.CheckInDate)
                throw new Exception("Invalid date");

            var room = await _roomServiceClient.GetRoomById(request.RoomId);
            Console.WriteLine("CALLING ROOM SERVICE...");

            // 🔥 FIX CHÍNH
            if (room == null || room.Id == 0)
                throw new Exception("Room not found");

            var isBooked = await _bookingRepository.IsRoomBooked(
                request.RoomId,
                request.CheckInDate,
                request.CheckOutDate
            );

            if (isBooked)
                throw new Exception("Room is already booked for the selected dates");
            

             
            var days = (request.CheckOutDate - request.CheckInDate).Days;

            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = days * room.PricePerNight,
                Status = BookingStatus.Pending,
            };

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            // SingalR: Notify clients about new booking
            await _hubContext.Clients.All.SendAsync("BookingCreated", new
            {
                BookingId = booking.Id,
                RoomId = booking.RoomId,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
            });

            // Send confirmation email (simplified)
            var user = await _authServiceClient.GetUserById(userId);

            if(user == null)
                throw new Exception("User not found");
            
            await _emailService.SendEmailAsync(
                toEmail: user.Email,
                subject: "Booking Confirmation",
                body: $"{user.Name} booking for room {room.Name} from {booking.CheckInDate:d} to {booking.CheckOutDate:d} has been created. Total price: ${booking.TotalPrice}"
            );

            return booking.Id;
        }

        public async Task<List<BookingResponse>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    RoomId = b.RoomId,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                })
                .ToList();
        }

        public async Task<List<BookingResponse>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            return bookings
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    RoomId = b.RoomId,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                })
                .ToList();
        }

        public async Task<List<BookingResponse>> GetPagedAsync(int page, int pageSize)
        {
            var bookings = await _bookingRepository.GetPagedAsync(page, pageSize);

            return bookings
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status.ToString(),
                })
                .ToList();
        }
    }
}
