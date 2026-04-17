using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.DTOs.Auth;

namespace BookingService.ExternalServices.Interfaces
{
    public interface IAuthServiceClient
    {
        Task<UserDto?> GetUserById(int userId);
    }
}