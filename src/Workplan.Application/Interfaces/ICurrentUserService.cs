namespace Workplan.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    IReadOnlyList<string> Roles { get; }
}
