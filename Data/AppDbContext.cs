using Microsoft.EntityFrameworkCore;

namespace CloudMediaHub.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {  }

        public DbSet<Entities.MediaFile> MediaFiles { get; set; }
    }
}
