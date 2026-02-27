using BMTECHRD.Pos.App.Core;

namespace BMTECHRD.Pos.App.Services;

public interface IDeviceRolePolicy
{
    bool IsRoleAllowedForDeviceMode(DeviceMode mode, string? role);
}
