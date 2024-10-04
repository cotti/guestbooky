using Guestbooky.API.DTOs.Auth;
using Guestbooky.Application.UseCases.AuthenticateUser;
using Guestbooky.Application.UseCases.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Guestbooky.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Login request endpoint reached. Sending command.");
            var command = new AuthenticateUserCommand(request.Username, request.Password);
            var result = await _mediator.Send(command, token);

            if(result.IsAuthenticated)
            {
                _logger.LogInformation("Authentication successful. Returning LoginResponse.");
                return Ok(new LoginResponseDto(result.Token, result.RefreshToken));
            }
            else
            {
                _logger.LogInformation("Authentication processed, but credentials did not match.");
                return Unauthorized();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred upon trying to login. Returning server error.");
            return Problem($"An error occurred on the server: {e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("refreshtoken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Refresh token endpoint reached. Sending command.");
            var command = new RefreshTokenCommand(request.RefreshToken);

            var result = await _mediator.Send(command, token);
            if (result.Success)
            {
                _logger.LogInformation("Refresh token request successful.");
                return Ok(new RefreshTokenResponseDto(result.Token, result.RefreshToken));
            }
            else
            {
                _logger.LogInformation($"Refresh token request failed. Reason: {result.ErrorMessage}");
                return Unauthorized(new ProblemDetails() { Detail = result.ErrorMessage, Title = "Refresh token failed" });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred upon trying to refresh the token. Returning server error.");
            return Problem($"An error occurred on the server: {e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
