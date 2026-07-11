using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
        builder.UseSetting("ConnectionStrings:Default", _container.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", "Workplan.Tests");
        builder.UseSetting("Jwt:Audience", "Workplan.Tests");
        builder.UseSetting("Jwt:SigningKey", "WORKPLAN_TEST_SIGNING_KEY_32_CHARS_MIN");
        builder.UseSetting("InitialAdmin:Email", "admin@test.local");
        builder.UseSetting("InitialAdmin:Password", "ChangeMe123!");
        builder.UseSetting("InitialAdmin:FullName", "Test Admin");
        builder.UseSetting("SeedDemoData", "false");
    }
}
