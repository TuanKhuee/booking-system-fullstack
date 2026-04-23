using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BookingService.Services.Interfaces;

namespace BookingService.Services.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _hostName;
        private const string QueueName = "payment.success";

        public RabbitMQConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory { HostName = _hostName };

            // Retry logic: RabbitMQ may take time to become ready in Docker
            var maxRetries = 10;
            var delay = TimeSpan.FromSeconds(2);

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    Console.WriteLine($"[*] Attempting to connect to RabbitMQ at '{_hostName}' (attempt {i + 1}/{maxRetries})...");
                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    await _channel.QueueDeclareAsync(queue: QueueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null,
                                         cancellationToken: cancellationToken);

                    Console.WriteLine("[✓] Successfully connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"[!] RabbitMQ connection failed: {ex.Message}. Retrying in {delay.TotalSeconds}s...");
                    await Task.Delay(delay, cancellationToken);
                    delay *= 2; // Exponential backoff
                }
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Because RabbitMQ Client v7 requires a slight change in setting up AsyncEventingBasicConsumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    // The payload from PaymentService is new { BookingId = payment.BookingId }
                    var data = JsonSerializer.Deserialize<JsonElement>(message);
                    if (data.TryGetProperty("BookingId", out var bookingIdProp) || data.TryGetProperty("bookingId", out bookingIdProp))
                    {
                        if (bookingIdProp.TryGetInt32(out int bookingId))
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                            await bookingService.ConfirmPaymentForBookingAsync(bookingId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[x] Error Processing RabbitMQ Message: {ex.Message}");
                }
            };

            await _channel.BasicConsumeAsync(queue: QueueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.CloseAsync();
            if (_connection != null)
                await _connection.CloseAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
