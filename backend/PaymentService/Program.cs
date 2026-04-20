using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PaymentService.Data;
using PaymentService.Models.Settings;
using PaymentService.Services.Factories;
using PaymentService.Services.Implementations;
using PaymentService.Services.Interfaces;
using PaymentService.Services.Providers;
using PaymentService.Services.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();
builder.Services.AddScoped<IPaymentService, PaymentService.Services.Implementations.PaymentService>();
builder.Services.AddSingleton<IEventPublisher, RabbitMQPublisher>();

builder.Services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));
builder.Services.AddScoped<MockProvider>();
builder.Services.AddHttpClient<PayPalProvider>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration!"))
            ),
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
