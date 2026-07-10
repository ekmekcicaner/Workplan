using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Workplan.Infrastructure.Messaging.Outbox;

namespace Workplan.Infrastructure.Persistence;

// EF CLI'nin WebApi host'unu (migration/seed/background worker dahil) başlatmadan
// migration üretebilmesi için tasarım-zamanı factory.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5434;Database=workplan;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(new OutboxSaveChangesInterceptor())
            .Options;

        return new AppDbContext(options);
    }
}
