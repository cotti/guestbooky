using Guestbooky.Application.UseCases.DeleteGuestbookMessage;
using Guestbooky.Domain.Abstractions.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.Application.UseCases;

public class DeleteGuestbookMessageCommandHandlerTests
{
    private readonly Mock<IGuestbookMessageRepository> _repositoryMock;
    private readonly DeleteGuestbookMessageCommandHandler _handler;

    public DeleteGuestbookMessageCommandHandlerTests()
    {
        _repositoryMock = new Mock<IGuestbookMessageRepository>();
        _handler = new DeleteGuestbookMessageCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_HasMessage_ReturnsSuccessTrue()
    {
        // Arrange
        var messageId = "existing-message-id";
        _repositoryMock.Setup(x => x.DeleteAsync(messageId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new DeleteGuestbookMessageCommand(messageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        _repositoryMock.Verify(x => x.DeleteAsync(messageId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoMessage_ReturnsFalse()
    {
        // Arrange
        var messageId = "non-existent-message-id";
        _repositoryMock.Setup(x => x.DeleteAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteGuestbookMessageCommand(messageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        _repositoryMock.Verify(x => x.DeleteAsync(messageId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_WithInvalidIds_StillAttemptsDelete(string invalidId)
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteGuestbookMessageCommand(invalidId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        _repositoryMock.Verify(x => x.DeleteAsync(invalidId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
