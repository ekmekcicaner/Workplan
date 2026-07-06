using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionsByProject;

public sealed record GetCrewRegionsByProjectQuery(Guid ProjectId, bool IncludeInactive = false)
    : IRequest<Result<List<CrewRegionDto>>>;
