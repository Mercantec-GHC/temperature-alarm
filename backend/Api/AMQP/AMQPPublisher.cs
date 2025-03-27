using Api.DBAccess;
using Api.Models;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.AMQP
{
    public class AMQPPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly DbAccess _dbAccess;
        private IConnection _conn;
        private IChannel _channel;
        private ConnectionFactory _factory;
        private string _queue;

        public AMQPPublisher(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
            _factory = new ConnectionFactory();
            _queue = "temperature-limits";

            InitFactory();
        }

        public async Task Handle_Push_Device_Limits()
        {
            while (true)
            {
                await Connect();

                // Publishes all devices limits
                var devices = _dbAccess.ReadDevices();
                foreach (var device in devices)
                {
                    var deviceLimit = new DeviceLimit();
                    deviceLimit.ReferenceId = device.ReferenceId;
                    deviceLimit.TempHigh = device.TempHigh;
                    deviceLimit.TempLow = device.TempLow;
                    string message = JsonSerializer.Serialize(deviceLimit);
                    var body = Encoding.UTF8.GetBytes(message);
                    await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: _queue, body: body);
                }

                // Short delay before disconnecting from rabbitMQ
                await Task.Delay(1000);

                // Disconnecting from rabbitMQ to save resources
                await Dispose();
                // 1 hour delay
                await Task.Delay(3600000);

                await Connect();

                // Here all messages is consumed so the queue is empty
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    Console.WriteLine("Emptying queue");

                    return Task.CompletedTask;
                };

                // Consumes the data in the queue
                await _channel.BasicConsumeAsync(_queue, true, consumer);

                // Short delay before disconnecting from rabbitMQ
                await Task.Delay(1000);
                await Dispose();
            }
        }

        // Disconnects from rabbitMQ
        private async Task<bool> Dispose()
        {
            await _channel.CloseAsync();
            await _conn.CloseAsync();
            await _channel.DisposeAsync();
            await _conn.DisposeAsync();
            return true;
        }

        // Connects to rabbitMQ
        private async Task<bool> Connect()
        {
            // Creating a new connection to rabbitMQ
            _conn = await _factory.CreateConnectionAsync();
            Console.WriteLine("AMQPClient connected");
            _channel = await _conn.CreateChannelAsync();

            // Here we connect to the queue through the channel that got created earlier
            await _channel.QueueDeclareAsync(queue: _queue, durable: false, exclusive: false, autoDelete: false);
            Console.WriteLine($"{_queue} connected");
            return true;
        }

        // The info for the factory
        private void InitFactory()
        {
            _factory.UserName = _configuration["AMQP:username"];
            _factory.Password = _configuration["AMQP:password"];
            _factory.HostName = _configuration["AMQP:host"];
            _factory.Port = Convert.ToInt32(_configuration["AMQP:port"]);
        }
    }
}
