using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using MediatR;
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
        IMediator mediator,
        [Required] string slug,
        Create.Model model,
        CancellationToken cancellationToken
    ) => mediator.Send(new Create.Command(model, slug), cancellationToken);

    private static Task<CommentsEnvelope> ListCommentsAsync(
        IMediator mediator,
        [Required] string slug,
        CancellationToken cancellationToken
    ) => mediator.Send(new List.Query(slug), cancellationToken);

    private static Task DeleteCommentAsync(
        IMediator mediator,
        [Required] string slug,
        int id,
        CancellationToken cancellationToken
    ) => mediator.Send(new Delete.Command(slug, id), cancellationToken);
}
