using Microsoft.EntityFrameworkCore;
using Sync_Data_API.Models;
namespace Sync_Data_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<Person> Persons { get; set; }
        public DbSet<PersonNew> PersonNews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>().HasData(
                new Person { Id = 1, Name = "bima", Age = 30 },
                new Person { Id = 2, Name = "yoga", Age = 25 },
                new Person { Id = 3, Name = "ciko", Age = 40 },
                new Person { Id = 4, Name = "wudd", Age = 35 }
            );
        }
    }
}
