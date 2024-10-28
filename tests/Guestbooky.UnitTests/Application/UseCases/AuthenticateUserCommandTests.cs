using Guestbooky.Application.Interfaces;
using Guestbooky.Application.UseCases.AuthenticateUser;
using Guestbooky.Domain.Abstractions.Infrastructure;
using Guestbooky.Domain.Entities.User;
using Moq;

namespace Guestbooky.UnitTests.Application.UseCases;

public class AuthenticateUserCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUserCredentialsProvider> _userCredentialsProviderMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly AuthenticateUserCommandHandler _handler;

    public AuthenticateUserCommandHandlerTests()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _userCredentialsProviderMock = new Mock<IUserCredentialsProvider>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        _handler = new AuthenticateUserCommandHandler(
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _userCredentialsProviderMock.Object,
            _refreshTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessfulAuthentication()
    {
        // Arrange
        var command = new AuthenticateUserCommand("testuser", "testpass");

        var userCredentials = new ApplicationUser("testuser","hashpass");
        var expectedToken = "token";
        var expectedRefreshToken = "refresh";

        _userCredentialsProviderMock.Setup(x => x.GetCredentials()).Returns(userCredentials);
        _passwordHasherMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns(expectedToken);
        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(expectedRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(expectedRefreshToken, result.RefreshToken);

        _refreshTokenServiceMock.Verify(x => x.SaveRefreshToken(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidUsername_ReturnsFailedAuthentication()
    {
        // Arrange
        var command = new AuthenticateUserCommand("wronguser", "userpass");
        var userCredentials = new ApplicationUser("testuser", "hashpass");

        _userCredentialsProviderMock.Setup(x => x.GetCredentials()).Returns(userCredentials);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Equal(string.Empty, result.Token);
        Assert.Equal(string.Empty, result.RefreshToken);

        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.SaveRefreshToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsFailedAuthentication()
    {
        // Arrange
        var command = new AuthenticateUserCommand("testuser", "wrongpass");
        var userCredentials = new ApplicationUser("testuser", "hashpass");

        _userCredentialsProviderMock.Setup(x => x.GetCredentials()).Returns(userCredentials);
        _passwordHasherMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Equal(string.Empty, result.Token);
        Assert.Equal(string.Empty, result.RefreshToken);

        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.SaveRefreshToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}