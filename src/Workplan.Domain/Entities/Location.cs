using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class Location : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    // TODO: bu alan hangi bölge kademesine karşılık geliyor netleştirilmeli (a-bölge mi b-bölge mi)
    public Guid CrewRegionId { get; private set; }
    public string Name { get; private set; } = string.Empty; // Örn: 30AAA, 30BBB
    public Guid? ParentId { get; private set; } // İç içe lokasyon ağacı için (Blok -> Kat)
    public Guid? HeadOfMasterUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation Properties
    public Project? Project { get; private set; }
    public CrewRegion? CrewRegion { get; private set; }
    public Location? Parent { get; private set; }
    private readonly List<Location> _children = new();
    public IReadOnlyCollection<Location> Children => _children.AsReadOnly();

    private Location()
    {
    }

    private Location(Guid id, Guid projectId, Guid crewRegionId, string name, Guid? parentId)
    {
        Id = id;
        ProjectId = projectId;
        CrewRegionId = crewRegionId;
        Name = name;
        ParentId = parentId;
    }

    public static Result<Location> Create(Guid projectId, Guid crewRegionId, string name, Guid? parentId = null)
    {
        if (projectId == Guid.Empty) return Result<Location>.Fail(Error.Validation("Proje ID boş olamaz."));
        if (crewRegionId == Guid.Empty)
            return Result<Location>.Fail(Error.Validation("Bölge (CrewRegion) ID boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result<Location>.Fail(Error.Validation("Lokasyon adı boş olamaz."));
        var loc = new Location(EntityId.New(), projectId, crewRegionId, name.Trim(), parentId);
        return loc;
    }

    public Result AssignHeadOfMaster(Guid headOfMasterUserId)
    {
        if (headOfMasterUserId == Guid.Empty)
            return Result.Fail(Error.Validation("Head of Master kullanıcı ID boş olamaz."));
        HeadOfMasterUserId = headOfMasterUserId;
        return Result.Ok();
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Fail(Error.Validation("Lokasyon adı boş olamaz."));
        Name = name.Trim();
        return Result.Ok();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
