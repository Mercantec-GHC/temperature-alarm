using Api.DBAccess;
using MQTTnet;
using MQTTnet.Extensions.TopicTemplate;
using System.Text;


namespace Api.MQTTReciever
{
    public class MQTTReciever
    {
        IMqttClient mqttClient;
        private readonly IConfiguration _configuration;

        public MQTTReciever(IConfiguration configuration)
        {
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
