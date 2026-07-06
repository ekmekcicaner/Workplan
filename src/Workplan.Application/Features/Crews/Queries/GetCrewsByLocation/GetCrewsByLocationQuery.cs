using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Queries.GetCrewsByLocation;

public sealed record GetCrewsByLocationQuery(Guid LocationId) : IRequest<Result<List<CrewDto>>>;
