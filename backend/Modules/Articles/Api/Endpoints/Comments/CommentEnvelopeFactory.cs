using System.Collections.Generic;
using System.Linq;
using Conduit.Articles.Api.Endpoints.Articles;
using Conduit.Articles.Application;

namespace Conduit.Articles.Api.Endpoints.Comments;

internal static class CommentEnvelopeFactory
{
    public static CommentResponse Create(CommentReadModel comment) => new()
    {
        Id = comment.Id,
        Body = comment.Body,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        Author = ArticleEnvelopeFactory.Create(comment.Author),
    };

    public static CommentsEnvelope Create(IReadOnlyCollection<CommentReadModel> comments) =>
        new([.. comments.Select(Create)]);
}
