using Api;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

class Program
{
    public static void Main(string[] args)
    {
        var app = CreateWebHostBuilder(args).Build();

        RunMigrations(app);

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
        await using var db = scope.ServiceProvider.GetService<DbContext>();

        if (db != null) {
            await db.Database.MigrateAsync();
        }
    }
}


