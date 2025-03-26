using Api.DBAccess;
using Api.Models;
using MQTTnet;
using System.Text;
using System.Text.Json;


namespace Api.MQTTReciever
{
    public class MQTTReciever
    {
        IMqttClient mqttClient;
        private readonly IConfiguration _configuration;
        private readonly DbAccess _dbAccess;

        public MQTTReciever(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
        }


        public async Task Handle_Received_Application_Message()
        {
            var mqttFactory = new MqttClientFactory();

            using (mqttClient = mqttFactory.CreateMqttClient())
            {
                // Entering our values for conecting to MQTT
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer($"{_configuration["MQTT:host"]}", Convert.ToInt32(_configuration["MQTT:port"]))
                    .WithCredentials($"{_configuration["MQTT:username"]}", $"{_configuration["MQTT:password"]}")
                    .WithCleanSession()
                    .Build();

                // Everytime a message is recieved from the queue it goes into this mqttClient.ApplicationMessageReceivedAsync
                // Setup message handling before connecting so that queued messages
                // are also handled properly. When there is no event handler attached all
                // received messages get lost.
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("Received application message.");

                    string sensorData = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    var messageReceive = JsonSerializer.Deserialize<MessageReceive>(sensorData);

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

                // Starts the connection to rabbitmq
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                Console.WriteLine("mqttClient");

                // Subscribes to our topic
                await mqttClient.SubscribeAsync("temperature");

                Console.WriteLine("MQTT client subscribed to topic.");

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
