using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public sealed record SetLocationActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
