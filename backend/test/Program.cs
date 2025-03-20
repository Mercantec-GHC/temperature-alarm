using MQTTnet;

var mqttFactory = new MqttClientFactory();
IMqttClient mqttClient;

using (mqttClient = mqttFactory.CreateMqttClient())
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer($"10.135.51.116", 1883)
            .WithCredentials($"h5", $"Merc1234")
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
