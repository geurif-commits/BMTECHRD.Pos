using System.Windows.Controls;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Services;

public sealed class MainWindowSessionOrchestrator : IMainWindowSessionOrchestrator
{
    public async Task<SignalRClient?> ConnectSignalRAsync(string baseUrl, Func<string?> accessTokenProvider, Guid businessId)
    {
        var signalR = new SignalRClient(baseUrl, () => Task.FromResult(accessTokenProvider()));
        try
        {
            await signalR.ConnectAsync();
            await signalR.JoinBusinessAsync(businessId);
            return signalR;
        }
        catch
        {
            return signalR;
        }
    }

    public object BuildContentForDeviceMode(DeviceMode deviceMode, ApiClient api, SignalRClient signalR, SessionModel session)
    {
        return deviceMode switch
        {
            DeviceMode.Server => BuildServerContent(api, signalR, session),
            DeviceMode.Cashier => BuildCashierContent(api, signalR, session),
            DeviceMode.Kitchen => BuildKitchenContent(api, signalR, session.BusinessId),
            DeviceMode.Bar => BuildBarContent(api, signalR, session.BusinessId),
            DeviceMode.Floor => BuildFloorContent(api, signalR, session),
            _ => new Views.AccessDeniedView()
        };
    }

    private static object BuildServerContent(ApiClient api, SignalRClient signalR, SessionModel session)
    {
        var adminView = new Views.Admin.AdminView();
        adminView.Initialize(api, session.BusinessId, session.UserId, signalR, session.Role);
        return adminView;
    }

    private static object BuildCashierContent(ApiClient api, SignalRClient signalR, SessionModel session)
    {
        var tab = new TabControl();

        var cashView = new Views.CashierView();
        cashView.Initialize(api, signalR, session.BusinessId, session.UserId);
        tab.Items.Add(new TabItem { Header = "Caja", Content = cashView });

        if (session.Role?.ToUpperInvariant() == "SUPERVISOR")
        {
            var adminView = new Views.Admin.AdminView();
            adminView.Initialize(api, session.BusinessId, session.UserId);
            tab.Items.Add(new TabItem { Header = "Admin", Content = adminView });
        }

        tab.SelectedIndex = 0;
        return tab;
    }

    private static object BuildKitchenContent(ApiClient api, SignalRClient signalR, Guid businessId)
    {
        var kitchenView = new Views.KitchenQueueView();
        kitchenView.Initialize(api, signalR, businessId);
        return kitchenView;
    }

    private static object BuildBarContent(ApiClient api, SignalRClient signalR, Guid businessId)
    {
        var barView = new Views.BarQueueView();
        barView.Initialize(api, signalR, businessId);
        return barView;
    }

    private static object BuildFloorContent(ApiClient api, SignalRClient signalR, SessionModel session)
    {
        var tablesView = new Views.TablesMapView();
        tablesView.Initialize(api, signalR, session.BusinessId, session.UserId);
        return tablesView;
    }
}
