using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Workplan.E2E.Tests;

public class CriticalFlowSmokeTests
{
    [Fact]
    [Trait("Category", "E2E")]
    public async Task Login_page_loads_when_e2e_base_url_is_configured()
    {
        var baseUrl = Environment.GetEnvironmentVariable("WORKPLAN_E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
            return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();

        var response = await page.GotoAsync(new Uri(new Uri(baseUrl), "/login").ToString());

        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();
        (await page.Locator("body").TextContentAsync()).Should().NotBeNullOrWhiteSpace();
    }
}
