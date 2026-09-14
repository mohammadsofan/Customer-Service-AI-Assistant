using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.Common.Settings;
using AIEmployeeSupport.Application.DTOs.Auth;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AIEmployeeSupport.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings,
        IPasswordHasher<User> passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Security Audit: Failed login attempt for non-existent or inactive email {Email}", request.Email);
            throw new UnauthorizedAccessException("بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Security Audit: Failed login attempt (invalid password) for email {Email}", request.Email);
            throw new UnauthorizedAccessException("بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.");
        }

        _logger.LogInformation("Security Audit: User {Email} ({UserId}) logged in successfully with role {Role}", user.Email, user.Id, user.Role);

        var accessToken = GenerateJwtToken(user, _jwtSettings.ExpirationMinutes);
        var rawRefreshToken = GenerateOpaqueRefreshToken();
        var tokenHash = HashToken(rawRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            }
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Security Audit: Empty refresh token received");
            throw new UnauthorizedAccessException("Invalid token.");
        }

        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken == null)
        {
            _logger.LogWarning("Security Audit: Refresh token not found");
            throw new UnauthorizedAccessException("Invalid token.");
        }

        if (existingToken.RevokedAt != null)
        {
            _logger.LogWarning("Security Audit: Token reuse detected for User {UserId}! Revoking active tokens.", existingToken.UserId);
            await _refreshTokenRepository.RevokeAllActiveTokensForUserAsync(existingToken.UserId, "Token reuse detected", cancellationToken);
            throw new UnauthorizedAccessException("Invalid token.");
        }

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Security Audit: Expired refresh token presented for User {UserId}", existingToken.UserId);
            throw new UnauthorizedAccessException("Token expired.");
        }

        var user = existingToken.User ?? await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Security Audit: User associated with refresh token is not found or inactive ({UserId})", existingToken.UserId);
            throw new UnauthorizedAccessException("Invalid token.");
        }

        var newAccessToken = GenerateJwtToken(user, _jwtSettings.ExpirationMinutes);
        var newRawRefreshToken = GenerateOpaqueRefreshToken();
        var newTokenHash = HashToken(newRawRefreshToken);

        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenId = newToken.Id;
        existingToken.RevocationReason = "Rotated";

        var rotated = await _refreshTokenRepository.RotateTokenAsync(existingToken, newToken, cancellationToken);
        if (!rotated)
        {
            _logger.LogWarning("Security Audit: Concurrency conflict detected during token rotation for User {UserId}", existingToken.UserId);
            throw new UnauthorizedAccessException("Invalid token.");
        }

        _logger.LogInformation("Security Audit: Token refreshed and rotated successfully for user {Email} ({UserId})", user.Email, user.Id);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            }
        };
    }

    public async Task LogoutAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return;
        }

        var tokenHash = HashToken(rawRefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (token != null && token.RevokedAt == null)
        {
            await _refreshTokenRepository.RevokeTokenAsync(token, "User logout", cancellationToken);
            _logger.LogInformation("Security Audit: Refresh token revoked on logout for user {UserId}", token.UserId);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };
    }

    private string GenerateJwtToken(User user, int expirationMinutes)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateOpaqueRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
