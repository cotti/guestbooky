using Guestbooky.Application.Interfaces;
using Guestbooky.Application.UseCases.RefreshToken;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.Application.UseCases;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new RefreshTokenCommandHandler(_refreshTokenServiceMock.Object, _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsToken()
    {
        // Arrange
        const string oldRefreshToken = "old-refresh-token";
        const string username = "testuser";
        const string newAccessToken = "new-access-token";
        const string newRefreshToken = "new-refresh-token";

        var claimsPrincipal = new ClaimsPrincipal(new GenericIdentity(username));

        _refreshTokenServiceMock.Setup(x => x.ValidateRefreshToken(It.IsAny<string>())).Returns(claimsPrincipal);
        _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns(newAccessToken);
        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        var command = new RefreshTokenCommand(oldRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(newAccessToken, result.Token);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        Assert.Null(result.ErrorMessage);

        _refreshTokenServiceMock.Verify(x => x.SaveRefreshToken(username, newRefreshToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ReturnsFailure()
    {
        // Arrange
        const string invalidRefreshToken = "invalid-refresh-token";

        _refreshTokenServiceMock.Setup(x => x.ValidateRefreshToken(It.IsAny<string>()))
            .Returns((ClaimsPrincipal)null!);

        var command = new RefreshTokenCommand(invalidRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Token);
        Assert.Equal(string.Empty, result.RefreshToken);
        Assert.Equal("Could not validate the cached refresh token.", result.ErrorMessage);

        _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<string>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.SaveRefreshToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
