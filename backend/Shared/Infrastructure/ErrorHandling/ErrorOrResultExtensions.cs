using System.Collections.Generic;
using System.Linq;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Conduit.Shared.Infrastructure.ErrorHandling;

public static class ErrorOrResultExtensions
{
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
