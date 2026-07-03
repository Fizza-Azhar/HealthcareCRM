using Microsoft.AspNetCore.Mvc;
using HealthcareCRM.API.Models;

namespace HealthcareCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    return Ok(new
    {
        message = "Login successful",
        token = "dummy-jwt-token-123456"
    });
}

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        return Ok(new
        {
            message = "Registration successful"
        });
    }
}