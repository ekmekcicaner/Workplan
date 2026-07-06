using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;
using RoleNames = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Features.DailyPlans;

// Identity rolü <-> onay iş akışındaki WorkStatus karşılığı. SystemAdmin kasıtlı olarak burada
// yok: salt admin rolüyle onay/red atılamaz, admin'e ayrıca ilgili iş rolü de atanmalı.
public static class ApproverRoleMap
{
    private static readonly Dictionary<string, WorkStatus> Map = new()
    {
        [RoleNames.SiteChief] = WorkStatus.ApprovedBySiteChief,
        [RoleNames.ProjectManager] = WorkStatus.ApprovedByPM,
    };

    public static Result<WorkStatus> Resolve(IReadOnlyList<string> callerRoles)
    {
        var matches = Map.Where(kv => callerRoles.Contains(kv.Key)).Select(kv => kv.Value).Distinct().ToList();

        return matches.Count switch
        {
            1 => Result<WorkStatus>.Ok(matches[0]),
            0 => Result<WorkStatus>.Fail(Error.ScopeMismatch("Kullanıcının onay/red yetkisi bulunan bir rolü yok.")),
            _ => Result<WorkStatus>.Fail(Error.ScopeMismatch("Kullanıcının birden fazla onay rolü var, işlem belirsiz."))
        };
    }
}
