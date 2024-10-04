using Guestbooky.Application.UseCases.ListGuestbookMessages;
using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Domain.Entities.Message;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.Application.UseCases;

public class ListGuestbookMessagesQueryHandlerTests
{
    private readonly Mock<IGuestbookMessageRepository> _repositoryMock;
    private readonly ListGuestbookMessagesQueryHandler _handler;

    public ListGuestbookMessagesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGuestbookMessageRepository>();
        _handler = new ListGuestbookMessagesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithMessages_ReturnsList()
    {
        // Arrange
        IEnumerable<GuestbookMessage> messages = new List<GuestbookMessage>
        {
            GuestbookMessage.CreateExisting(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Asdrubal",
                "Hello World",
                DateTimeOffset.UnixEpoch.AddDays(1)),
            GuestbookMessage.CreateExisting(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Godolina",
                "Perdi",
                DateTimeOffset.UnixEpoch.AddDays(2))
        };

        _repositoryMock.Setup(x => x.GetAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(messages);

        var query = new ListGuestbookMessagesQuery(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Messages);
        var messagesList = result.Messages.ToList();
        Assert.Equal(2, messagesList.Count);
        
        Assert.Equal("11111111-1111-1111-1111-111111111111", messagesList[0].Id);
        Assert.Equal("Asdrubal", messagesList[0].Author);
        Assert.Equal("Hello World", messagesList[0].Message);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddDays(1), messagesList[0].Timestamp);

        Assert.Equal("22222222-2222-2222-2222-222222222222", messagesList[1].Id);
        Assert.Equal("Godolina", messagesList[1].Author);
        Assert.Equal("Perdi", messagesList[1].Message);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddDays(2), messagesList[1].Timestamp);
    }

    [Fact]
    public async Task Handle_NoItems_ReturnsEmpty()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var query = new ListGuestbookMessagesQuery(It.IsAny<int>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Messages);
        Assert.Empty(result.Messages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Handle_WithDifferentOffsets_PassesOffset(long offset)
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(offset, It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<GuestbookMessage>());

        var query = new ListGuestbookMessagesQuery(offset);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetAsync(offset, It.IsAny<CancellationToken>()), Times.Once);
    }
}