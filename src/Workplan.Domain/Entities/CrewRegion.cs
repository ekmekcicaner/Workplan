using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class CrewRegion : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Code { get; private set; } = string.Empty; // Örn: abolge, bbolge
    public string Name { get; private set; } = string.Empty;
    public Guid? SiteChiefUserId { get; private set; }
    public Guid? TechOfficeUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Project? Project { get; private set; }

    private readonly List<Location> _locations = new();
    public IReadOnlyCollection<Location> Locations => _locations.AsReadOnly();

    private CrewRegion()
    {
    }

    private CrewRegion(Guid id, Guid projectId, string code, string name)
    {
        Id = id;
        ProjectId = projectId;
        Code = code;
        Name = name;
    }

    public static Result<CrewRegion> Create(Guid projectId, string code, string name)
    {
        if (projectId == Guid.Empty) return Result<CrewRegion>.Fail(Error.Validation("Proje ID boş olamaz."));
        if (string.IsNullOrWhiteSpace(code)) return Result<CrewRegion>.Fail(Error.Validation("Bölge kodu boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result<CrewRegion>.Fail(Error.Validation("Bölge adı boş olamaz."));

        return new CrewRegion(EntityId.New(), projectId, code.Trim(), name.Trim());
    }

    public Result AssignSiteChief(Guid siteChiefUserId)
    {
        if (siteChiefUserId == Guid.Empty) return Result.Fail(Error.Validation("Site Chief kullanıcı ID boş olamaz."));
        SiteChiefUserId = siteChiefUserId;
        return Result.Ok();
    }

    public Result AssignTechOffice(Guid techOfficeUserId)
    {
        if (techOfficeUserId == Guid.Empty) return Result.Fail(Error.Validation("Tech Office kullanıcı ID boş olamaz."));
        TechOfficeUserId = techOfficeUserId;
        return Result.Ok();
    }

    public Result Update(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) return Result.Fail(Error.Validation("Bölge kodu boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result.Fail(Error.Validation("Bölge adı boş olamaz."));
        Code = code.Trim();
        Name = name.Trim();
        return Result.Ok();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
