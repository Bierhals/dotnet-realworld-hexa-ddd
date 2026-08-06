using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.CommentList;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Comments;

internal sealed class ListCommentsEndpoint : IEndpoint
{
    public const string Name = nameof(ListCommentsEndpoint);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("", HandleAsync)
            .AllowAnonymous()
            .WithName(Name)
            .WithSummary("Get comments for an article")
            .WithDescription("Get the comments for an article. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-comments-from-an-article\">Conduit Spec for get comments endpoint</a>")
            .Produces<CommentsEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<CommentsEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article that you want to get comments for")] string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var comments = await mediator.Send(new CommentListQuery { Slug = slug }, cancellationToken);

        return comments.IsError
            ? comments.Errors.ToProblemResult()
            : TypedResults.Ok(CommentEnvelopeFactory.Create(comments.Value));
    }
}
