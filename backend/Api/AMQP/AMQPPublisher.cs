using Api.DBAccess;
using Api.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Api.AMQP
{
    public class AMQPPublisher
    {
        private readonly IConfiguration _configuration;
        private IConnection _conn;
        private IChannel _channel;
        private ConnectionFactory _factory;
        private string _queue;

        public AMQPPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory();
            _queue = "temperature-limits";

            InitFactory();
        }

        public async void Handle_Push_Device_Limits(DeviceLimit deviceLimit)
        {
            // Connecting to rabbitMQ
            await Connect();

            string message = JsonSerializer.Serialize(deviceLimit);
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange: _queue, routingKey: _queue, body: body);


            // Short delay before disconnecting from rabbitMQ
            await Task.Delay(1000);

            // Disconnecting from rabbitMQ to save resources
            await Dispose();

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
            await _channel.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false);
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
