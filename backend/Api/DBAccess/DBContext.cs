using Microsoft.EntityFrameworkCore;
using Api.Models.Users;
using Api.Models.Devices;

namespace Api
{
    public class DBContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    }
}
