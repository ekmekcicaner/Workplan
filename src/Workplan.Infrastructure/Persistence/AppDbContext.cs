using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Infrastructure.Identity;
using Workplan.Infrastructure.Messaging.Outbox;

namespace Workplan.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}
