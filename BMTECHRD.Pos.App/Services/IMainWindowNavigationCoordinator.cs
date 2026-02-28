using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.Auth.Core.Services;

namespace BMTECHRD.Pos.App.Services;

public interface IMainWindowNavigationCoordinator
{
    object BuildStartContent(ApiClient api, AuthSessionService authSession, Action<Models.SessionModel> onLoginSuccess);
    object BuildAccessDeniedContent(SignalRClient signalR, Guid businessId);
}
