using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Features.Comments;

public static class CommentsEndpoints
{
    private const string CommentRouteName = "Comment";

    // No GET endpoint exists for a single comment (only DELETE), but the
    // route name is still used to build the Location header for newly
    // created comments, since it uniquely identifies the resource's address.
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
            .WithName(CommentRouteName)
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#delete-comment\">Conduit Spec for delete comment endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<Created<CommentEnvelope>> CreateCommentAsync(
        [Required]
        [Description("Slug of the article that you want to create a comment for")]
        string slug,
        [Description("Comment you want to create")]
        Create.Model model,
        ICommandHandler<Create.Command, CommentEnvelope> commandHandler,
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var envelope = await commandHandler.Handle(new Create.Command(model, slug), cancellationToken);
        var location = linkGenerator.GetUriByName(
            httpContext,
            CommentRouteName,
            new { slug, id = envelope.Comment.CommentId }
        );
        return TypedResults.Created(location, envelope);
    }

    private static Task<CommentsEnvelope> ListCommentsAsync(
        [Required]
        [Description("Slug of the article that you want to get comments for")]
        string slug,
        IQueryHandler<List.Query, CommentsEnvelope> queryHandler,
        CancellationToken cancellationToken
    ) => queryHandler.Handle(new List.Query(slug), cancellationToken);

    private static async Task<NoContent> DeleteCommentAsync(
        [Required]
        [Description("Slug of the article that you want to delete a comment for")]
        string slug,
        [Description("ID of the comment you want to delete")]
        int id,
        ICommandHandler<Delete.Command> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new Delete.Command(slug, id), cancellationToken);
        return TypedResults.NoContent();
    }
}
