using FluentValidation;
using FluentValidation.Results;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;

namespace Pos.BuildingBlocks.Application.Behaviors;

/// <summary>Runs all FluentValidation validators for a command before its handler.</summary>
internal static class ValidationDecorators
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            ValidationError? error = await ValidateAsync(command, validators, cancellationToken);
            return error is null
                ? await inner.HandleAsync(command, cancellationToken)
                : Result.Failure<TResponse>(error);
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            ValidationError? error = await ValidateAsync(command, validators, cancellationToken);
            return error is null
                ? await inner.HandleAsync(command, cancellationToken)
                : Result.Failure(error);
        }
    }

    private static async Task<ValidationError?> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators,
        CancellationToken cancellationToken)
    {
        IValidator<TCommand>[] validatorArray = validators.ToArray();
        if (validatorArray.Length == 0)
        {
            return null;
        }

        ValidationResult[] results = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(command, cancellationToken)));

        Error[] errors = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        return errors.Length == 0 ? null : new ValidationError(errors);
    }
}
