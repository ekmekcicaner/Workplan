using Microsoft.EntityFrameworkCore;
using Workplan.Domain.Entities;

namespace Workplan.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<CrewRegion> CrewRegions { get; }
    DbSet<Location> Locations { get; }
    DbSet<CrewType> CrewTypes { get; }
    DbSet<WorkItemType> WorkItemTypes { get; }
    DbSet<DailyPlan> DailyPlans { get; }
    DbSet<StatusTransition> StatusTransitions { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
