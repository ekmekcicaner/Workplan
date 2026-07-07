using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Queries.GetCrewTypes;

public sealed record GetCrewTypesQuery(bool IncludeInactive) : IRequest<Result<List<CrewTypeDto>>>;
