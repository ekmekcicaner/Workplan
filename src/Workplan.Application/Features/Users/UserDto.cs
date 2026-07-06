namespace Workplan.Application.Features.Users;

public sealed record UserDto(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles, bool IsActive);
