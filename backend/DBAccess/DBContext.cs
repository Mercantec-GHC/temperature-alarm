using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Models;

namespace DBAccess
{
    public class DBContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            string connstring = _configuration.GetConnectionString("Database");

            optionsBuilder.UseSqlite(connstring);
        }
    }
}
