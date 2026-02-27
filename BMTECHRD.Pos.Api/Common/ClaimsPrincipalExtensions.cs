using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BMTECHRD.Pos.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredBusinessId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("bid")?.Value;
        if (!Guid.TryParse(value, out var businessId))
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Unauthorized", "Missing or invalid businessId claim", "AUTH_BID_MISSING");

        return businessId;
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("sub")?.Value;
        if (!Guid.TryParse(value, out var userId))
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Unauthorized", "Missing or invalid user ID claim", "AUTH_SUB_MISSING");

        return userId;
    }
}
