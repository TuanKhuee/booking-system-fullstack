using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomService.DTOs
{
    public class RoomTypeDetailResponse
    {
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public List<RoomInTypes> Rooms { get; set; } = new();
    }
}