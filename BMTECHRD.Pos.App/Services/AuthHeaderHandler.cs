using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BMTECHRD.Pos.App.Services;

/// <summary>
/// DelegatingHandler que inyecta JWT access token en cada request y maneja auto-refresh en 401.
/// ETAPA 9 (HARDENED):
/// - Evita loops (excluye login/refresh/logout)
/// - Control de concurrencia (1 refresh a la vez)
/// - Reintento seguro (clona HttpRequestMessage para POST/PUT con body)
/// - DeviceId enviado en refresh para Device Binding
/// - Resuelve ciclo ApiClient <-> HttpClient usando SetApiClient(...)
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static readonly HttpRequestOptionsKey<bool> _retriedKey = new("BMTECHRD.AuthHeaderHandler.Retried");

    private readonly AuthSessionService _session;

    // Se setea después de construir HttpClient+ApiClient
    private ApiClient? _apiClient;

    public AuthHeaderHandler(AuthSessionService session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
    }

    /// <summary>
    /// Se llama una vez luego de crear ApiClient.
    /// </summary>
    public void SetApiClient(ApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        AttachBearerIfNeeded(request);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (IsAuthEndpoint(request.RequestUri))
            return response;

        if (string.IsNullOrWhiteSpace(_session.RefreshToken))
        {
            _session.Clear();
            return response;
        }

        // evita loop infinito
        if (request.Options.TryGetValue(_retriedKey, out var alreadyRetried) && alreadyRetried)
            return response;

        // si todavía no se seteo ApiClient, no podemos refrescar
        if (_apiClient is null)
            return response;

        var tokenBefore = _session.AccessToken;

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // si otro request ya refrescó mientras esperábamos
            if (!string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessToken != tokenBefore)
            {
                // ya refrescado por otro thread
            }
            else
            {
                var refreshToken = _session.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _session.Clear();
                    return response;
                }

                // ✅ ETAPA 9: enviar DeviceId en refresh
                var refreshed = await _apiClient
                    .RefreshTokenAsync(refreshToken, _session.DeviceId, cancellationToken)
                    .ConfigureAwait(false);

                if (refreshed == null ||
                    string.IsNullOrWhiteSpace(refreshed.AccessToken) ||
                    string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                {
                    _session.Clear();
                    return response;
                }

                _session.UpdateTokens(refreshed.AccessToken, refreshed.RefreshToken, refreshed.ExpiresAt);
            }
        }
        finally
        {
            _refreshLock.Release();
        }

        response.Dispose();

        var retryRequest = await CloneHttpRequestMessageAsync(request, cancellationToken).ConfigureAwait(false);
        retryRequest.Options.Set(_retriedKey, true);

        AttachBearerIfNeeded(retryRequest);

        return await base.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
    }

    private void AttachBearerIfNeeded(HttpRequestMessage request)
    {
        if (IsAuthEndpoint(request.RequestUri))
            return;

        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }
    }

    private static bool IsAuthEndpoint(Uri? uri)
    {
        if (uri is null) return false;
        var path = (uri.PathAndQuery ?? string.Empty).ToLowerInvariant();

        return path.Contains("/api/auth/login")
            || path.Contains("/api/auth/refresh")
            || path.Contains("/api/auth/logout");
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage original, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var opt in original.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(opt.Key), opt.Value);

        if (original.Content != null)
        {
            var ms = new MemoryStream();
            await original.Content.CopyToAsync(ms, ct).ConfigureAwait(false);
            ms.Position = 0;

            var newContent = new StreamContent(ms);

            foreach (var h in original.Content.Headers)
                newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);

            clone.Content = newContent;
        }

        return clone;
    }
}