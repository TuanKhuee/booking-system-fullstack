using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services.Messaging
{
    public class RabbitMQPublisher : IEventPublisher
    {
        private readonly string _hostName;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        }

        public async Task PublishAsync(string queue, object message)
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: "",
                                 routingKey: queue,
                                 body: body);

            Console.WriteLine($"[x] Sent {json} to {queue}");
        }
    }
}
