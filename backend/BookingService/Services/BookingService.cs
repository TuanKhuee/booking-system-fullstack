using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using BookingService.DTOs;
using BookingService.ExternalServices.Interfaces;
using BookingService.Hubs;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

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
                TotalPrice = days * room.Price,
                Status = BookingStatus.Pending,
            };

            // SingalR: Notify clients about new booking
            await _hubContext.Clients.All.SendAsync(
                "BookingCreated",
                new
                {
                    BookingId = booking.Id,
                    RoomId = booking.RoomId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status.ToString(),
                }
            );

            // Send confirmation email (simplified)
            var user = await _authServiceClient.GetUserById(userId);

            if (user == null)
                throw new Exception("User not found");
            var totalVnd = booking.TotalPrice.ToString("N0") + " VND";

            var body =
                $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333'>
                    <h2 style='color: #2c3e50;'>🎉 Booking Confirmed</h2>
    
                    <p>Xin chào <b>{user.Name}</b>,</p>
    
                    <p>Đặt phòng của bạn đã được tạo thành công với thông tin chi tiết:</p>
    
                    <table style='border-collapse: collapse; width: 100%; margin-top: 10px'>
                <tr>
                <td style='padding: 8px; border: 1px solid #ddd'><b>Room</b></td>
                <td style='padding: 8px; border: 1px solid #ddd'>{room.RoomNumber}</td>
                </tr>
                <tr>
                <td style='padding: 8px; border: 1px solid #ddd'><b>Check-in</b></td>
                <td style='padding: 8px; border: 1px solid #ddd'>{booking.CheckInDate:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd'><b>Check-out</b></td>
                    <td style='padding: 8px; border: 1px solid #ddd'>{booking.CheckOutDate:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd'><b>Total Price</b></td>
                    <td style='padding: 8px; border: 1px solid #ddd; color: green; font-weight: bold'>
                {totalVnd}
                </td>
                </tr>
                </table>

                <p style='margin-top: 20px'>
                    Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi ❤️
                </p>

                <hr />
                <small style='color: gray'>
                Đây là email tự động, vui lòng không trả lời.
                        </small>
                </div>
            ";
            await _emailService.SendEmailAsync(
                toEmail: user.Email,
                subject: "Booking Confirmation",
                body: body
            );

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

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

        public async Task ConfirmPaymentForBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return;
            }

            if (booking.Status == BookingStatus.Confirmed)
            {
                return;
            }

            booking.Status = BookingStatus.Confirmed;
            await _bookingRepository.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "BookingPaymentSuccess",
                new
                {
                    BookingId = booking.Id,
                    Status = booking.Status.ToString()
                }
            );
        }
    }
}
