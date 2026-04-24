using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TKC_Cloud.Services;

namespace TKC_Cloud.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // Create a Register-Token
    [Authorize(Roles = "Admin")]
    [HttpPost("create-register-token")]
    public async Task<IActionResult> CreateRegisterToken()
    {
        var token = await _authService.CreateRegisterTokenAsync();

        return Ok(token);
    }

    // Register per Email and Password
    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password, string registerToken)
    {
        var user = await _authService.RegisterAsync(email, password, registerToken);
        return Ok(user);
    }

    // Login per Email and Password
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginWithRefreshAsync(request.Email, request.Password);

        if (result == null)
            return Unauthorized();

        return Ok(new 
        {
            accessToken = result.Value.accessToken,
            refreshToken = result.Value.refreshToken  
        });
    }

    // Refresh the Access-Token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var newToken = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
        if (newToken == null)
            return Unauthorized();

        return Ok(new { accessToken = newToken});
    }
}

// Request`s Class
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = "";
}