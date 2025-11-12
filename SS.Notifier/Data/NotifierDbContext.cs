using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data;

using Microsoft.EntityFrameworkCore;

public class NotifierDbContext : DbContext
{
    public NotifierDbContext(DbContextOptions<NotifierDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<ApartmentEntity> Aparments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entities here
    }
    
    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ApartmentEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ApartmentEntity apartment = (ApartmentEntity)entry.Entity;
        
            if (entry.State == EntityState.Added)
                apartment.CreatedAt = DateTime.UtcNow;
        
            apartment.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }
}