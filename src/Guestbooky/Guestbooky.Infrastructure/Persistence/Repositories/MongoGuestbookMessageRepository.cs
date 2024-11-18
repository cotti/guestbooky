using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Domain.Entities.Message;
using Guestbooky.Infrastructure.Persistence.DTOs;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Guestbooky.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// First time ever using NoSQL here. I'm still not sure if I fell into any big anti-patterns.
    /// As someone much more used to SQL lingo, this feels a bit weird but it is enjoyable.
    /// Could use more robust error handling but I've yet to see it not working.
    /// </summary>
    public class MongoGuestbookMessageRepository : IGuestbookMessageRepository
    {
        private const string COLLECTION_NAME = "GuestbookMessages";

        private readonly IMongoCollection<GuestbookMessageDto> _messages;
        private readonly ILogger<MongoGuestbookMessageRepository> _logger;

        public MongoGuestbookMessageRepository(IMongoDatabase database, ILogger<MongoGuestbookMessageRepository> logger)
        {
            _messages = database.GetCollection<GuestbookMessageDto>(COLLECTION_NAME);
            _logger = logger;
        }

        public async Task AddAsync(GuestbookMessage message, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Adding guestbook entry.");

            var messageDto = MapToDto(message);
            await _messages.InsertOneAsync(messageDto, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Removing guestbook entry. Id: {id}");

            var result = await _messages.DeleteOneAsync(m => m.Id == id, cancellationToken: cancellationToken);

            if (result.DeletedCount != 1)
                _logger.LogError($"Deleted count is different from 1. Value: {result.DeletedCount}");
            if (!result.IsAcknowledged)
                _logger.LogError("Deletion was not acknowledged.");

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<IEnumerable<GuestbookMessage>> GetAsync(long offset, CancellationToken cancellationToken)
        {
            if (offset < 0) offset = 0;
            _logger.LogDebug("Acquiring guestbook entries.");
            var messageDtos = await _messages.Find(_ => true)
                .SortBy(x => x.Timestamp)
                .Skip((int?)offset)
                .Limit(10)
                .ToCursorAsync(cancellationToken);
            return messageDtos.ToEnumerable().Select(MapToDomainModel).ToArray();
        }

        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting amount of messages stored.");
            return await _messages.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
        }

        private static GuestbookMessage MapToDomainModel(GuestbookMessageDto dto)
        {
            return GuestbookMessage.CreateExisting(
                Guid.Parse(dto.Id),
                dto.Author,
                dto.Message,
                new DateTimeOffset(dto.Timestamp, TimeSpan.Zero)
            );
        }

        private static GuestbookMessageDto MapToDto(GuestbookMessage message)
        {
            return new GuestbookMessageDto
            {
                Id = message.Id.ToString(),
                Author = message.Author,
                Message = message.Message,
                Timestamp = message.Timestamp.UtcDateTime
            };
        }
    }
}
