using Api;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

var app = WebHost.CreateDefaultBuilder(args)
	.UseUrls("http://0.0.0.0:5000")
	.UseStartup<Startup>()
	.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetService<DBContext>();
await db.Database.MigrateAsync();

app.Run();

