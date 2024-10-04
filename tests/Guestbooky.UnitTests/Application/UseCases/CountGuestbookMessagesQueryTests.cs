using Guestbooky.Application.UseCases.CountGuestbookMessages;
using Guestbooky.Domain.Abstractions.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.Application.UseCases;

public class CountGuestbookMessagesQueryHandlerTests
{
    private readonly Mock<IGuestbookMessageRepository> _repositoryMock;
    private readonly CountGuestbookMessagesQueryHandler _handler;

    public CountGuestbookMessagesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGuestbookMessageRepository>();
        _handler = new CountGuestbookMessagesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectCount()
    {
        // Arrange
        const long expectedCount = 42;
        _repositoryMock.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedCount);

        var query = new CountGuestbookMessagesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedCount, result.Amount);
        _repositoryMock.Verify(x => x.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}