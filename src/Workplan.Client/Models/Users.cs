namespace Workplan.Client.Models;

public record UserDto(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles, bool IsActive);

public record CreateUserRequest(string Email, string Password, string FullName, IReadOnlyList<string> Roles);

public record UpdateUserRolesRequest(IReadOnlyList<string> Roles);

public record ResetPasswordRequest(string NewPassword);
