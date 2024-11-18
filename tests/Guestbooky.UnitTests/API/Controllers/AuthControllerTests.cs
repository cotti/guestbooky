using Guestbooky.API.Configurations;
using Guestbooky.API.Controllers;
using Guestbooky.API.DTOs.Auth;
using Guestbooky.Application.UseCases.AuthenticateUser;
using Guestbooky.Application.UseCases.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Guestbooky.UnitTests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly APISettings _settings;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _settings = new APISettings() { RunningEnvironment = Guestbooky.API.Enums.RunningEnvironment.Development };
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        var httpResponseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();
        var cookies = new Mock<IResponseCookies>();

        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
        httpResponseMock.SetupGet(r => r.Cookies).Returns(cookies.Object);
        httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);



        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            }

        };

        var requestDto = new LoginRequestDto("testuser", "password");
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<AuthenticateUserCommand>(), default))
            .ReturnsAsync(new AuthenticateUserResult(true, "testToken", "testRefreshToken"));

        // Act
        var result = await controller.Login(requestDto, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);
        cookies.Verify(c => c.Append("token", It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once);
        Assert.Equal("testToken", responseDto.Token);
        Assert.Equal("testRefreshToken", responseDto.RefreshToken);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings);

        var requestDto = new LoginRequestDto("testuser", "wrongpassword");

        _mediatorMock.Setup(m => m.Send(It.IsAny<AuthenticateUserCommand>(), default))
            .ReturnsAsync(new AuthenticateUserResult(false, string.Empty, string.Empty));

        // Act
        var result = await controller.Login(requestDto, default);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsProblemDetails_WhenExceptionIsThrown()
    {
        // Arrange
        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings);

        var requestDto = new LoginRequestDto("testuser", "password");

        _mediatorMock.Setup(m => m.Send(It.IsAny<AuthenticateUserCommand>(), default))
            .ThrowsAsync(new Exception());

        // Act
        var result = await controller.Login(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("An error occurred on the server:", problemDetails.Detail);

    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenTokenMatches()
    {
        // Arrange
        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings);

        var requestDto = new RefreshTokenRequestDto("refresh");

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ReturnsAsync(new RefreshTokenResult(true, "testToken", "testRefreshToken"));

        // Act
        var result = await controller.RefreshToken(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<RefreshTokenResponseDto>(okResult.Value);
        Assert.Equal("testToken", responseDto.Token);
        Assert.Equal("testRefreshToken", responseDto.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenTokenNotMatched()
    {
        // Arrange
        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings);

        var requestDto = new RefreshTokenRequestDto("refresh");

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ReturnsAsync(new RefreshTokenResult(false, string.Empty, string.Empty, "error"));

        // Act
        var result = await controller.RefreshToken(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("error", problemDetails.Detail);
    }

    [Fact]
    public async Task RefreshToken_ReturnsProblemDetails_WhenExceptionIsThrown()
    {
        // Arrange
        var controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _settings);

        var requestDto = new RefreshTokenRequestDto("refresh");

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ThrowsAsync(new Exception());

        // Act
        var result = await controller.RefreshToken(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("An error occurred on the server:", problemDetails.Detail);
    }
}