using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.Extensions.Logging;
using BMTECHRD.Pos.Auth.Core.Interfaces;

namespace BMTECHRD.Pos.Auth.Core.Services;

public sealed class AuthClient : IAuthClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AuthClient> _logger;

    public AuthClient(HttpClient http, ILogger<AuthClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken cancellationToken)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed with status {Status}", (int)resp.StatusCode);
                return null;
            }
            return await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login exception");
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshAsync(RefreshTokenRequest req, CancellationToken cancellationToken)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/refresh", req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Refresh rejected by server with status {Status}", (int)resp.StatusCode);
                    return null;
                }

                _logger.LogWarning("Refresh failed with status {Status}", (int)resp.StatusCode);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh exception");
            return null;
        }
    }
}
