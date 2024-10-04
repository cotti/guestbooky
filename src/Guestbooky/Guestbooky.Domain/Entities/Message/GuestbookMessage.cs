namespace Guestbooky.Domain.Entities.Message;

/// <summary>
/// Rather slim! Not very DDD'd of me. Worked well enough though, but it's more of a DTO...?
/// Quick, think of weird stuff to do in a... simple guestbook manager!
/// </summary>
public record GuestbookMessage
{
    public required Guid Id { get; init; }
    public required string Author { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    private GuestbookMessage() { }

    public static GuestbookMessage Create(string author, string message)
    {
        return new GuestbookMessage
        {
            Id = Guid.NewGuid(),
            Author = author,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    public static GuestbookMessage CreateExisting(Guid id, string author, string message, DateTimeOffset timestamp)
    {
        return new GuestbookMessage
        {
            Id = id,
            Author = author,
            Message = message,
            Timestamp = timestamp
        };
    }
}
