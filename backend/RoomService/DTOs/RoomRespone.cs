using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RoomService.DTOs
{
    public class RoomRespone
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Status { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; }
    }
}