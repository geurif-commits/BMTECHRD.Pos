using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.App.Services;

public sealed class AuthClient
{
    private readonly HttpClient _http;

    public AuthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken cancellationToken)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshAsync(RefreshTokenRequest req, CancellationToken cancellationToken)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/refresh", req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }
}
