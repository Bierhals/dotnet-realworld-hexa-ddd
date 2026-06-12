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

        comments.MapPost("", CreateCommentAsync);
        comments.MapGet("", ListCommentsAsync).AllowAnonymous();
        comments.MapDelete("{id:int}", DeleteCommentAsync);

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
