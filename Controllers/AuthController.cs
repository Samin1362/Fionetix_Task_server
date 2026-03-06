using Microsoft.AspNetCore.Mvc;
using FionetixAPI.DTOs;

namespace FionetixAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var email = HttpContext.Items["UserEmail"]?.ToString();
        var role = HttpContext.Items["UserRole"]?.ToString();

        if (email == null || role == null)
            return Unauthorized(new { error = "Not authenticated." });

        return Ok(new UserMeDto { Email = email, Role = role });
    }
}
