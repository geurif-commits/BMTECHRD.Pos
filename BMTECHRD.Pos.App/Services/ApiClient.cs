using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.App.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public Uri? BaseAddress => _http.BaseAddress;

    public async Task<List<TableModel>> GetTablesAsync(Guid businessId)
    {
        var url = $"api/tables?businessId={businessId}";
        var res = await _http.GetFromJsonAsync<List<TableModel>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return res ?? new List<TableModel>();
    }

    public async Task<HttpResponseMessage> OpenTableAsync(Guid tableId, Guid waiterId)
    {
        var req = new OpenTableRequest { WaiterId = waiterId };
        return await _http.PostAsJsonAsync($"api/tables/{tableId}/open", req);
    }

    public async Task<TableAccessResponse?> AccessTableAsync(Guid tableId, Guid actorUserId, string pin)
    {
        var req = new TableAccessRequest { ActorUserId = actorUserId, Pin = pin };
        var resp = await _http.PostAsJsonAsync($"api/tables/{tableId}/access", req);
        if (!resp.IsSuccessStatusCode)
            return null;
        return await resp.Content.ReadFromJsonAsync<TableAccessResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<HttpResponseMessage> UpdateTablePositionAsync(Guid tableId, double posX, double posY)
    {
        var req = new UpdateTablePositionRequest { PosX = posX, PosY = posY };
        var resp = await _http.PatchAsync($"api/tables/{tableId}/position", JsonContent.Create(req));
        return resp;
    }

    // Public businesses
    public async Task<List<BusinessPublicModel>> GetBusinessesPublicAsync()
    {
        var res = await _http.GetFromJsonAsync<List<BusinessPublicModel>>("api/business/public", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return res ?? new List<BusinessPublicModel>();
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }


    public async Task<List<MenuCategoryModel>> GetCategoriesAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<MenuCategoryModel>>($"api/categories?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return list ?? new List<MenuCategoryModel>();
    }

    public async Task<List<MenuProductModel>> GetProductsAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<MenuProductModel>>($"api/products?businessId={businessId}&active=true", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return list ?? new List<MenuProductModel>();
    }

    public async Task<CreateOrderBatchResponse?> CreateOrderBatchAsync(CreateOrderBatchRequest req)
    {
        var response = await _http.PostAsJsonAsync("api/orders/batch", req);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CreateOrderBatchResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<List<ProductionQueueItemModel>> GetKitchenQueueAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<ProductionQueueItemDto>>($"api/kitchen/queue?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<ProductionQueueItemModel>();
        return list.Select(dto => new ProductionQueueItemModel
        {
            OrderItemId = dto.OrderItemId,
            OrderId = dto.OrderId,
            TableId = dto.TableId,
            TableNumber = dto.TableNumber,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            Status = dto.Status,
            Area = dto.Area,
            CreatedAt = dto.CreatedAt
        }).ToList();
    }

    public async Task<List<ProductionQueueItemModel>> GetBarQueueAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<ProductionQueueItemDto>>($"api/bar/queue?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<ProductionQueueItemModel>();
        return list.Select(dto => new ProductionQueueItemModel
        {
            OrderItemId = dto.OrderItemId,
            OrderId = dto.OrderId,
            TableId = dto.TableId,
            TableNumber = dto.TableNumber,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            Status = dto.Status,
            Area = dto.Area,
            CreatedAt = dto.CreatedAt
        }).ToList();
    }

    public async Task<bool> UpdateKitchenItemStatusAsync(Guid orderItemId, string status)
    {
        var resp = await _http.PatchAsJsonAsync($"api/kitchen/items/{orderItemId}/status", new { status });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBarItemStatusAsync(Guid orderItemId, string status)
    {
        var resp = await _http.PatchAsJsonAsync($"api/bar/items/{orderItemId}/status", new { status });
        return resp.IsSuccessStatusCode;
    }

    // Cashier endpoints
    public async Task<List<TableSummaryDto>> GetCashTablesAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<TableSummaryDto>>($"api/cash/tables?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return list ?? new List<TableSummaryDto>();
    }

    // Inventory
    public async Task<List<StockItemModel>> GetStockAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<StockItemDto>>($"api/inventory/stock?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<StockItemModel>();
        return list.Select(d => new StockItemModel { ProductId = d.ProductId, Name = d.Name, Stock = d.Stock, TrackInventory = d.TrackInventory }).ToList();
    }

    public async Task<List<InventoryMovementModel>> GetMovementsAsync(Guid businessId, Guid? productId, DateTime? from, DateTime? to, int? limit)
    {
        var url = $"api/inventory/movements?businessId={businessId}";
        if (productId.HasValue) url += $"&productId={productId.Value}";
        if (from.HasValue) url += $"&from={System.Net.WebUtility.UrlEncode(from.Value.ToString("o"))}";
        if (to.HasValue) url += $"&to={System.Net.WebUtility.UrlEncode(to.Value.ToString("o"))}";
        if (limit.HasValue) url += $"&limit={limit.Value}";
        var list = await _http.GetFromJsonAsync<List<InventoryMovementDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<InventoryMovementModel>();
        return list.Select(d => new InventoryMovementModel
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.ProductName,
            QuantityDelta = d.QuantityDelta,
            Reason = d.Reason,
            ActorUserId = d.ActorUserId,
            ActorUsername = d.ActorUsername,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task<bool> AdjustInventoryAsync(InventoryAdjustRequestModel req)
    {
        var dto = new InventoryAdjustRequest { BusinessId = req.BusinessId, ProductId = req.ProductId, QuantityDelta = req.QuantityDelta, Reason = req.Reason };
        var resp = await _http.PostAsJsonAsync($"api/inventory/adjust", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<GetBillResponse?> GetBillAsync(Guid businessId, Guid tableId)
    {
        var resp = await _http.GetAsync($"api/cash/bill?businessId={businessId}&tableId={tableId}");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<GetBillResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<CreatePaymentResponse?> CreatePaymentAsync(CreatePaymentRequest req)
    {
        var resp = await _http.PostAsJsonAsync($"api/cash/payments", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<CreatePaymentResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // Shifts
    public async Task<ShiftStatusResponse?> GetActiveShiftAsync(Guid businessId, Guid userId)
    {
        var resp = await _http.GetAsync($"api/shifts/active?businessId={businessId}&userId={userId}");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ShiftStatusResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<CreateShiftResponse?> OpenShiftAsync(CreateShiftRequest req)
    {
        var resp = await _http.PostAsJsonAsync($"api/shifts/open", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<CreateShiftResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<ShiftSummaryResponse?> CloseShiftAsync(CloseShiftRequest req)
    {
        var resp = await _http.PostAsJsonAsync($"api/shifts/close", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ShiftSummaryResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<List<ShiftListItemModel>> GetShiftsAsync(Guid businessId, Guid? userId, DateTime? from, DateTime? to, string? status, int? limit)
    {
        var url = $"api/shifts/list?businessId={businessId}";
        if (userId.HasValue) url += $"&userId={userId.Value}";
        if (from.HasValue) url += $"&from={System.Net.WebUtility.UrlEncode(from.Value.ToString("o"))}";
        if (to.HasValue) url += $"&to={System.Net.WebUtility.UrlEncode(to.Value.ToString("o"))}";
        if (!string.IsNullOrWhiteSpace(status)) url += $"&status={System.Net.WebUtility.UrlEncode(status)}";
        if (limit.HasValue) url += $"&limit={limit.Value}";

        var list = await _http.GetFromJsonAsync<List<ShiftListItemDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<ShiftListItemModel>();

        return list.Select(dto => new ShiftListItemModel
        {
            ShiftId = dto.ShiftId,
            UserId = dto.UserId,
            Username = dto.Username,
            OpenedAt = dto.OpenedAt,
            ClosedAt = dto.ClosedAt,
            OpeningCash = dto.OpeningCash,
            ClosingCash = dto.ClosingCash,
            Status = dto.Status
        }).ToList();
    }

    public async Task<ShiftSummaryModel?> GetShiftSummaryAsync(Guid businessId, Guid shiftId)
    {
        var resp = await _http.GetAsync($"api/shifts/{shiftId}/summary?businessId={businessId}");
        if (!resp.IsSuccessStatusCode) return null;

        var dto = await resp.Content.ReadFromJsonAsync<ShiftSummaryResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (dto == null) return null;

        return new ShiftSummaryModel
        {
            ShiftId = dto.ShiftId,
            SalesCash = dto.SalesCash,
            SalesCard = dto.SalesCard,
            SalesTransfer = dto.SalesTransfer,
            SalesMixed = dto.SalesMixed,
            TotalPayments = dto.TotalSales,
            ExpectedCash = dto.ExpectedCash,
            Difference = dto.Difference
        };
    }

    // Users (Admin)
    public async Task<List<UserListItemModel>> GetUsersAsync(Guid businessId)
    {
        var list = await _http.GetFromJsonAsync<List<UserListItemDto>>($"api/users?businessId={businessId}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<UserListItemModel>();
        return list.Select(dto => new UserListItemModel
        {
            Id = dto.Id,
            Username = dto.Username,
            Role = dto.Role,
            IsActive = dto.IsActive,
            HasPin = dto.HasPin,
            CreatedAt = dto.CreatedAt
        }).ToList();
    }

    public async Task<bool> CreateUserAsync(CreateUserRequestModel req)
    {
        var dto = new CreateUserRequest { BusinessId = req.BusinessId, ActorUserId = req.ActorUserId, Username = req.Username, Password = req.Password, Pin4 = req.Pin4, Role = req.Role, IsActive = req.IsActive };
        var resp = await _http.PostAsJsonAsync($"api/users", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequestModel req)
    {
        var dto = new UpdateUserRequest { BusinessId = req.BusinessId, Role = req.Role, IsActive = req.IsActive };
        var resp = await _http.PutAsJsonAsync($"api/users/{userId}", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, ResetPasswordRequestModel req)
    {
        var dto = new ResetPasswordRequest { BusinessId = req.BusinessId, ActorUserId = req.ActorUserId, NewPassword = req.NewPassword };
        var resp = await _http.PostAsJsonAsync($"api/users/{userId}/reset-password", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPinAsync(Guid userId, ResetPinRequestModel req)
    {
        var dto = new ResetPinRequest { BusinessId = req.BusinessId, ActorUserId = req.ActorUserId, NewPin4 = req.NewPin4 };
        var resp = await _http.PostAsJsonAsync($"api/users/{userId}/reset-pin", dto);
        return resp.IsSuccessStatusCode;
    }


    public async Task<BusinessSettingsModel?> GetBusinessSettingsAsync(Guid businessId)
    {
        var response = await _http.GetAsync($"api/business/{businessId}/settings");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<BusinessSettingsModel>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> UpdateBusinessSettingsAsync(BusinessSettingsModel model)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(model.Name), nameof(model.Name));
        form.Add(new StringContent(model.EnableItbis.ToString()), nameof(model.EnableItbis));
        form.Add(new StringContent(model.ItbisRate.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(model.ItbisRate));
        form.Add(new StringContent(model.EnableTip.ToString()), nameof(model.EnableTip));
        form.Add(new StringContent(model.TipRate.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(model.TipRate));
        form.Add(new StringContent(model.EnableFiscalReceipt.ToString()), nameof(model.EnableFiscalReceipt));
        form.Add(new StringContent(model.EnableElectronicInvoice.ToString()), nameof(model.EnableElectronicInvoice));

        var response = await _http.PutAsync($"api/business/{model.BusinessId}/settings", form);
        return response.IsSuccessStatusCode;
    }

    // Reports
    public async Task<DailySalesReportModel?> GetDailySalesAsync(Guid businessId, DateTime from, DateTime to)
    {
        var fromStr = System.Net.WebUtility.UrlEncode(from.ToString("o"));
        var toStr = System.Net.WebUtility.UrlEncode(to.ToString("o"));
        var url = $"api/reports/sales/daily?businessId={businessId}&from={fromStr}&to={toStr}";

        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;

        var dto = await resp.Content.ReadFromJsonAsync<DailySalesReportResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (dto == null) return null;

        var model = new DailySalesReportModel();
        foreach (var item in dto.Items)
        {
            model.Items.Add(new SalesDailyItemModel { Date = item.Date, Total = item.Total });
        }
        model.Payments = new PaymentsSummaryModel
        {
            Cash = dto.Payments.Cash,
            Card = dto.Payments.Card,
            Transfer = dto.Payments.Transfer,
            Mixed = dto.Payments.Mixed,
            Total = dto.Payments.Total
        };
        return model;
    }

    public async Task<List<SalesByProductModel>> GetSalesByProductAsync(Guid businessId, DateTime from, DateTime to, int limit)
    {
        var fromStr = System.Net.WebUtility.UrlEncode(from.ToString("o"));
        var toStr = System.Net.WebUtility.UrlEncode(to.ToString("o"));
        var url = $"api/reports/sales/by-product?businessId={businessId}&from={fromStr}&to={toStr}&limit={limit}";

        var list = await _http.GetFromJsonAsync<List<SalesByProductDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<SalesByProductModel>();

        return list.Select(dto => new SalesByProductModel
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Qty = dto.Qty,
            Total = dto.Total
        }).ToList();
    }

    public async Task<List<SalesByUserModel>> GetSalesByUserAsync(Guid businessId, DateTime from, DateTime to)
    {
        var fromStr = System.Net.WebUtility.UrlEncode(from.ToString("o"));
        var toStr = System.Net.WebUtility.UrlEncode(to.ToString("o"));
        var url = $"api/reports/sales/by-user?businessId={businessId}&from={fromStr}&to={toStr}";

        var list = await _http.GetFromJsonAsync<List<SalesByUserDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (list == null) return new List<SalesByUserModel>();

        return list.Select(dto => new SalesByUserModel
        {
            UserId = dto.UserId,
            Username = dto.Username,
            Total = dto.Total
        }).ToList();
    }

    /// <summary>
    /// Renueva el access token usando refresh token.
    /// Parte de BLOQUE 7 (WPF Auth handler).
    /// Mantiene compatibilidad con llamadas existentes.
    /// </summary>
    public Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
        => RefreshTokenAsync(refreshToken, deviceId: null, CancellationToken.None);

    /// <summary>
    /// Versión ETAPA 9: permite deviceId y CancellationToken (para el AuthHeaderHandler).
    /// </summary>
    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string? deviceId, CancellationToken cancellationToken)
    {
        try
        {
            var req = new RefreshTokenRequest
            {
                RefreshToken = refreshToken,
                DeviceId = deviceId
            };

            var resp = await _http.PostAsJsonAsync("api/auth/refresh", req, cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Revoca el refresh token (logout).
    /// Parte de BLOQUE 7 (WPF Auth handler).
    /// Mantiene compatibilidad con llamadas existentes.
    /// </summary>
    public Task<bool> LogoutAsync(string refreshToken)
        => LogoutAsync(refreshToken, CancellationToken.None);

    /// <summary>
    /// Versión ETAPA 9 con CancellationToken.
    /// </summary>
    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var req = new LogoutRequest { RefreshToken = refreshToken };
            var resp = await _http.PostAsJsonAsync("api/auth/logout", req, cancellationToken);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}