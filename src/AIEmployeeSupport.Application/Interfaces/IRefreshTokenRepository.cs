using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RotateTokenAsync(RefreshToken oldToken, RefreshToken newToken, CancellationToken cancellationToken = default);
    Task RevokeAllActiveTokensForUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(RefreshToken token, string reason, CancellationToken cancellationToken = default);
}
