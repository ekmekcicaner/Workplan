using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Workplan.E2E.Tests;

public class CriticalFlowSmokeTests
{
    [Theory]
    [Trait("Category", "E2E")]
    [InlineData("/login", 390, 844)]
    [InlineData("/login", 1440, 960)]
    [InlineData("/work-item-types", 390, 844)]
    [InlineData("/work-item-types", 1440, 960)]
    [InlineData("/daily-plans/new", 390, 844)]
    [InlineData("/daily-plans/new", 1440, 960)]
    [InlineData("/my-work", 390, 844)]
    [InlineData("/my-work", 1440, 960)]
    [InlineData("/approvals", 390, 844)]
    [InlineData("/approvals", 1440, 960)]
    [InlineData("/daily-tracking", 390, 844)]
    [InlineData("/daily-tracking", 1440, 960)]
    [InlineData("/reports", 390, 844)]
    [InlineData("/reports", 1440, 960)]
    public async Task Critical_routes_load_without_layout_overflow_when_e2e_base_url_is_configured(string path, int width, int height)
    {
        var baseUrl = Environment.GetEnvironmentVariable("WORKPLAN_E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
            return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = width,
                Height = height
            }
        });

        var response = await page.GotoAsync(new Uri(new Uri(baseUrl), path).ToString());

        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();
        (await page.Locator("body").TextContentAsync()).Should().NotBeNullOrWhiteSpace();
        var hasHorizontalOverflow = await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > window.innerWidth + 2");
        hasHorizontalOverflow.Should().BeFalse();
    }
}
