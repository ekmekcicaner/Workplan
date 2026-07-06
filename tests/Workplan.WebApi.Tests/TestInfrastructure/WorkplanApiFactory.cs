using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Workplan.WebApi.Tests.TestInfrastructure;

public sealed class WorkplanApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("workplan_api_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        if (!DockerTestGuard.Enabled)
            return;

        await _container.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        if (DockerTestGuard.Enabled)
            await _container.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _container.GetConnectionString(),
                ["Jwt:Issuer"] = "Workplan.Tests",
                ["Jwt:Audience"] = "Workplan.Tests",
                ["Jwt:SigningKey"] = "WORKPLAN_TEST_SIGNING_KEY_32_CHARS_MIN",
                ["InitialAdmin:Email"] = "admin@test.local",
                ["InitialAdmin:Password"] = "ChangeMe123!",
                ["InitialAdmin:FullName"] = "Test Admin",
                ["SeedDemoData"] = "false"
            };
            config.AddInMemoryCollection(values);
        });
    }
}
