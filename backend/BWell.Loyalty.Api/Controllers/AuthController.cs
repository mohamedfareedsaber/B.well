using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Auth;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [EnableRateLimiting(DependencyInjection.LoginRateLimit)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        if (result is null)
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid email or password."));

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }
}
