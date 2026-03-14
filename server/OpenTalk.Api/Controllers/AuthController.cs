using Microsoft.AspNetCore.Mvc;
using OpenTalk.Application.Services;

namespace OpenTalk.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(UserService users) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = await users.RegisterAsync(req.Username, req.Password, req.Nickname);
        return Ok(new { user.Id, user.Username, user.Nickname, user.Avatar });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await users.LoginAsync(req.Username, req.Password);
        if (user is null) return Unauthorized();
        return Ok(new { token = "demo-token", user = new { user.Id, user.Username, user.Nickname } });
    }
}

public record RegisterRequest(string Username, string Password, string? Nickname);
public record LoginRequest(string Username, string Password);
