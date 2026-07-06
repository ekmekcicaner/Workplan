using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public sealed record SetProjectActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
