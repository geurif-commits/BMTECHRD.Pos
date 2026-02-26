using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Application.Abstractions.Security;

public interface ITableAccessPolicy
{
    bool RequiresPin(UserRole actorRole, Guid? openedByWaiterId, Guid actorUserId);
    bool CanOverride(UserRole actorRole);
}