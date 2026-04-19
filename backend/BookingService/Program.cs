using System.Text;
using BookingService.Data;
using BookingService.ExternalServices;
using BookingService.ExternalServices.Interfaces;
using BookingService.Hubs;
using BookingService.Repositories;
using BookingService.Services;
using BookingService.Services.Interfaces;
using BookingService.Services.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Dependency Injection
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService.Services.BookingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddSignalR();

// HttpClient → call RoomService
builder.Services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5294");
});

// HttpClient → call AuthService
builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5221");
});

// JWT Authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
        };
    });


// Authorization
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapHub<BookingHub>("/hubs/bookingHub");
// Auth
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
