using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Services;

public interface IMainWindowSessionOrchestrator
{
    Task<SignalRClient?> ConnectSignalRAsync(string baseUrl, Func<string?> accessTokenProvider, Guid businessId);
    object BuildContentForDeviceMode(DeviceMode deviceMode, ApiClient api, SignalRClient signalR, SessionModel session);
}
