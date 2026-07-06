using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionById;

public sealed record GetCrewRegionByIdQuery(Guid Id) : IRequest<Result<CrewRegionDto>>;
