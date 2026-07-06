using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

// Gün Sonu: Gerçekleşen Quantity, Man-Day, Overtime ve varsa gerekçe/ZZZ detayı girilerek onaya sunulur.
public sealed record SubmitProgressCommand(
    Guid DailyPlanId,
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment) : IRequest<Result>;
