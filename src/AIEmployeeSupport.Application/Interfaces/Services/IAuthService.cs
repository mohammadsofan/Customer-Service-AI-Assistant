using AIEmployeeSupport.Application.DTOs.Auth;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
