using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.Auth.Core.Services;

namespace BMTECHRD.Pos.App.Services;

public sealed class MainWindowNavigationCoordinator : IMainWindowNavigationCoordinator
{
    public object BuildStartContent(ApiClient api, AuthSessionService authSession, Action<SessionModel> onLoginSuccess)
    {
        var start = new Views.StartView();
        start.Initialize(api, authSession);
        start.OnLoginSuccess += onLoginSuccess;
        return start;
    }

    public object BuildAccessDeniedContent(SignalRClient signalR, Guid businessId)
    {
        var accessDenied = new Views.AccessDeniedView();
        accessDenied.Initialize(signalR, businessId);
        return accessDenied;
    }
}
