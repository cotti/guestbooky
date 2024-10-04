using Guestbooky.Domain.Abstractions.Repositories;
using MediatR;

namespace Guestbooky.Application.UseCases.CountGuestbookMessages;

#region Types
public record CountGuestbookMessagesQuery() : IRequest<CountGuestbookMessagesQueryResult>;
public record CountGuestbookMessagesQueryResult(long Amount);
#endregion

public class CountGuestbookMessagesQueryHandler : IRequestHandler<CountGuestbookMessagesQuery, CountGuestbookMessagesQueryResult>
{
    private readonly IGuestbookMessageRepository _repository;

    public CountGuestbookMessagesQueryHandler(IGuestbookMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<CountGuestbookMessagesQueryResult> Handle(
        CountGuestbookMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var messageAmount = await _repository.CountAsync(cancellationToken);

        return new CountGuestbookMessagesQueryResult(messageAmount);
    }
}
