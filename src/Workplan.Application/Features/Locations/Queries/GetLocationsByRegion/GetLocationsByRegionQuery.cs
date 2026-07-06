using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationsByRegion;

public sealed record GetLocationsByRegionQuery(Guid CrewRegionId, bool IncludeInactive = false)
    : IRequest<Result<List<LocationDto>>>;
