using Guestbooky.API.Controllers;
using Guestbooky.API.DTOs.Messages;
using Guestbooky.Application.UseCases.CountGuestbookMessages;
using Guestbooky.Application.UseCases.DeleteGuestbookMessage;
using Guestbooky.Application.UseCases.InsertGuestMessage;
using Guestbooky.Application.UseCases.ListGuestbookMessages;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Guestbooky.UnitTests.API.Controllers;

public class MessageControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<MessageController>> _loggerMock;

    public MessageControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<MessageController>>();
    }

    #region Insert()
    [Fact]
    public async Task Insert_ReturnsCreated_WhenValidMessageSent()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new InsertMessageRequestDto() { Author = "tester", Message = "message", CaptchaResponse = "validCaptcha" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<InsertGuestbookMessageCommand>(), default))
            .ReturnsAsync(new InsertGuestbookMessageResult(true, string.Empty));

        // Act
        var result = await controller.Insert(requestDto, default);

        // Assert
        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task Insert_ReturnsForbidden_WhenInvalidInputProvided()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new InsertMessageRequestDto() { Author = "tester", Message = "message", CaptchaResponse = "invalidCaptcha" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<InsertGuestbookMessageCommand>(), default))
            .ReturnsAsync(new InsertGuestbookMessageResult(false, "An invalid captcha has been provided"));

        // Act
        var result = await controller.Insert(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("An invalid captcha has been provided", problemDetails.Detail);
    }

    [Fact]
    public async Task Insert_ReturnsProblemDetails_WhenExceptionIsThrown()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new InsertMessageRequestDto() { Author = "tester", Message = "message", CaptchaResponse = "invalidCaptcha" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<InsertGuestbookMessageCommand>(), default))
            .ThrowsAsync(new Exception());

        // Act
        var result = await controller.Insert(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("An error occurred on the server:", problemDetails.Detail);
    }

    #endregion

    #region Get
    [Fact]
    public async Task Get_ReturnsMessages_WhenRangeValueSent()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        var httpResponseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
        httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);

        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            }
        };
        
        var requestedRange = new System.Net.Http.Headers.RangeItemHeaderValue(1, 3);
        List<GuestbookMessageQueryResult> availableMessages = [
                    new GuestbookMessageQueryResult("test-id1", "test-author", "test-message1", DateTimeOffset.UnixEpoch.AddDays(1)),
                    new GuestbookMessageQueryResult("test-id2", "test-author", "test-message2", DateTimeOffset.UnixEpoch.AddDays(2)),
                    new GuestbookMessageQueryResult("test-id3", "test-author", "test-message3", DateTimeOffset.UnixEpoch.AddDays(3)),
                    new GuestbookMessageQueryResult("test-id4", "test-author", "test-message4", DateTimeOffset.UnixEpoch.AddDays(4)),
                    new GuestbookMessageQueryResult("test-id5", "test-author", "test-message5", DateTimeOffset.UnixEpoch.AddDays(5)),
                ];
        var intendedMessages = availableMessages.Skip((int)requestedRange.From! - 1).Take((int)requestedRange.From! - 1 + (int)requestedRange.To!);

        var requestDto = new GetMessagesRequestDto() { Range = new System.Net.Http.Headers.RangeHeaderValue() { Unit = "messages" } };
        requestDto.Range.Ranges.Add(requestedRange);

        _mediatorMock.Setup(m => m.Send(It.IsAny<ListGuestbookMessagesQuery>(), default))
            .ReturnsAsync(new ListGuestbookMessagesQueryResult(intendedMessages));
        _mediatorMock.Setup(m => m.Send(It.IsAny<CountGuestbookMessagesQuery>(), default))
            .ReturnsAsync(new CountGuestbookMessagesQueryResult(availableMessages.Count));

        // Act
        var result = await controller.Get(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<GetMessagesResponseDto>>(objectResult.Value);

        httpResponseMock.VerifySet(r => r.StatusCode = StatusCodes.Status206PartialContent);
        Assert.Equal("messages", headers["Accept-Ranges"]);
        Assert.Contains("messages", headers["Content-Range"].ToString());
    }

    [Fact]
    public async Task Get_ReturnsMessages_WithNoRange()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        var httpResponseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupGet(r => r.Headers).Returns(headers);
        httpContextMock.SetupGet(h => h.Response).Returns(httpResponseMock.Object);

        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            }
        };

        List<GuestbookMessageQueryResult> availableMessages = [
                    new GuestbookMessageQueryResult("test-id1", "test-author", "test-message1", DateTimeOffset.UnixEpoch.AddDays(1)),
                    new GuestbookMessageQueryResult("test-id2", "test-author", "test-message2", DateTimeOffset.UnixEpoch.AddDays(2)),
                    new GuestbookMessageQueryResult("test-id3", "test-author", "test-message3", DateTimeOffset.UnixEpoch.AddDays(3)),
                    new GuestbookMessageQueryResult("test-id4", "test-author", "test-message4", DateTimeOffset.UnixEpoch.AddDays(4)),
                    new GuestbookMessageQueryResult("test-id5", "test-author", "test-message5", DateTimeOffset.UnixEpoch.AddDays(5)),
                ];
        var intendedMessages = availableMessages.Skip(0).Take(50);

        var requestDto = new GetMessagesRequestDto() { Range = null! };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ListGuestbookMessagesQuery>(), default))
            .ReturnsAsync(new ListGuestbookMessagesQueryResult(intendedMessages));
        _mediatorMock.Setup(m => m.Send(It.IsAny<CountGuestbookMessagesQuery>(), default))
            .ReturnsAsync(new CountGuestbookMessagesQueryResult(availableMessages.Count));

        // Act
        var result = await controller.Get(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<GetMessagesResponseDto>>(objectResult.Value);

        httpResponseMock.VerifySet(r => r.StatusCode = StatusCodes.Status206PartialContent);
        Assert.Equal("messages", headers["Accept-Ranges"]);
        Assert.Contains("messages", headers["Content-Range"].ToString());
    }

    [Fact]
    public async Task Get_ReturnsProblemDetails_WhenExceptionIsThrown()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new GetMessagesRequestDto() { Range = null! };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ListGuestbookMessagesQuery>(), default))
            .ThrowsAsync(new Exception());

        // Act
        var result = await controller.Get(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("An error occurred on the server:", problemDetails.Detail);
    }

    #endregion

    #region Delete
    [Fact]
    public async Task Delete_ReturnsOk_WithValidMessage()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new DeleteMessageRequestDto() { Id = "test_id" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGuestbookMessageCommand>(), default))
            .ReturnsAsync(new DeleteGuestbookMessageResult(true));

        // Act
        var result = await controller.Delete(requestDto, default);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenInvalidInputProvided()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new DeleteMessageRequestDto() { Id = "invalid_test_id" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGuestbookMessageCommand>(), default))
            .ReturnsAsync(new DeleteGuestbookMessageResult(false));

        // Act
        var result = await controller.Delete(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("Could not delete the guestbook entry", problemDetails.Detail);
    }

    [Fact]
    public async Task Delete_ReturnsProblemDetails_WhenExceptionIsThrown()
    {
        // Arrange
        var controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);

        var requestDto = new DeleteMessageRequestDto() { Id = "invalid_test_id" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGuestbookMessageCommand>(), default))
            .ThrowsAsync(new Exception());

        // Act
        var result = await controller.Delete(requestDto, default);

        // Assert
        Assert.NotNull(result);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.StartsWith("An error occurred on the server:", problemDetails.Detail);
    }
    #endregion

}
