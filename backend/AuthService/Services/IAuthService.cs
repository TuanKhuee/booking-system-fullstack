using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task Register( RegisterRequest request);
        Task<(string accessToken, string refreshToken)> Login(LoginRequest request);
        Task<(string accessToken, string refreshToken)> RefreshToken(RefreshTokenRequest request);
        Task Logout(string refreshToken);
        Task<UserRespone?> GetUserById(int userId);
    }
}