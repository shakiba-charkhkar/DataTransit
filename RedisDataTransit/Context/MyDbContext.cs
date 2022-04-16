using Microsoft.EntityFrameworkCore;
using RedisDataTransit.Models;

namespace RedisDataTransit.Context
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataTransit> DataTransits { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }
}
