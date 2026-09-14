using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.DTOs.Auth;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AIEmployeeSupport.IntegrationTests;

public class RefreshTokenTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RefreshTokenTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<(User User, string Password)> CreateTestUserAsync(string role = "Employee")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var password = "SecurePassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"user_{Guid.NewGuid():N}@telecomtest.com",
            FullName = "Refresh Test User",
            Role = role == "Administrator" ? UserRole.Administrator : UserRole.Employee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (user, password);
    }

    [Fact]
    public async Task Login_ReturnsOpaqueRefreshToken_AndStoresHashInDatabase()
    {
        // Arrange
        var (user, password) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        // Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = password
        });

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        authResult.Should().NotBeNull();
        authResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authResult.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // Token must NOT be a 3-part dot-separated JWT
        authResult.RefreshToken.Split('.').Length.Should().NotBe(3);
        // Token must be high-entropy URL-safe Base64Url (32 bytes = 43 chars)
        authResult.RefreshToken.Length.Should().Be(43);

        // Verify the database contains ONLY the deterministic SHA-256 hash, never the raw token
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var expectedHash = HashToken(authResult.RefreshToken);

        var tokenRecord = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == expectedHash);
        tokenRecord.Should().NotBeNull();
        tokenRecord!.UserId.Should().Be(user.Id);
        tokenRecord.RevokedAt.Should().BeNull();
        tokenRecord.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_ValidToken_RotatesTokenAndRevokesPrevious()
    {
        // Arrange
        var (user, password) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = password
        });
        var initialAuth = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
        var initialRefreshToken = initialAuth!.RefreshToken;

        // Act: Refresh the token
        var refreshRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = initialRefreshToken
        });

        // Assert
        refreshRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuth = await refreshRes.Content.ReadFromJsonAsync<LoginResponse>();
        newAuth.Should().NotBeNull();
        newAuth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        newAuth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        newAuth.RefreshToken.Should().NotBe(initialRefreshToken);

        // Verify database: Old token is revoked with "Rotated" reason and points to new token
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var oldHash = HashToken(initialRefreshToken);
        var newHash = HashToken(newAuth.RefreshToken);

        var oldRecord = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == oldHash);
        var newRecord = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == newHash);

        oldRecord.Should().NotBeNull();
        oldRecord!.RevokedAt.Should().NotBeNull();
        oldRecord.RevocationReason.Should().Be("Rotated");
        oldRecord.ReplacedByTokenId.Should().Be(newRecord!.Id);

        newRecord.Should().NotBeNull();
        newRecord!.RevokedAt.Should().BeNull();
        newRecord.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_RevokedToken_TriggersReuseDetection_AndRevokesTokenFamily()
    {
        // Arrange
        var (user, password) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        // 1. Initial Login
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = password
        });
        var initialAuth = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
        var initialRefreshToken = initialAuth!.RefreshToken;

        // 2. Legitimate Refresh -> rotates initial token
        var refresh1Res = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = initialRefreshToken
        });
        var secondAuth = await refresh1Res.Content.ReadFromJsonAsync<LoginResponse>();
        var legitimateActiveToken = secondAuth!.RefreshToken;

        // 3. Attacker replays the old initialRefreshToken!
        var replayRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = initialRefreshToken
        });

        // Assert: Replay attempt is rejected with 401
        replayRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Assert: Reuse detection revoked the active token family in the database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var activeTokenHash = HashToken(legitimateActiveToken);

        var revokedFamilyToken = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == activeTokenHash);
        revokedFamilyToken.Should().NotBeNull();
        revokedFamilyToken!.RevokedAt.Should().NotBeNull();
        revokedFamilyToken.RevocationReason.Should().Be("Token reuse detected");

        // 4. Any subsequent attempt with legitimateActiveToken is now ALSO rejected
        var subsequentRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = legitimateActiveToken
        });
        subsequentRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var (user, _) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        var rawToken = "expired_raw_refresh_token_string_entropy_32";
        var hash = HashToken(rawToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hash,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
                RevokedAt = null
            });
            await db.SaveChangesAsync();
        }

        // Act
        var res = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = rawToken
        });

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ExplicitlyRevokesRefreshToken()
    {
        // Arrange
        var (user, password) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = password
        });
        var authResult = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();

        // Act: Logout
        var logoutRes = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest
        {
            RefreshToken = authResult!.RefreshToken
        });

        // Assert
        logoutRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB record is revoked
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hash = HashToken(authResult.RefreshToken);
        var record = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);

        record.Should().NotBeNull();
        record!.RevokedAt.Should().NotBeNull();
        record.RevocationReason.Should().Be("User logout");

        // Attempting to refresh should now be rejected
        var refreshRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = authResult.RefreshToken
        });
        refreshRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ConcurrentRequests_SafeHandling()
    {
        // Arrange
        var (user, password) = await CreateTestUserAsync();
        var client = _factory.CreateClient();

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = password
        });
        var authResult = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
        var refreshToken = authResult!.RefreshToken;

        // Act: Execute two simultaneous refresh requests with the same token
        var task1 = client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest { RefreshToken = refreshToken });
        var task2 = client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest { RefreshToken = refreshToken });

        var responses = await Task.WhenAll(task1, task2);

        // Assert: They cannot both succeed legitimately with multiple active unlinked tokens
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var unauthorizedCount = responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized);

        // At most one request should succeed (or if concurrency triggers reuse detection, both fail)
        successCount.Should().BeLessThanOrEqualTo(1);
        (successCount + unauthorizedCount).Should().Be(2);
    }
}
