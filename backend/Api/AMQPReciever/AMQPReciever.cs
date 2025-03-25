using Api.DBAccess;
using Api.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Api.AMQPReciever
{
    public class AMQPReciever
    {
        private readonly IConfiguration _configuration;
        private readonly DbAccess _dbAccess;

        public AMQPReciever(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
        }

        public async Task Handle_Received_Application_Message()
        {
            var factory = new ConnectionFactory();
            var queue = "temperature-logs";

            factory.UserName = _configuration["AMQP:username"];
            factory.Password = _configuration["AMQP:password"];
            factory.HostName = _configuration["AMQP:host"];
            factory.Port = Convert.ToInt32(_configuration["AMQP:port"]);

            // Connecting to our rabbitmq and after that it create's a channel where you can connect to a queue
            using var conn = await factory.CreateConnectionAsync();
            Console.WriteLine("AMQPClien connected");
            using var channel = await conn.CreateChannelAsync();

            // Here we connect to the queue through the channel that got created earlier
            await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false);
            Console.WriteLine($"{queue} connected");

            // Everytime a message is recieved from the queue it goes into this consumer.ReceivedAsync
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                Console.WriteLine("Received application message.");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var messageReceive = JsonSerializer.Deserialize<MQTTMessageReceive>(message);

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
            await channel.BasicConsumeAsync(queue, true, consumer);

			while (true);
        }
    }
}
