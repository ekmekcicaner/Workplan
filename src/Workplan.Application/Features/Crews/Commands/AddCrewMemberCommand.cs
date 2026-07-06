using Mediator;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Commands;

public sealed record AddCrewMemberCommand(Guid CrewId, WorkerType WorkerType, string PersonnelRef)
    : IRequest<Result<Guid>>;
