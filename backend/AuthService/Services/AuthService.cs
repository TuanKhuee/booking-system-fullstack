using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task Register(RegisterRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(string accessToken, string refreshToken)> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }
            bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!valid)
            {
                throw new Exception("Invalid credentials");
            }
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshToken(
            RefreshTokenRequest request
        )
        {
            var token = await _context
                .RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (token == null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Expired refresh token");
            }

            // revoke token cũ
            token.IsRevoked = true;

            // tạo token mới
            var newRefreshTokenString = _jwtService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = token.UserId,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
            };

            _context.RefreshTokens.Add(newRefreshToken);

            // tạo access token mới
            var newAccessToken = _jwtService.GenerateAccessToken(token.User);

            await _context.SaveChangesAsync();

            return (newAccessToken, newRefreshTokenString);
        }

        public async Task Logout(string refreshToken)
        {
            var token = _context.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
            if (token == null)
            {
                throw new Exception("Invalid refresh token");
            }
            if (token.IsRevoked)
            {
                return ;
            }
            token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }
    }
}
