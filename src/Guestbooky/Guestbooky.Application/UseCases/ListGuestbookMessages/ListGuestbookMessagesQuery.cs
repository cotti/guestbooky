using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Domain.Entities.Message;
using MediatR;

namespace Guestbooky.Application.UseCases.ListGuestbookMessages;

#region Types
public record ListGuestbookMessagesQuery(long Offset) : IRequest<ListGuestbookMessagesQueryResult>;
public record GuestbookMessageQueryResult(string Id, string Author, string Message, DateTimeOffset Timestamp);
public record ListGuestbookMessagesQueryResult(IEnumerable<GuestbookMessageQueryResult> Messages);
#endregion

public class ListGuestbookMessagesQueryHandler : IRequestHandler<ListGuestbookMessagesQuery, ListGuestbookMessagesQueryResult>
{
    private readonly IGuestbookMessageRepository _repository;

    public ListGuestbookMessagesQueryHandler(IGuestbookMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListGuestbookMessagesQueryResult> Handle(
        ListGuestbookMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var messages = await _repository.GetAsync(request.Offset, cancellationToken);
        
        var queryResult = new ListGuestbookMessagesQueryResult(messages.Select(m => new GuestbookMessageQueryResult(m.Id.ToString(), m.Author!, m.Message!, m.Timestamp)));
        return queryResult;
    }
}
