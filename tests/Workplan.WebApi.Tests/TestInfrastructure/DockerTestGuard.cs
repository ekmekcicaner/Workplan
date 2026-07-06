namespace Workplan.WebApi.Tests.TestInfrastructure;

internal static class DockerTestGuard
{
    public static bool Enabled =>
        string.Equals(Environment.GetEnvironmentVariable("WORKPLAN_RUN_DOCKER_TESTS"), "1", StringComparison.Ordinal);
}
