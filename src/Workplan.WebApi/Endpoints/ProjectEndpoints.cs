using Mediator;
using Workplan.Application.Features.Projects.Commands;
using Workplan.Application.Features.Projects.Queries.GetProjectById;
using Workplan.Application.Features.Projects.Queries.GetProjects;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapPost("/", async (CreateProjectCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapGet("/", async (bool? includeInactive, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetProjectsQuery(includeInactive ?? false), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetProjectByIdQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateProjectRequest request, ISender sender, CancellationToken ct)
                => (await sender.Send(new UpdateProjectCommand(id, request.Code, request.Name), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetProjectActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        return app;
    }
}

public sealed record UpdateProjectRequest(string Code, string Name);
