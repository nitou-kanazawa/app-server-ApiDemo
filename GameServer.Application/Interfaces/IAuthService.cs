using GameServer.Application.DTOs;

namespace GameServer.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string? Token, string? ErrorMessage)> LoginAsync(LoginRequest request);
}
