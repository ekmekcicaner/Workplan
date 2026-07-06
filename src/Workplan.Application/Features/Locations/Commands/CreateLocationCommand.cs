using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public sealed record CreateLocationCommand(
    Guid ProjectId,
    Guid CrewRegionId,
    string Name,
    Guid? ParentId) : IRequest<Result<Guid>>;
