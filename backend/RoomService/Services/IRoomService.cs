using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomRespone>> GetAllRooms();
        Task<RoomType> CreateRoomType(CreateRoomTypeRequest request);
        Task<Room> CreateRoom(CreateRoomRequest request);
        Task<IEnumerable<RoomTypeDetailResponse>> GetAllRoomsByType(int roomTypeId);
        Task<Room> GetRoomById(string roomNumber);
        Task UpdateRoom(string roomNumber, UpdateRoomRequest request);
        Task DeleteRoom(string roomNumber);
    }
}