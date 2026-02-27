using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.AuthHarness.Shared;

public enum RefreshFailureMode
{
    None = 0,
    Unauthorized401 = 1,
    ReuseDetected403 = 2
}

public class HarnessSimState
{
    public RefreshFailureMode RefreshFailMode { get; set; } = RefreshFailureMode.None;
    public bool RotateRefreshTokenOnSuccess { get; set; } = true;
    public int RefreshAttempts { get; set; }
    public bool TokenUpdated { get; set; }

    public void Reset()
    {
        RefreshFailMode = RefreshFailureMode.None;
        RotateRefreshTokenOnSuccess = true;
        RefreshAttempts = 0;
        TokenUpdated = false;
    }
}

public class FakeAuthHandler : HttpMessageHandler
{
    private readonly HarnessSimState _state;
    public FakeAuthHandler(HarnessSimState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath?.Contains("/api/auth/refresh") == true)
        {
            _state.RefreshAttempts++;

            var body = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult() ?? string.Empty;
            string? refreshToken = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("refreshToken", out var rt))
                    refreshToken = rt.GetString();
            }
            catch { }

            if (string.Equals(refreshToken, "reused_refresh", StringComparison.Ordinal))
            {
                if (_state.RefreshFailMode == RefreshFailureMode.ReuseDetected403)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Refresh token reuse detected") });
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Invalid refresh") });
                }
            }

            if (string.Equals(refreshToken, "valid-refresh", StringComparison.Ordinal))
            {
                _state.TokenUpdated = true;
                var newRefresh = _state.RotateRefreshTokenOnSuccess ? "valid_refresh_rotated" : "valid-refresh";
                var resp = new LoginResponse
                {
                    AccessToken = "new-access",
                    RefreshToken = newRefresh,
                    UserId = Guid.NewGuid(),
                    BusinessId = Guid.NewGuid(),
                    Username = "harness",
                    Role = "ADMIN",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };
                var json = JsonSerializer.Serialize(resp);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Invalid refresh") });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

public class FakeApiHandler : HttpMessageHandler
{
    private readonly HarnessSimState _state;
    public FakeApiHandler(HarnessSimState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath?.Contains("/api/tables") == true)
        {
            if (!_state.TokenUpdated)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            else
            {
                var json = JsonSerializer.Serialize(new object[0]);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
