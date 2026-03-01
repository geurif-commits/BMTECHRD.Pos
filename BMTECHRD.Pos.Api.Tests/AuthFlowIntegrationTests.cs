using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public sealed class AuthFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Then_Me_ReturnsAuthenticatedIdentity()
    {
        var businessId = await GetAnyBusinessIdAsync();

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            businessId,
            username = "admin",
            password = "admin",
            deviceId = "integration-device-1"
        });

        if (loginResp.StatusCode != HttpStatusCode.OK)
        {
            var body = await loginResp.Content.ReadAsStringAsync();
            Console.WriteLine("LOGIN RESPONSE BODY:\n" + body);
            throw new InvalidOperationException($"Login failed: {loginResp.StatusCode}");
        }

        using var loginDoc = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
        var accessToken = loginDoc.RootElement.GetProperty("accessToken").GetString();
        var loginBusinessId = loginDoc.RootElement.GetProperty("businessId").GetGuid();
        var username = loginDoc.RootElement.GetProperty("username").GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.Equal(businessId, loginBusinessId);
        Assert.Equal("admin", username);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResp = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);

        using var meDoc = JsonDocument.Parse(await meResp.Content.ReadAsStringAsync());
        var meBusinessId = meDoc.RootElement.GetProperty("businessId").GetGuid();
        var meRole = meDoc.RootElement.GetProperty("role").GetString();
        var meUsername = meDoc.RootElement.GetProperty("username").GetString();

        Assert.Equal(businessId, meBusinessId);
        Assert.Equal("ADMIN", meRole);
        Assert.Equal("admin", meUsername);
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesRefreshToken()
    {
        var businessId = await GetAnyBusinessIdAsync();

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            businessId,
            username = "admin",
            password = "admin",
            deviceId = "integration-device-2"
        });

        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        using var loginDoc = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
        var refreshToken = loginDoc.RootElement.GetProperty("refreshToken").GetString();

        var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken,
            deviceId = "integration-device-2"
        });

        Assert.Equal(HttpStatusCode.OK, refreshResp.StatusCode);

        using var refreshDoc = JsonDocument.Parse(await refreshResp.Content.ReadAsStringAsync());
        var nextRefreshToken = refreshDoc.RootElement.GetProperty("refreshToken").GetString();
        var nextAccessToken = refreshDoc.RootElement.GetProperty("accessToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(nextRefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(nextAccessToken));
        Assert.NotEqual(refreshToken, nextRefreshToken);
    }

    private async Task<Guid> GetAnyBusinessIdAsync()
    {
        var businessResp = await _client.GetAsync("/api/business/public");
        Assert.Equal(HttpStatusCode.OK, businessResp.StatusCode);

        using var businessDoc = JsonDocument.Parse(await businessResp.Content.ReadAsStringAsync());
        var first = businessDoc.RootElement.EnumerateArray().FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("No public business found for integration test setup.");

        return first.GetProperty("businessId").GetGuid();
    }
}
