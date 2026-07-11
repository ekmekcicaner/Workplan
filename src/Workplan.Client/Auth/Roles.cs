namespace Workplan.Client.Auth;

public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string TechnicalOfficeEngineer = "TechnicalOfficeEngineer";
    public const string HeadOfMaster = "HeadOfMaster";
    public const string SiteChief = "SiteChief";
    public const string ProjectManager = "ProjectManager";

    public const string Administration = SystemAdmin + "," + TechnicalOfficeEngineer;
    public const string Approval = SiteChief + "," + ProjectManager;
    public const string Reporting = Administration + "," + ProjectManager;
    public const string Tracking = Reporting + "," + SiteChief + "," + HeadOfMaster;

    public static readonly IReadOnlyList<string> All =
    [
        SystemAdmin, TechnicalOfficeEngineer, HeadOfMaster, SiteChief, ProjectManager
    ];

    public static string DisplayName(string role) => role switch
    {
        SystemAdmin => "Sistem Yöneticisi",
        TechnicalOfficeEngineer => "Teknik Ofis",
        HeadOfMaster => "Ustabaşı",
        SiteChief => "Şantiye Şefi",
        ProjectManager => "Proje Müdürü",
        _ => role
    };
}
