namespace GameServer.Application.DTOs;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token);
