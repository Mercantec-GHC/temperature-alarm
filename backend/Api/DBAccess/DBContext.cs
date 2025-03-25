using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.Models.User;

namespace Api
{
    public class DBContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    }
}
