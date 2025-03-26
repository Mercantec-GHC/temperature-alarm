using Api.DBAccess;
using Api.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Api.AMQP
{
    public class AMQPPush
    {
        private readonly IConfiguration _configuration;
        private readonly DbAccess _dbAccess;

        public AMQPPush(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
        }

        public async Task Handle_Push_Device_Limits()
        {
            var factory = new ConnectionFactory();
            var queue = "temperature-limits";

            factory.UserName = _configuration["AMQP:username"];
            factory.Password = _configuration["AMQP:password"];
            factory.HostName = _configuration["AMQP:host"];
            factory.Port = Convert.ToInt32(_configuration["AMQP:port"]);

            // Connecting to our rabbitmq and after that it create's a channel where you can connect to a queue
            var conn = await factory.CreateConnectionAsync();
            Console.WriteLine("AMQPClient connected");
            var channel = await conn.CreateChannelAsync();

            // Here we connect to the queue through the channel that got created earlier
            await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false);
            Console.WriteLine($"{queue} connected");

            while (true)
            {

                var devices = _dbAccess.ReadDevices();
                foreach (var device in devices)
                {
                    var deviceLimit = new DeviceLimit();
                    deviceLimit.ReferenceId = device.ReferenceId;
                    deviceLimit.TempHigh = device.TempHigh;
                    deviceLimit.TempLow = device.TempLow;
                    string message = JsonSerializer.Serialize(deviceLimit);
                    var body = Encoding.UTF8.GetBytes(message);
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
                }

                await Task.Delay(10000);

                await channel.CloseAsync();
                Console.WriteLine($"{queue} disconnected");
                await conn.CloseAsync();
                Console.WriteLine("AMQPClient disconnected");
                await channel.DisposeAsync();
                await conn.DisposeAsync();
                await Task.Delay(3600000);

                conn = await factory.CreateConnectionAsync();
                Console.WriteLine("AMQPClient connected");
                channel = await conn.CreateChannelAsync();

                // Here we connect to the queue through the channel that got created earlier
                await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false);
                Console.WriteLine($"{queue} connected");

                // Everytime a message is recieved from the queue it goes into this consumer.ReceivedAsync
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    Console.WriteLine("Emptying queue");

                    return Task.CompletedTask;
                };

                // Consumes the data in the queue
                await channel.BasicConsumeAsync(queue, true, consumer);

            }
        }
    }
}
