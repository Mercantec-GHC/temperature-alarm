using Api;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

public class Program
{
    private static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }
    // Calls the startup class and creates the webinterface
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}