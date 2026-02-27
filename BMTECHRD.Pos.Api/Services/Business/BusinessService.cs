using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Business;

public sealed class BusinessService : IBusinessService
{
    private readonly AppDbContext _ctx;

    public BusinessService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<CreateBusinessResponse> CreateAsync(CreateBusinessRequest request, IFormFile? logo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid business", "Name is required", "BIZ_NAME_REQUIRED");

        var business = new Domain.Entities.Business
        {
            Name = request.Name,
            CurrencyCode = "DOP"
        };

        var activationKey = GenerateActivationKey();
        business.License = new License
        {
            ActivationKey = activationKey,
            Plan = LicensePlan.TRIAL_7_DAYS,
            Status = LicenseStatus.INACTIVE,
            Business = business
        };

        if (logo != null && logo.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(logo.FileName)}";
            var relative = $"/uploads/logos/{fileName}";
            var physical = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(physical)!);
            await using var stream = System.IO.File.Create(physical);
            await logo.CopyToAsync(stream, ct);
            business.LogoPath = relative;
        }

        _ctx.Businesses.Add(business);
        await _ctx.SaveChangesAsync(ct);

        return new CreateBusinessResponse
        {
            BusinessId = business.Id,
            Name = business.Name,
            LogoPath = business.LogoPath,
            ActivationKey = activationKey
        };
    }

    public Task<List<BusinessPublicDto>> GetPublicAsync(CancellationToken ct)
        => _ctx.Businesses.AsNoTracking()
            .Select(b => new BusinessPublicDto { BusinessId = b.Id, Name = b.Name, LogoPath = b.LogoPath })
            .ToListAsync(ct);

    private static string GenerateActivationKey()
    {
        static string Part()
        {
            var rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 5).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }

        return $"BMT-{Part()}-{Part()}";
    }
}
