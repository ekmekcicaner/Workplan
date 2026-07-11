using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class Project : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? PmUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<CrewRegion> _crewRegions = new();
    public IReadOnlyCollection<CrewRegion> CrewRegions => _crewRegions.AsReadOnly();

    private Project()
    {
    }

    private Project(Guid id, string code, string name, Guid? pmUserId)
    {
        Id = id;
        Code = code;
        Name = name;
        PmUserId = pmUserId;
        IsActive = true;
    }

    public static Result<Project> Create(string code, string name, Guid? pmUserId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return Result<Project>.Fail(Error.Validation("Proje kodu boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result<Project>.Fail(Error.Validation("Proje adı boş olamaz."));

        return new Project(EntityId.New(), code.Trim(), name.Trim(), pmUserId);
    }

    public Result AssignPm(Guid pmUserId)
    {
        if (pmUserId == Guid.Empty) return Result.Fail(Error.Validation("PM kullanıcı ID boş olamaz."));
        PmUserId = pmUserId;
        return Result.Ok();
    }

    public Result Update(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) return Result.Fail(Error.Validation("Proje kodu boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result.Fail(Error.Validation("Proje adı boş olamaz."));
        Code = code.Trim();
        Name = name.Trim();
        return Result.Ok();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
