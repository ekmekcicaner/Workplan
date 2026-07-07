using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Common;
using Workplan.Domain.Entities;
using Workplan.Infrastructure.Identity;

namespace Workplan.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly IPublisher _publisher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<CrewRegion> CrewRegions => Set<CrewRegion>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<CrewType> CrewTypes => Set<CrewType>();
    public DbSet<WorkItemType> WorkItemTypes => Set<WorkItemType>();
    public DbSet<DailyPlan> DailyPlans => Set<DailyPlan>();
    public DbSet<StatusTransition> StatusTransitions => Set<StatusTransition>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregatesWithEvents)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
