using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public sealed record UpdateLocationCommand(Guid Id, string Name) : IRequest<Result>;
