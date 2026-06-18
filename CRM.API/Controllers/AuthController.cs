using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new LoginCommand(request.EmailOrUsername, request.Password, request.RememberMe),
                cancellationToken);

            return Ok(new
            {
                result.AccessToken,
                result.ExpiresAt,
                User = new
                {
                    result.User.Id,
                    result.User.Email,
                    result.User.Username,
                    result.User.Roles
                }
            });
        }
        catch (UnauthorizedException)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }
}

public record LoginRequest(string EmailOrUsername, string Password, bool RememberMe);
