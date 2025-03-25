using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


var factory = new ConnectionFactory();
var queue = "test";


factory.UserName = "h5";
factory.Password = "Merc1234";
factory.HostName = "10.135.51.116";
factory.Port = 5672;

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
    Console.WriteLine(message);

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue, true, consumer);


const string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
Console.WriteLine(" Press enter to continue.");
Console.ReadLine();
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
Console.WriteLine(" Press enter to continue.");
Console.ReadLine();
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
Console.WriteLine(" Press enter to continue.");
Console.ReadLine();
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
Console.WriteLine(" Press enter to continue.");
Console.ReadLine();
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
Console.WriteLine(" Press enter to exit.");
Console.ReadLine();