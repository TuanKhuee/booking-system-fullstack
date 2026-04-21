using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RoomService.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string? Location { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<Room> Rooms { get; set; }

    }
}