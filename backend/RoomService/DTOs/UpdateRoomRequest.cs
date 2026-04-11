using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomService.DTOs
{
    public class UpdateRoomRequest
    {
        public string RoomNumber { get; set; }
        public int RoomTypeId { get; set; }
        public string Status { get; set; }
    }
}