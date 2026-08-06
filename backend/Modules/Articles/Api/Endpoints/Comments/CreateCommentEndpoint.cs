using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.CreateComment;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Comments;

internal sealed class CreateCommentEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#add-comments-to-an-article\">Conduit Spec for add comment endpoint</a>")
            .Produces<CommentEnvelope>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Created<CommentEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article that you want to create a comment for")] string slug,
        [Description("Comment you want to create")] CreateCommentRequest request,
        ICqrsMediator mediator,
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand
        {
            Slug = slug,
            Body = request.Comment.Body,
        };

        var comment = await mediator.Send(command, cancellationToken);
        if (comment.IsError)
        {
            return comment.Errors.ToProblemResult();
        }

        var location = linkGenerator.GetPathByName(httpContext, ListCommentsEndpoint.Name, new { slug });

        return TypedResults.Created(location, new CommentEnvelope(CommentEnvelopeFactory.Create(comment.Value)));
    }
}
