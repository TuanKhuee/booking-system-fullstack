using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.DTOs.Room;
using BookingService.ExternalServices.Interfaces;

namespace BookingService.ExternalServices
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;

        public RoomServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RoomDto?> GetRoomById(int roomId)
        {
            var url = $"/api/rooms/{roomId}";
            Console.WriteLine($"Calling: {url}");

            var res = await _httpClient.GetAsync(url);

            Console.WriteLine($"Status: {res.StatusCode}");

            if (!res.IsSuccessStatusCode)
                return null;

            var content = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}");

            var room = await res.Content.ReadFromJsonAsync<RoomDto>();

            return room;
        }
    }
}
