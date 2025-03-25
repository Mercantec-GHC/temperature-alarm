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

            using var conn = await factory.CreateConnectionAsync();
            Console.WriteLine("AMQPClien connected");
            using var channel = await conn.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false);
            Console.WriteLine($"{queue} connected");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                Console.WriteLine("Received application message.");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var messageReceive = JsonSerializer.Deserialize<MQTTMessageReceive>(message);

                if (messageReceive == null || messageReceive.temperature == 0 || messageReceive.device_id == null || messageReceive.timestamp == 0)
                {
                    return Task.CompletedTask;
                }

                TemperatureLogs newLog = new TemperatureLogs();
                string refernceId = messageReceive.device_id;
                var device = _dbAccess.ReadDevice(refernceId);

                if (device == null) { return Task.CompletedTask; }

                newLog.Temperature = messageReceive.temperature;
                newLog.Date = DateTimeOffset.FromUnixTimeSeconds(messageReceive.timestamp).DateTime;
                newLog.TempHigh = device.TempHigh;
                newLog.TempLow = device.TempLow;

                _dbAccess.CreateLog(newLog, refernceId);

                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue, true, consumer);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
