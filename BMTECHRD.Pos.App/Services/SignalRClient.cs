using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace BMTECHRD.Pos.App.Services;

/// <summary>
/// Cliente SignalR con soporte para autenticación JWT (BLOQUE 6 + 7).
/// </summary>
public sealed class SignalRClient
{
    private readonly HubConnection _connection;

    public event Action? OnKitchenQueueUpdated;
    public event Action? OnBarQueueUpdated;
    public event Action? OnTablesUpdated;
    public event Action? OnInventoryUpdated;

    public SignalRClient(string baseUrl, Func<Task<string?>>? accessTokenFactory = null)
    {
        var hubUrl = new Uri(new Uri(baseUrl), "hubs/pos");

        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Si tenemos factory de token, usarla para obtener el token
                if (accessTokenFactory != null)
                {
                    options.AccessTokenProvider = accessTokenFactory;
                }
            })
            .WithAutomaticReconnect()
            .Build();

        _connection = builder;

        _connection.On("kitchen.queue.updated", () => OnKitchenQueueUpdated?.Invoke());
        _connection.On("bar.queue.updated", () => OnBarQueueUpdated?.Invoke());
        _connection.On("tables.updated", () => OnTablesUpdated?.Invoke());
        _connection.On("inventory.updated", () => OnInventoryUpdated?.Invoke());
    }

    public async Task ConnectAsync()
    {
        if (_connection.State == HubConnectionState.Connected) return;
        await _connection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected) return;
        await _connection.StopAsync();
    }

    public async Task JoinBusinessAsync(Guid businessId)
    {
        await _connection.InvokeAsync("JoinBusiness", businessId.ToString());
    }

    public async Task LeaveBusinessAsync(Guid businessId)
    {
        await _connection.InvokeAsync("LeaveBusiness", businessId.ToString());
    }
}
