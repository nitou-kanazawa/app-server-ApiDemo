using GameServer.Application.DTOs;
using GameServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, errorMessage) = await _authService.RegisterAsync(request);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, errorMessage) = await _authService.LoginAsync(request);

        if (!success)
        {
            return Unauthorized(new { message = errorMessage });
        }

        return Ok(new { token });
    }
}