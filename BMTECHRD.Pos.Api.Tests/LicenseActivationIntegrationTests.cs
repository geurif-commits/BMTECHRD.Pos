using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public sealed class LicenseActivationIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public LicenseActivationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SeedDatabase(); // Ensure database is seeded
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Activate_WithDemoKey_ReturnsActiveLicense()
    {
        var response = await _client.PostAsJsonAsync("/api/license/activate", new
        {
            activationKey = "BMT-DEMO-00000"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var status = doc.RootElement.GetProperty("status").GetString();
        var plan = doc.RootElement.GetProperty("plan").GetString();

        Assert.Equal("ACTIVE", status);
        Assert.False(string.IsNullOrWhiteSpace(plan));
    }
}
