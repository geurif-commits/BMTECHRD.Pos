using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Infrastructure.Auth;

public sealed class LicenseService : ILicenseService
{
    private readonly AppDbContext _ctx;

    public LicenseService(AppDbContext ctx) => _ctx = ctx;

    public async System.Threading.Tasks.Task<bool> IsBusinessActiveAsync(System.Guid businessId, System.Threading.CancellationToken ct)
    {
        var license = await _ctx.Licenses.FirstOrDefaultAsync(x => x.BusinessId == businessId, ct);
        if (license == null) return false;
        return license.IsActive(System.DateTime.UtcNow);
    }
}
