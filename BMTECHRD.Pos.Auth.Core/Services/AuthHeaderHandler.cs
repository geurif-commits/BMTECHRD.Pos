using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BMTECHRD.Pos.Auth.Core.Interfaces;

namespace BMTECHRD.Pos.Auth.Core.Services;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static readonly HttpRequestOptionsKey<bool> _retriedKey = new("BMTECHRD.AuthHeaderHandler.Retried");

    private readonly AuthSessionService _session;
    private readonly IAuthClient _authClient;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(AuthSessionService session, IAuthClient authClient, Microsoft.Extensions.Logging.ILogger<AuthHeaderHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(authClient);
        ArgumentNullException.ThrowIfNull(logger);
        _session = session;
        _authClient = authClient;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var _scope = _logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
        {
            ["Area"] = "Auth",
            ["Path"] = request.RequestUri?.AbsolutePath ?? "(null)"
        });

        _logger.LogInformation("Start processing {Method} {Path}", request.Method, request.RequestUri?.AbsolutePath);

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

        if (request.Options.TryGetValue(_retriedKey, out var alreadyRetried) && alreadyRetried)
            return response;

        var tokenBefore = _session.AccessToken;

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessToken != tokenBefore)
            {
                // already refreshed
            }
            else
            {
                var refreshToken = _session.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _session.Clear();
                    return response;
                }

                _logger.LogInformation("401 detected. Attempting refresh for device {DeviceId}.", _session.DeviceId);

                var refreshed = await _authClient
                    .RefreshAsync(new BMTECHRD.Pos.Application.DTOs.RefreshTokenRequest { RefreshToken = refreshToken, DeviceId = _session.DeviceId }, cancellationToken)
                    .ConfigureAwait(false);

                if (refreshed != null)
                {
                    _logger.LogInformation("Refresh successful");
                }
                else
                {
                    _logger.LogWarning("Refresh failed or returned null");
                }

                if (refreshed == null ||
                    string.IsNullOrWhiteSpace(refreshed.AccessToken) ||
                    string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                {
                    _logger.LogWarning("Refresh failed - clearing session");
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

#pragma warning disable CA2000 // Dispose objects before losing scope - ownership transferred to HttpClient.SendAsync which will dispose
        var retryRequest = await CloneHttpRequestMessageAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA2000
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