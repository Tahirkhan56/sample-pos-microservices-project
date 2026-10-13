using Microsoft.AspNetCore.Http;
using Pos.BuildingBlocks.Domain.Results;

namespace Pos.BuildingBlocks.Web.Errors;

/// <summary>Translates <see cref="Result"/> failures into RFC 9457 problem details.</summary>
public static class ResultExtensions
{
    public static IResult ToProblem(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");
        }

        Error error = result.Error;
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        if (error is ValidationError validationError)
        {
            Dictionary<string, string[]> errors = validationError.Errors
                .GroupBy(e => e.Code, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Select(e => e.Description).ToArray(), StringComparer.Ordinal);

            return Results.ValidationProblem(errors, title: error.Description, extensions: CodeExtension(error));
        }

        return Results.Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: statusCode,
            extensions: CodeExtension(error));
    }

    /// <summary>Returns <paramref name="onSuccess"/> for success or a problem response for failure.</summary>
    public static IResult Match<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : result.ToProblem();
    }

    public static IResult Match(this Result result, Func<IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess() : result.ToProblem();
    }

    private static Dictionary<string, object?> CodeExtension(Error error) => new(StringComparer.Ordinal) { ["code"] = error.Code };
}
