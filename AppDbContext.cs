using Microsoft.EntityFrameworkCore;

namespace WebAPIWithHangFire
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
        public DbSet<Item> Items { get; set; } = default;

    }
}
