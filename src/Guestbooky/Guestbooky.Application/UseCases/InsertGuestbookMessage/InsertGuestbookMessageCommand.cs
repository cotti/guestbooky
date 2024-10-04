using FluentValidation;
using Guestbooky.Application.Interfaces;
using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Domain.Entities.Message;
using MediatR;
using MediatR.Pipeline;

namespace Guestbooky.Application.UseCases.InsertGuestMessage;

#region Types
public record InsertGuestbookMessageCommand(string Author, string Message, string CaptchaResponse) : IRequest<InsertGuestbookMessageResult>;
public record InsertGuestbookMessageResult(bool Success, string? ErrorMessage);
#endregion

public class InsertGuestbookMessageCommandHandler : IRequestHandler<InsertGuestbookMessageCommand, InsertGuestbookMessageResult>
{
    private readonly IGuestbookMessageRepository _guestbookMessageRepository;

    public InsertGuestbookMessageCommandHandler(IGuestbookMessageRepository guestbookMessageRepository)
    {
        _guestbookMessageRepository = guestbookMessageRepository;
    }

    public async Task<InsertGuestbookMessageResult> Handle(InsertGuestbookMessageCommand request, CancellationToken cancellationToken)
    {
        var newMessage = GuestbookMessage.Create(request.Author, request.Message);
        await _guestbookMessageRepository.AddAsync(newMessage, cancellationToken);
        return new InsertGuestbookMessageResult(true, string.Empty);
    }
}

#region Validation

/// <summary>
/// Oh, this is where the interesting tidbit from the summary in <seealso cref="Behaviors.ValidationBehavior{T,U}"/> is.
/// <para/>
/// This class inherits from FluentValidation's <see cref="AbstractValidator{T}"/>, so all that's needed is to write the rules
/// in the constructor. Very neat.
/// <para/>
/// This guarantees we have independent layers helping keep integrity on the (rather anemic) domain model.
/// Of course, there are improvements here and there we can think about.
/// If a rule fails here, we drop further below in this source file, to <seealso cref="InsertGuestbookMessageCommandExceptionHandler"/>.
/// </summary>
public class InsertGuestbookMessageCommandValidator : AbstractValidator<InsertGuestbookMessageCommand>
{
    private readonly ICaptchaVerifier _captchaVerifier;

    public InsertGuestbookMessageCommandValidator(ICaptchaVerifier captchaVerifier)
    {
        _captchaVerifier = captchaVerifier;

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("An author is required.")
            .MaximumLength(200).WithMessage("The author field should not exceed 200 characters.");
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("A message is required.")
            .MaximumLength(4096).WithMessage("The message field should not exceed 4096 characters.");
        RuleFor(x => x.CaptchaResponse)
            .NotEmpty().WithMessage("A captcha response must be sent with the payload.")
            .MustAsync(async (captchaResponse, cancellationToken) => await _captchaVerifier.VerifyAsync(captchaResponse, cancellationToken))
            .WithMessage("The captcha challenge response was not accepted.");
    }
}

public class InsertGuestbookMessageCommandExceptionHandler : IRequestExceptionHandler<InsertGuestbookMessageCommand, InsertGuestbookMessageResult, ValidationException>
{
    public Task Handle(InsertGuestbookMessageCommand request, ValidationException exception, RequestExceptionHandlerState<InsertGuestbookMessageResult> state, CancellationToken cancellationToken)
    {
        state.SetHandled(new InsertGuestbookMessageResult(false, string.Join(' ', exception.Errors)));
        return Task.CompletedTask;
    }
}

#endregion