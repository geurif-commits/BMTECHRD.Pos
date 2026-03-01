using BMTECHRD.Pos.Api.Controllers;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Auth;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_ShouldAllowUsingPinCredential_WhenPinMatches()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);

        var hasher = new PasswordHasher();
        var tokenService = new FakeTokenService();

        var businessId = Guid.NewGuid();

        ctx.Businesses.Add(new Business
        {
            Id = businessId,
            Name = "Demo",
            CurrencyCode = "DOP",
            CreatedAt = DateTime.UtcNow
        });

        ctx.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Username = "maria",
            PasswordHash = hasher.Hash("Abcd1234"),
            PinHash = hasher.Hash("123456"),
            Role = UserRole.CASHIER,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var controller = new AuthController(ctx, hasher, tokenService);

        var result = await controller.Login(new LoginRequest
        {
            BusinessId = businessId,
            Username = "maria",
            Password = "123456"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("maria", payload.Username);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string CreateAccessToken(User user, Guid businessId) => "access-token";

        public Task<(string RawToken, RefreshToken Entity)> CreateRefreshTokenAsync(User user, Guid businessId, string? deviceId = null, string? ipAddress = null)
        {
            return Task.FromResult(("refresh-raw", new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BusinessId = businessId,
                TokenHash = "hash",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceId = deviceId,
                IpAddress = ipAddress
            }));
        }

        public Task<RefreshToken?> ValidateRefreshTokenAsync(string rawToken) => Task.FromResult<RefreshToken?>(null);

        public Task<RefreshTokenValidationResult> ValidateRefreshTokenDetailedAsync(string rawToken, string? deviceIdFromRequest)
            => Task.FromResult(new RefreshTokenValidationResult(false, false, null, null));

        public Task RevokeRefreshTokenAsync(Guid tokenId, Guid? replacedByTokenId = null) => Task.CompletedTask;

        public Task<int> RevokeAllRefreshTokensAsync(Guid userId, Guid businessId, string reason) => Task.FromResult(0);

        public string HashToken(string rawToken) => "hash";
    }
}
