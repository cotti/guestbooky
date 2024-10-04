using FluentValidation;
using MediatR;

namespace Guestbooky.Application.Behaviors;

/// <summary>
/// This class provides a concrete implementation of the <see cref="IPipelineBehavior{T,K}"/> interface from MediatR.
/// Clear enough, of course, but what for?
/// <para/>
/// When you want to have some sort of validation of a Command's parameters, in order to avoid doing that
/// in <c>Handle()</c>, you build plumbing that does the happy path and the... other paths, for you.
/// <para/>
/// Isn't it nice? Yes. Isn't it also a hefty dose of over-engineering for a pretty small project? Yes, it is!
/// <para/>
/// But let's not lose track of the personal backscratcher ethos. We're here to show and learn too.
/// <para/>
/// When a command pops up, this <see cref="IPipelineBehavior{T,K}"/> will act as a middleware, or filter in .NET lingo.
/// It will be loaded with any <see cref="IValidator"/> found during initialization, and it will search for an <see cref="IValidator"/>
/// for that <typeparamref name="TRequest"/>.
/// <para/>
/// (You will notice I'm actually using FluentValidation's <see cref="AbstractValidator{T}"/>, instead of implementing <see cref="IValidator"/>;
/// It helped quite a bit and looks rather nice. It is also over-engineering.)
/// <para/>
/// Then, if it finds any validation errors, it throws a <see cref="ValidationException"/>. Else, the pipeline goes on.
/// <para/>
/// The catch here is that something should catch it. in order to keep the presentation layer clean from Application layer behavior like this,
/// we can set up an <see cref="MediatR.Pipeline.IRequestExceptionHandler{TCommand,TResult,TException}"/> that will join the plumbing to continue handling
/// from the point the exception occurs, and more refined treatment can be implemented then.
/// <para/>
/// That was a trip to figure out from scratch. But it is pretty interesting.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToArray();

        if (failures.Length != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}