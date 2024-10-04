using Guestbooky.Domain.Abstractions.Repositories;
using MediatR;

namespace Guestbooky.Application.UseCases.DeleteGuestbookMessage;

#region Types
public record DeleteGuestbookMessageCommand(string Id) : IRequest<DeleteGuestbookMessageResult>;
public record DeleteGuestbookMessageResult(bool Success);
#endregion

public class DeleteGuestbookMessageCommandHandler : IRequestHandler<DeleteGuestbookMessageCommand, DeleteGuestbookMessageResult>
{
    private readonly IGuestbookMessageRepository _guestbookMessageRepository;

    public DeleteGuestbookMessageCommandHandler(IGuestbookMessageRepository guestbookMessageRepository)
    {
        _guestbookMessageRepository = guestbookMessageRepository;
    }

    public async Task<DeleteGuestbookMessageResult> Handle(DeleteGuestbookMessageCommand request, CancellationToken cancellationToken)
    {
        var result = await _guestbookMessageRepository.DeleteAsync(request.Id, cancellationToken);
        return new DeleteGuestbookMessageResult(result);
    }
}