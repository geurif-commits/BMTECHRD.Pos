using BMTECHRD.Pos.App.Core;

namespace BMTECHRD.Pos.App.Services;

public sealed class DeviceRolePolicy : IDeviceRolePolicy
{
    public bool IsRoleAllowedForDeviceMode(DeviceMode mode, string? role)
    {
        var userRole = role?.ToUpperInvariant() ?? string.Empty;

        return mode switch
        {
            DeviceMode.Server => userRole == "ADMIN" || userRole == "SUPERVISOR",
            DeviceMode.Cashier => userRole == "CASHIER" || userRole == "ADMIN" || userRole == "SUPERVISOR",
            DeviceMode.Kitchen => userRole == "KITCHEN" || userRole == "ADMIN" || userRole == "SUPERVISOR",
            DeviceMode.Bar => userRole == "BAR" || userRole == "ADMIN" || userRole == "SUPERVISOR",
            DeviceMode.Floor => userRole == "WAITER" || userRole == "CASHIER" || userRole == "ADMIN" || userRole == "SUPERVISOR",
            _ => false
        };
    }
}
