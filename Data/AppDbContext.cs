using Microsoft.EntityFrameworkCore;
using Sync_Data_API.Models;
namespace Sync_Data_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<PersonNew> PersonNews { get; set; }
    }
}
