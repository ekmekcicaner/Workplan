using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IRequest<Result<LocationDto>>;
