using Mediator;
using Workplan.Application.Features.Auth.Commands;
using Workplan.Application.Features.Users.Commands;
using Workplan.Application.Features.Users.Queries.GetUsers;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", async (string? role, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetUsersQuery(role), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPost("/", async (RegisterCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin));

        group.MapPut("/{id:guid}/roles",
                async (Guid id, UpdateUserRolesRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new UpdateUserRolesCommand(id, request.Roles), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetUserActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin));

        group.MapPost("/{id:guid}/reset-password",
                async (Guid id, ResetPasswordRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new ResetUserPasswordCommand(id, request.NewPassword), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin));

        return app;
    }
}

public sealed record UpdateUserRolesRequest(IReadOnlyList<string> Roles);

public sealed record ResetPasswordRequest(string NewPassword);
