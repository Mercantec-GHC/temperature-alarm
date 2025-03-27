using Api.DBAccess;
using Api.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Api.AMQPReciever
{
    public class AMQPReciever
    {
        private readonly IConfiguration _configuration;
        private readonly DbAccess _dbAccess;
        private IConnection _conn;
        private IChannel _channel;
        private ConnectionFactory _factory;
        private string _queue;

        public AMQPReciever(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
            _factory = new ConnectionFactory();
            _queue = "temperature-logs";

            InitFactory();
        }

        public async Task Handle_Received_Application_Message()
        {
            await Connect();

            // Everytime a message is recieved from the queue it goes into this consumer.ReceivedAsync
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                Console.WriteLine("Received application message.");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var messageReceive = JsonSerializer.Deserialize<MessageReceive>(message);

                // Checks if the message has the data we need
                if (messageReceive == null || messageReceive.device_id == null || messageReceive.timestamp == 0)
                {
                    return Task.CompletedTask;
                }

                // Convert to the model we use in the database and gets the device from the database that is used for getting the current set temphigh and templow
                TemperatureLogs newLog = new TemperatureLogs();
                string refernceId = messageReceive.device_id;
                var device = _dbAccess.ReadDevice(refernceId);

                // Checks if the device exist if it doesn't it throws the data away
                if (device == null) { return Task.CompletedTask; }

                newLog.Temperature = messageReceive.temperature;
                newLog.Date = DateTimeOffset.FromUnixTimeSeconds(messageReceive.timestamp).DateTime;
                newLog.TempHigh = device.TempHigh;
                newLog.TempLow = device.TempLow;

                // Send the data to dbaccess to be saved
                _dbAccess.CreateLog(newLog, refernceId);

                return Task.CompletedTask;
            };

            // Consumes the data in the queue
            await _channel.BasicConsumeAsync(_queue, true, consumer);

			while (true);

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
