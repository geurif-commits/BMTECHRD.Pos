using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class HealthEndpointIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public HealthEndpointIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SeedDatabase(); // Ensure database is seeded
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
