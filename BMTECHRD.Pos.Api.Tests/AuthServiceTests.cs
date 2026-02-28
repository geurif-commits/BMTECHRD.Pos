using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Auth;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Businesses.Add(new Business { Id = businessId, Name = "B1" });
        db.Users.Add(new User
        {
            Id = userId,
            BusinessId = businessId,
            Username = "admin",
            PasswordHash = "hash:good",
            Role = UserRole.ADMIN
        });
        await db.SaveChangesAsync();

        var sut = new AuthService(db, new FakePasswordHasher(), new FakeTokenService(userId, businessId));

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.LoginAsync(new LoginRequest
            {
                BusinessId = businessId,
                Username = "admin",
                Password = "bad"
            }, null, CancellationToken.None));

        Assert.Equal("AUTH_INVALID_CREDENTIALS", ex.ErrorCode);
    }

    [Fact]
    public async Task RefreshAsync_WithEmptyToken_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var sut = new AuthService(db, new FakePasswordHasher(), new FakeTokenService(Guid.NewGuid(), Guid.NewGuid()));

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = string.Empty }, null, CancellationToken.None));

        Assert.Equal("AUTH_REFRESH_TOKEN_REQUIRED", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string plain) => $"hash:{plain}";
        public bool Verify(string plain, string hash) => hash == $"hash:{plain}";
    }

    private sealed class FakeTokenService : ITokenService
    {
        private readonly Guid _userId;
        private readonly Guid _businessId;

        public FakeTokenService(Guid userId, Guid businessId)
        {
            _userId = userId;
            _businessId = businessId;
        }

        public string CreateAccessToken(User user, Guid businessId) => "access-token";

        public Task<(string RawToken, RefreshToken Entity)> CreateRefreshTokenAsync(User user, Guid businessId, string? deviceId = null, string? ipAddress = null)
            => Task.FromResult(("refresh-token", new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, BusinessId = _businessId, TokenHash = "h", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(1) }));

        public Task<RefreshToken?> ValidateRefreshTokenAsync(string rawToken)
            => Task.FromResult<RefreshToken?>(null);

        public Task<RefreshTokenValidationResult> ValidateRefreshTokenDetailedAsync(string rawToken, string? deviceIdFromRequest)
            => Task.FromResult(new RefreshTokenValidationResult(false, false, "NOT_FOUND", null));

        public Task RevokeRefreshTokenAsync(Guid tokenId, Guid? replacedByTokenId = null) => Task.CompletedTask;

        public Task<int> RevokeAllRefreshTokensAsync(Guid userId, Guid businessId, string reason) => Task.FromResult(0);

        public string HashToken(string rawToken) => rawToken;
    }
}
