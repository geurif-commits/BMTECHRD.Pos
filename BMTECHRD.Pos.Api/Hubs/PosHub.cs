using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BMTECHRD.Pos.Api.Hubs;

/// <summary>
/// Hub de SignalR para eventos en tiempo real del POS.
/// Requiere autenticación JWT.
/// </summary>
[Authorize]
public class PosHub : Hub
{
    /// <summary>
    /// Agrupa la conexión a un negocio específico.
    /// Valida que el businessId del token coincida con el requestado.
    /// </summary>
    public async Task JoinBusiness(string businessIdString)
    {
        if (!Guid.TryParse(businessIdString, out var businessId))
        {
            throw new HubException("Invalid businessId format");
        }

        // Extraer businessId del claim JWT
        var claimBid = Context.User?.FindFirst("bid")?.Value;
        if (string.IsNullOrEmpty(claimBid) || !Guid.TryParse(claimBid, out var tokenBusinessId))
        {
            throw new HubException("Missing or invalid businessId claim in token");
        }

        // Validar que el usuario no intente unirse a otro negocio
        if (businessId != tokenBusinessId)
        {
            throw new HubException("Unauthorized: businessId mismatch");
        }

        // Agregar a grupo de negocio
        await Groups.AddToGroupAsync(Context.ConnectionId, businessIdString);
    }

    /// <summary>
    /// Remueve la conexión del grupo del negocio.
    /// </summary>
    public async Task LeaveBusiness(string businessIdString)
    {
        if (!Guid.TryParse(businessIdString, out _))
        {
            throw new HubException("Invalid businessId format");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, businessIdString);
    }
}
