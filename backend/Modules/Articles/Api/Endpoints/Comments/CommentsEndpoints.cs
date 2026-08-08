using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Comments;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder MapCommentsGroup(this IEndpointRouteBuilder endpoints)
    {
        var comments = endpoints.MapGroup("/articles/{slug}/comments")
            .WithTags("Comments");

        comments.AddEndpoint<CreateCommentEndpoint>();
        comments.AddEndpoint<ListCommentsEndpoint>();
        comments.AddEndpoint<DeleteCommentEndpoint>();

        return endpoints;
    }
}
