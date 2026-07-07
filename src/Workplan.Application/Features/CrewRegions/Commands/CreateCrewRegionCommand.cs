using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public sealed record CreateCrewRegionCommand(
    Guid ProjectId,
    string Code,
    string Name,
    Guid? SiteChiefUserId = null,
    Guid? TechOfficeUserId = null) : IRequest<Result<Guid>>;
