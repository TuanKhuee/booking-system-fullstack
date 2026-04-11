using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;
        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomRespone>> GetAllRooms()
        {
            return await _context.Rooms.Include(x => x.RoomType).Select(r => new RoomRespone
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Status = r.Status,
                RoomTypeId = r.RoomTypeId,
                RoomTypeName = r.RoomType.Name,
                Price = r.RoomType.Price,
                Description = r.RoomType.Description,
                Capacity = r.RoomType.Capacity
            }).ToListAsync();
        }

        public async Task<Room> CreateRoom(CreateRoomRequest request)
        {
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                RoomTypeId = request.RoomTypeId,
                Status = request.Status
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public Task<RoomType> CreateRoomType(CreateRoomTypeRequest request)
        {
            var roomType = new RoomType
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Capacity = request.Capacity
            };
            _context.RoomTypes.Add(roomType);
            return _context.SaveChangesAsync().ContinueWith(t => roomType);
        }

        public Task DeleteRoom(string roomNumber)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                throw new Exception("Room not found");
            }
            else

                _context.Rooms.Remove(room);
            return _context.SaveChangesAsync();

        }

        public async Task<IEnumerable<RoomTypeDetailResponse>> GetAllRoomsByType(int roomTypeId)
    {
        return await _context.RoomTypes
        .Where(rt => rt.Id == roomTypeId)
        .Select(rt => new RoomTypeDetailResponse // Dùng đúng class đã tạo ở trên
        {
            RoomTypeName = rt.Name,
            Price = rt.Price,
            Description = rt.Description,
            Rooms = rt.Rooms.Select(r => new RoomInTypes // Map danh sách phòng vào DTO con
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Status = r.Status
            }).ToList()
        })
        .ToListAsync();
}

        public async Task<Room> GetRoomById(string roomNumber)
        {
            return await _context.Rooms.Include(x => x.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        }

        public Task UpdateRoom(string roomNumber, UpdateRoomRequest request)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                throw new Exception("Room not found");
            }
            room.RoomNumber = request.RoomNumber;
            room.RoomTypeId = request.RoomTypeId;
            room.Status = request.Status;
            return _context.SaveChangesAsync();
        }


    }
}