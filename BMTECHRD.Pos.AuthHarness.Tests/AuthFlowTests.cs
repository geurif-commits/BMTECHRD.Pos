using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BMTECHRD.Pos.AuthHarness.Shared;
using BMTECHRD.Pos.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class AuthFlowTests
{
    [Fact]
    [Trait("Category","AuthHarness")]
    public async Task S1_RefreshOk_RetrySucceeds()
    {
        var state = new HarnessSimState { RefreshFailMode = RefreshFailureMode.None };
        using var sp = TestHostFactory.Build(state);

        var session = sp.GetRequiredService<AuthSessionService>();
        var api = sp.GetRequiredService<ApiClient>();

        int expired = 0;
        session.SessionExpired += (_, _) => Interlocked.Increment(ref expired);

        session.SetSession("expired", "valid-refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

        var tables = await api.GetTablesAsync(Guid.NewGuid());

        Assert.Equal(1, state.RefreshAttempts);
        Assert.Equal(0, expired);
        Assert.NotNull(tables);
    }

    [Theory]
    [Trait("Category","AuthHarness")]
    [InlineData(RefreshFailureMode.Unauthorized401, "invalid-refresh")]
    [InlineData(RefreshFailureMode.ReuseDetected403, "reused_refresh")]
    public async Task RefreshRejected_TriggersSessionExpired(RefreshFailureMode mode, string refreshToken)
    {
        var state = new HarnessSimState { RefreshFailMode = mode };
        using var sp = TestHostFactory.Build(state);

        var session = sp.GetRequiredService<AuthSessionService>();
        var api = sp.GetRequiredService<ApiClient>();

        int expired = 0;
        session.SessionExpired += (_, _) => Interlocked.Increment(ref expired);

        session.SetSession("expired", refreshToken, Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

        var tables = await api.GetTablesAsync(Guid.NewGuid());

        Assert.Equal(1, state.RefreshAttempts);
        Assert.Equal(1, expired);
    }

    [Fact]
    [Trait("Category","AuthHarness")]
    public async Task S3_Concurrency_OnlyOneRefresh()
    {
        var state = new HarnessSimState { RefreshFailMode = RefreshFailureMode.None };
        using var sp = TestHostFactory.Build(state);

        var session = sp.GetRequiredService<AuthSessionService>();
        var api = sp.GetRequiredService<ApiClient>();

        session.SetSession("expired", "valid-refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var list = await api.GetTablesAsync(Guid.NewGuid());
            Assert.NotNull(list);
        });

        await Task.WhenAll(tasks);

        Assert.Equal(1, state.RefreshAttempts);
    }

    // S4 covered by parameterized test RefreshRejected_TriggersSessionExpired

    [Fact]
    [Trait("Category","AuthHarness")]
    public async Task S4b_ConcurrencyReuse_OnlyOneRefresh_OneSessionExpired()
    {
        var state = new HarnessSimState { RefreshFailMode = RefreshFailureMode.ReuseDetected403 };
        using var sp = TestHostFactory.Build(state);

        var session = sp.GetRequiredService<AuthSessionService>();
        var api = sp.GetRequiredService<ApiClient>();

        int expired = 0;
        session.SessionExpired += (_, _) => Interlocked.Increment(ref expired);

        session.SetSession("expired", "reused_refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var list = await api.GetTablesAsync(Guid.NewGuid());
            // allowed final states: empty list (401) or success if race
            Assert.NotNull(list);
        });

        await Task.WhenAll(tasks);

        Assert.Equal(1, state.RefreshAttempts);
        Assert.Equal(1, expired);
    }
}
