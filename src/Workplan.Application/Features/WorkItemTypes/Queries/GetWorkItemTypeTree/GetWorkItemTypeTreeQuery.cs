using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Queries.GetWorkItemTypeTree;

public sealed record GetWorkItemTypeTreeQuery(bool IncludeInactive = false) : IRequest<Result<List<WorkItemTypeDto>>>;
