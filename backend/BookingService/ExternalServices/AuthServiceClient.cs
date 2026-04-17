using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.DTOs.Auth;
using BookingService.ExternalServices.Interfaces;

namespace BookingService.ExternalServices
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;
        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public Task<UserDto?> GetUserById(int userId)
        {
            var respone = _httpClient.GetAsync($"/api/auth/users/{userId}");
            if (!respone.Result.IsSuccessStatusCode)
                throw new Exception($"Failed to get user with id {userId} from AuthService");
            
            var user = respone.Result.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
    }
}