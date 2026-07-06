namespace Workplan.Application.Interfaces;

public sealed record AuthenticatedUser(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);
