using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public static class SeedData
    {
        public static async Task SeedAdmin(AppDbContext context, IConfiguration config)
        {
            // 1. Lấy mật khẩu thuần từ biến môi trường
            var adminEmail = config["ADMIN_EMAIL"];
            var plainPassword = config["ADMIN_PASSWORD"];

            // 2. Kiểm tra xem biến có tồn tại không để tránh lỗi "inputKey" null
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(plainPassword))
            {
                Console.WriteLine("⚠️ Bỏ qua Seeding: Thiếu biến môi trường ADMIN_EMAIL hoặc ADMIN_PASSWORD.");
                return;
            }

            // 3. Kiểm tra xem Admin đã có trong DB chưa
            var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

            if (!adminExists)
            {
                Console.WriteLine($"🚀 Đang khởi tạo tài khoản Admin: {adminEmail}...");

                var admin = new User
                {
                    Name = "Admin Hotel",
                    Email = "admin.hotel@gmail.com",
                    PhoneNumber = "0123456789",
                    Role = "Admin",
                    // 4. HASH mật khẩu ngay tại đây trước khi lưu
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword)
                };

                try
                {
                    context.Users.Add(admin);
                    await context.SaveChangesAsync();
                    Console.WriteLine(" Seed account admin completed!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error seeding Admin: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ℹ Account admin already exists, no need to seed.");
            }
        }
    }
}