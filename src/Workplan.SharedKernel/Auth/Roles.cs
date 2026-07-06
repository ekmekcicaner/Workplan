namespace Workplan.SharedKernel.Auth;

public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string TechnicalOfficeEngineer = "TechnicalOfficeEngineer";
    public const string HeadOfMaster = "HeadOfMaster";
    public const string SiteChief = "SiteChief";
    public const string ProjectManager = "ProjectManager";

    public static readonly IReadOnlyList<string> All =
    [
        SystemAdmin, TechnicalOfficeEngineer, HeadOfMaster, SiteChief, ProjectManager
    ];
}
