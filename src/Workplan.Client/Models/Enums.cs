namespace Workplan.Client.Models;

public enum WorkStatus
{
    Draft = 1,
    Assigned = 2,
    InProgress = 3,
    Submitted = 4,
    ApprovedBySiteChief = 6,
    ApprovedByPM = 7
}

public enum Unit
{
    None,
    Ton,
    M3,
    M2
}

public enum DailyPlanCommentKind
{
    Progress,
    Rejection
}
