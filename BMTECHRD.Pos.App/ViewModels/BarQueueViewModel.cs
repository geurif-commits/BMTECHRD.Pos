using System;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.Models;
using System.Linq;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class BarQueueViewModel : ProductionQueueViewModelBase
{
    public BarQueueViewModel(ApiClient api, SignalRClient signalR, Guid businessId) : base(api, signalR, businessId)
    {
        _signalR.OnBarQueueUpdated += async () => await RefreshAsync();
    }

    protected override async Task LoadItemsAsync()
    {
        var list = await _api.GetBarQueueAsync(_businessId);
        Items.Clear();
        foreach (var i in list.OrderBy(x => x.CreatedAt)) Items.Add(i);
    }

    protected override Task<bool> UpdateStatusAsync(Guid orderItemId, string status)
    {
        return _api.UpdateBarItemStatusAsync(orderItemId, status);
    }
}
