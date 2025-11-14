using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SS.Notifier.Data;

public class NotifierDbContextFactory : IDesignTimeDbContextFactory<NotifierDbContext>
{
    public NotifierDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connStr = configuration.GetConnectionString("Postgres")
                      ?? throw new InvalidOperationException("Missing connection string");

        var optionsBuilder = new DbContextOptionsBuilder<NotifierDbContext>();
        optionsBuilder.UseNpgsql(connStr);

        return new NotifierDbContext(optionsBuilder.Options);
    }
}