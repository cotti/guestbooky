using Guestbooky.Application.Interfaces;
using Guestbooky.Domain.Abstractions.Infrastructure;
using MediatR;

namespace Guestbooky.Application.UseCases.AuthenticateUser;

#region Types
public record AuthenticateUserCommand(string Username, string Password) : IRequest<AuthenticateUserResult>;
public record AuthenticateUserResult(bool IsAuthenticated, string Token, string RefreshToken);
#endregion

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserCredentialsProvider _userCredentialsProvider;
    private readonly IJwtTokenService _tokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthenticateUserCommandHandler(IPasswordHasher passwordHasher, IJwtTokenService tokenGenerator, IUserCredentialsProvider userCredentialsProvider, IRefreshTokenService refreshTokenService)
    {
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _userCredentialsProvider = userCredentialsProvider;
        _refreshTokenService = refreshTokenService;
    }

    public Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = _userCredentialsProvider.GetCredentials();
        if (request.Username != _userCredentialsProvider.GetCredentials().Username || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Task.FromResult(new AuthenticateUserResult(false, string.Empty, string.Empty));
        }

        var token = _tokenGenerator.GenerateToken(request.Username);
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        _refreshTokenService.SaveRefreshToken(request.Username, refreshToken);

        return Task.FromResult(new AuthenticateUserResult(true, token, refreshToken));
    }
}
