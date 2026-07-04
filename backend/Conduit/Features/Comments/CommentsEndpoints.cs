using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Comments;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder MapCommentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var comments = endpoints.MapGroup("/articles/{slug}/comments")
            .WithTags("Comments")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtIssuerOptions.Schemes });

        comments.MapPost("", CreateCommentAsync)
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#add-comments-to-an-article\">Conduit Spec for add comment endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        comments.MapGet("", ListCommentsAsync)
            .AllowAnonymous()
            .WithSummary("Get comments for an article")
            .WithDescription("Get the comments for an article. Auth is optional<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-comments-from-an-article\">Conduit Spec for get comments endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
        comments.MapDelete("{id:int}", DeleteCommentAsync)
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#delete-comment\">Conduit Spec for delete comment endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static Task<CommentEnvelope> CreateCommentAsync(
        ICommandHandler<Create.Command, CommentEnvelope> commandHandler,
        [Required] string slug,
        Create.Model model,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Create.Command(model, slug), cancellationToken);

    private static Task<CommentsEnvelope> ListCommentsAsync(
        IQueryHandler<List.Query, CommentsEnvelope> queryHandler,
        [Required] string slug,
        CancellationToken cancellationToken
    ) => queryHandler.Handle(new List.Query(slug), cancellationToken);

    private static Task DeleteCommentAsync(
        ICommandHandler<Delete.Command> commandHandler,
        [Required] string slug,
        int id,
        CancellationToken cancellationToken
    ) => commandHandler.Handle(new Delete.Command(slug, id), cancellationToken);
}
