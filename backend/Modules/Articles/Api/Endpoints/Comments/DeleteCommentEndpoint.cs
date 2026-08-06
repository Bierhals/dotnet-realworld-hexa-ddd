using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.DeleteComment;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Comments;

internal sealed class DeleteCommentEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#delete-comment\">Conduit Spec for delete comment endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article that you want to delete a comment for")] string slug,
        [Description("ID of the comment you want to delete")] int id,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCommentCommand { Slug = slug, CommentId = id };

        var result = await mediator.Send(command, cancellationToken);

        return result.IsError
            ? result.Errors.ToProblemResult()
            : TypedResults.NoContent();
    }
}
