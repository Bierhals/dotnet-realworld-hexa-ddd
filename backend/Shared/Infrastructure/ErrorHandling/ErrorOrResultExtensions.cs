using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Conduit.Shared.Infrastructure.ErrorHandling;

public static class ErrorOrResultExtensions
{
    public static Results<Ok<TValue>, ProblemHttpResult> ToOkResult<TValue>(this ErrorOr<TValue> errorOr) =>
        errorOr.Match<Results<Ok<TValue>, ProblemHttpResult>>(
            value => TypedResults.Ok(value),
            errors => errors.ToProblemResult());

    public static async Task<Results<Ok<TValue>, ProblemHttpResult>> ToOkResult<TValue>(this Task<ErrorOr<TValue>> errorOrTask) =>
        (await errorOrTask).ToOkResult();

    public static Results<Created<TValue>, ProblemHttpResult> ToCreatedResult<TValue>(this ErrorOr<TValue> errorOr, Func<TValue, string?> locationFactory) =>
        errorOr.Match<Results<Created<TValue>, ProblemHttpResult>>(
            value => TypedResults.Created(locationFactory(value), value),
            errors => errors.ToProblemResult());

    public static async Task<Results<Created<TValue>, ProblemHttpResult>> ToCreatedResult<TValue>(this Task<ErrorOr<TValue>> errorOrTask, Func<TValue, string?> locationFactory) =>
        (await errorOrTask).ToCreatedResult(locationFactory);

    public static Results<NoContent, ProblemHttpResult> ToNoContentResult(this ErrorOr<Success> errorOr) =>
        errorOr.Match<Results<NoContent, ProblemHttpResult>>(
            _ => TypedResults.NoContent(),
            errors => errors.ToProblemResult());

    public static async Task<Results<NoContent, ProblemHttpResult>> ToNoContentResult(this Task<ErrorOr<Success>> errorOrTask) =>
        (await errorOrTask).ToNoContentResult();

    public static ProblemHttpResult ToProblemResult(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        var firstError = errors[0];
        return firstError.Type switch
        {
            ErrorType.Validation => ToValidationProblemResult(errors),
            ErrorType.NotFound => TypedResults.Problem(firstError.Description, statusCode: StatusCodes.Status404NotFound, type: firstError.Code),
            ErrorType.Conflict => TypedResults.Problem(firstError.Description, statusCode: StatusCodes.Status409Conflict, type: firstError.Code),
            ErrorType.Unauthorized => TypedResults.Problem(firstError.Description, statusCode: StatusCodes.Status401Unauthorized, type: firstError.Code),
            ErrorType.Forbidden => TypedResults.Problem(firstError.Description, statusCode: StatusCodes.Status403Forbidden, type: firstError.Code),
            _ => TypedResults.Problem(firstError.Description, statusCode: StatusCodes.Status500InternalServerError, type: firstError.Code),
        };
    }

    private static ProblemHttpResult ToValidationProblemResult(List<Error> errors)
    {
        var errorsByCode = errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        var problemDetails = new HttpValidationProblemDetails(errorsByCode)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
        };

        return TypedResults.Problem(problemDetails);
    }
}
