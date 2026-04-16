using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.ExternalServices.Interfaces
{
    public interface  IRoomServiceClient
    {
        Task<RoomDto?> GetRoomById(int roomId);
    }
}