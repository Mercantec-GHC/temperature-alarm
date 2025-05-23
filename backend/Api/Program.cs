using Api;
using Api.AMQP;
using Api.AMQPReciever;
using Api.DBAccess;
using Api.MQTTReciever;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

class Program
{

    public static void Main(string[] args)
    {
        var app = CreateWebHostBuilder(args).Build();
        string rabbitMQ = "AMQP"; // This value has to be either "AMQP" or "MQTT"

        RunMigrations(app);

        Task.Run(() =>
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var dbAccess = services.GetRequiredService<DbAccess>();

                // Choose to either connect AMQP or MQTT
                if (rabbitMQ == "AMQP")
                {
                    AMQPReciever amqpReciever = new AMQPReciever(configuration, dbAccess);
                    amqpReciever.Handle_Received_Application_Message().Wait();
                }
                else if (rabbitMQ == "MQTT")
                {
                    MQTTReciever mqtt = new MQTTReciever(configuration, dbAccess);
                    mqtt.Handle_Received_Application_Message().Wait();
                }                
            }
        });


        app.Run();
    }

    // Calls the startup class and creates the webinterface
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:5000")
                .UseStartup<Startup>();

    public static async void RunMigrations(IWebHost app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetService<DBContext>();

        if (db != null) {
            await db.Database.MigrateAsync();
        }
    }
}


