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
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer($"{_configuration["MQTT:host"]}", 1883)
                    .WithCredentials($"{_configuration["MQTT:username"]}", $"{_configuration["MQTT:password"]}")
                    .WithCleanSession()
                    .Build();

                // Setup message handling before connecting so that queued messages
                // are also handled properly. When there is no event handler attached all
                // received messages get lost.
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("Received application message.");

                    string sensorData = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    var mqttMessageReceive = JsonSerializer.Deserialize<MQTTMessageReceive>(sensorData);

                    if (mqttMessageReceive == null || mqttMessageReceive.temperature == 0 || mqttMessageReceive.device_id == null || mqttMessageReceive.timestamp == null)
                    {
                        return Task.CompletedTask;
                    }

                    TemperatureLogs newLog = new TemperatureLogs();
                    string refernceId = mqttMessageReceive.device_id;
                    var device = _dbAccess.ReadDevice(refernceId);

                    if (device == null) { return Task.CompletedTask; }

                    newLog.Temperature = mqttMessageReceive.temperature;
                    newLog.Date = DateTimeOffset.FromUnixTimeSeconds(mqttMessageReceive.timestamp).DateTime;
                    newLog.TempHigh = device.TempHigh;
                    newLog.TempLow = device.TempLow;

                    _dbAccess.CreateLog(newLog, refernceId);

                    return Task.CompletedTask;
                };


                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                Console.WriteLine("mqttClient");

                //var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicTemplate(topic).Build();

                await mqttClient.SubscribeAsync("temperature");

                Console.WriteLine("MQTT client subscribed to topic.");

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
