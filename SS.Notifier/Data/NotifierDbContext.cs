namespace SS.Notifier.Data;

using Microsoft.EntityFrameworkCore;

public class NotifierDbContext : DbContext
{
    public NotifierDbContext(DbContextOptions<NotifierDbContext> options) 
        : base(options)
    {
    }

    // Add your DbSets here
    // public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entities here
    }
}