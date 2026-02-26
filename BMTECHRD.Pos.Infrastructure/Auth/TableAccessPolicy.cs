using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Infrastructure.Auth;

public sealed class TableAccessPolicy : ITableAccessPolicy
{
    public bool RequiresPin(UserRole actorRole, Guid? openedByWaiterId, Guid actorUserId)
    {
        if (openedByWaiterId == null) return false;
        if (openedByWaiterId == actorUserId) return false;
        if (actorRole == UserRole.WAITER) return true;
        if (actorRole == UserRole.ADMIN || actorRole == UserRole.SUPERVISOR) return true;
        // default: kitchen/bar do not require pin
        return false;
    }

    public bool CanOverride(UserRole actorRole)
    {
        return actorRole == UserRole.ADMIN || actorRole == UserRole.SUPERVISOR;
    }
}
