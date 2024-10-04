using Guestbooky.Application.Interfaces;
using MediatR;
using System.Runtime.InteropServices;

namespace Guestbooky.Application.UseCases.RefreshToken;

#region Types
public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
public record RefreshTokenResult(bool Success, string Token, string RefreshToken, string? ErrorMessage = null);
#endregion

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(IRefreshTokenService refreshTokenService, IJwtTokenService jwtTokenService)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenService = jwtTokenService;
    }

    public Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _refreshTokenService.ValidateRefreshToken(request.RefreshToken);
        if (principal == null)
            return Task.FromResult(new RefreshTokenResult(false, string.Empty, string.Empty, "Could not validate the cached refresh token."));

        var username = principal.Identity!.Name!;
        var newAccessToken = _jwtTokenService.GenerateToken(username);
        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

        _refreshTokenService.SaveRefreshToken(username, newRefreshToken);

        return Task.FromResult(new RefreshTokenResult(true, newAccessToken, newRefreshToken));
    }
}