using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var res = await _httpClient.GetAsync($"/api/rooms/{roomId}");
            if (res.IsSuccessStatusCode)
            {
                return null;
            }
            return await res.Content.ReadFromJsonAsync<RoomDto>();
        }
    }
}